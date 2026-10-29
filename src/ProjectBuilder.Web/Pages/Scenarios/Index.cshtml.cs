
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.DataAccess;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models.CandidatePool;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using ProjectBuilder.Web.Jobs;
using ProjectBuilder.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.Scenarios
{
    public class IndexModel : PageModel
    {
        private readonly IScenarioAppService _scenarioAppService;
        private readonly ICandidatePoolAppService _candidatePoolAppService;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;
        private readonly ProjectBuilderDbContext _context;
        private readonly IProjectTreatmentRepository _projectTreatment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ILogger<IndexModel> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly ITourGuideSeenService _tourGuideSeenService;

        public IndexModel(
            IScenarioAppService scenarioAppService,
            ICandidatePoolAppService candidatePoolAppService,
            IUserRepository userRepository,
            IConfiguration config,
            ProjectBuilderDbContext context,
            IProjectTreatmentRepository projectTreatment,
            IHttpContextAccessor httpContextAccessor,
            IBackgroundJobClient backgroundJobClient,
            IPermissionChecker permissionChecker,
            ILogger<IndexModel> logger,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService)
        {
            _scenarioAppService = scenarioAppService;
            _candidatePoolAppService = candidatePoolAppService;
            _userRepository = userRepository;
            _config = config;
            _context = context;
            _projectTreatment = projectTreatment;
            _httpContextAccessor = httpContextAccessor;
            _backgroundJobClient = backgroundJobClient;
            _permissionChecker = permissionChecker;
            _logger = logger;
            _currentUser = currentUser;
            _tourGuideSeenService = tourGuideSeenService;
        }

        public async Task<IActionResult> OnGetTourSeenAsync(string tourKey)
        {
            var userId = _currentUser.Id;
            var seen = userId == null || string.IsNullOrWhiteSpace(tourKey)
                || await _tourGuideSeenService.IsSeenAsync(userId.Value, tourKey);
            return new JsonResult(new { seen });
        }

        public async Task<IActionResult> OnPostMarkTourSeenAsync(string tourKey)
        {
            var userId = _currentUser.Id;
            if (userId != null && !string.IsNullOrWhiteSpace(tourKey))
            {
                await _tourGuideSeenService.MarkSeenAsync(userId.Value, tourKey);
            }
            return new JsonResult(new { success = true });
        }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; } = string.Empty;

        public ScenarioDto ScenarioVm { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsUserAuthenticated())
                return RedirectToPage("/Index");

            ScenarioVm = await _scenarioAppService.GetScenariosAsync(SearchString);

            foreach (var scenario in ScenarioVm.Scenarios)
            {
                scenario.BridgesCount = _context.CustomTreatments
                    .Count(t => t.LibraryId == scenario.LibraryId && t.AssetType == "B");

                scenario.PavementsCount = _context.CustomTreatments
                    .Count(t => t.LibraryId == scenario.LibraryId && t.AssetType == "P");

                var isStale = await _projectTreatment.CheckStale(scenario.Id);
                if (isStale.GetValueOrDefault() && !scenario.Locked.GetValueOrDefault())
                {
                    scenario.Notes = "Scenario treatments have been modified. To apply the changes, please perform a scenario re-run.";
                    scenario.ReRun = true;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnGetHasTreatmentsAsync(int scenarioId)
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Run))
                return Forbid();

            if (scenarioId <= 0)
                return BadRequest(new { error = "Invalid scenario ID." });

            var scenario = await _context.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
                return NotFound(new { error = "Scenario not found." });

            return new JsonResult(new { hasTreatments = ScenarioHasTreatments(scenario) });
        }

        private bool ScenarioHasTreatments(ProjectBuilder.Entities.Scenario scenario)
        {
            if (scenario?.LibraryId == null)
                return false;
            return _context.CustomTreatments.Any(t => t.LibraryId == scenario.LibraryId);
        }

        public async Task<IActionResult> OnGetScenariosAsync()
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Default))
                return Forbid();

            var result = await _scenarioAppService.GetScenariosAsync();
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostCreateScenarioAsync([FromBody] CreateScenarioDto input)
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Create))
                return Forbid();

            if (input == null)
                return BadRequest(new { error = "Scenario request is invalid." });

            try
            {
                var result = await _scenarioAppService.CreateNewScenarioAsync(input);
                return new JsonResult(new
                {
                    message = "Scenario created successfully.",
                    scenarioId = result.Id,
                    jobId = result.Id.ToString()
                });
            }
            catch (UserFriendlyException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create scenario.");
                return StatusCode(500, new { error = "Failed to create the scenario. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostRunScenarioAsync(int scenarioId)
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Run))
                return Forbid();

            if (scenarioId <= 0)
                return BadRequest(new { error = "Invalid scenario ID." });

            var scenario = await _context.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
                return NotFound(new { error = "Scenario not found." });

            if (scenario.Locked == true)
            {
                return StatusCode(409, new
                {
                    error = $"Scenario \"{scenario.ScenarioName}\" is already running. Please wait for it to finish."
                });
            }

            if (!ScenarioHasTreatments(scenario))
            {
                return BadRequest(new
                {
                    error = "No treatments are in this scenario.",
                    noTreatments = true
                });
            }

            if (ScenarioRunJobHelper.HasPendingJob(scenarioId))
            {
                return StatusCode(409, new
                {
                    error = $"Scenario \"{scenario.ScenarioName}\" already has a run queued or in progress."
                });
            }

            var requestedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(requestedBy))
                return Unauthorized();

            var jobId = _backgroundJobClient.Enqueue<ScenarioRunnerJob>(
                job => job.RunAsync(scenarioId, requestedBy));

            return new JsonResult(new { success = true, jobId });
        }

        public IActionResult OnGetJobStatus(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return new JsonResult(new { status = "NotFound" });

            try
            {
                var connection = JobStorage.Current.GetConnection();
                var jobData = connection.GetJobData(jobId);

                if (jobData == null)
                    return new JsonResult(new { status = "NotFound" });

                var stateData = connection.GetStateData(jobId);
                string? message = stateData?.Reason;

                if (string.Equals(jobData.State, "Failed", StringComparison.OrdinalIgnoreCase)
                    && stateData?.Data != null
                    && stateData.Data.TryGetValue("ExceptionMessage", out var exMsg)
                    && !string.IsNullOrWhiteSpace(exMsg))
                {
                    message = exMsg;
                }

                return new JsonResult(new { status = jobData.State, message });
            }
            catch (SqlException ex)
            {
                // Job-status DB is under heavy load from the scenario run itself (same connection/DB).
                // Report "Unknown" so the client keeps polling instead of surfacing a spurious failure.
                _logger.LogWarning("Job status check timed out/failed for job {JobId}; will retry on next poll. {Error}", ProjectBuilder.Web.Common.LogSanitizer.SanitizeForLog(jobId), ProjectBuilder.Web.Common.LogSanitizer.SanitizeForLog(ex.Message));
                return new JsonResult(new { status = "Unknown" });
            }
        }

        public async Task<IActionResult> OnPostDeleteScenarioAsync(int scenarioId)
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Delete))
                return Forbid();

            var success = await _scenarioAppService.DeleteScenarioAsync(scenarioId);

            if (!success)
                return BadRequest("Could not delete the selected scenario.");

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostCheckScenarioNameAsync(string attemptedName)
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Create))
                return Forbid();

            var isAvailable = await _scenarioAppService.CheckScenarioNameAsync(attemptedName);

            if (isAvailable)
                return new JsonResult(new { success = true });

            return BadRequest("name already exists.");
        }

        public async Task<IActionResult> OnPostCheckScenarioDeletionAsync(int scenId)
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Delete))
                return Forbid();

            var result = await _scenarioAppService.CheckScenarioDeletionAsync(scenId);
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnGetCandidatePoolsAsync(bool isShared = false)
        {
            if (!await _permissionChecker.IsGrantedAsync(ScenarioPermissions.Create))
                return Forbid();

            var result = await _candidatePoolAppService.ListAsync(scenid: null, importing: false);

            if (result?.Libraries == null)
            {
                return new JsonResult(new { libraries = new List<object>() });
            }

            var filtered = isShared
                ? result.Libraries.Where(l => l.IsShared == true).ToList()
                : result.Libraries.ToList();

            return new JsonResult(new { libraries = filtered });
        }

        public bool IsUserAuthenticated()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            return claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated;
        }
    }
}