using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.CandidatePool;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Treatment;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using ProjectBuilder.Web.Jobs;
using ProjectBuilder.Web.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.TreatmentPoolLibraries
{
    public class IndexModel : PageModel
    {
        private readonly ICandidatePoolRepository _candidate;
        private readonly ICandidatePoolAppService _candidatePoolAppService;
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ITourGuideSeenService _tourGuideSeenService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ICandidatePoolRepository candidate,
            ICandidatePoolAppService candidatePoolAppService,
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IBackgroundJobClient backgroundJobClient,
            IPermissionChecker permissionChecker,
            ITourGuideSeenService tourGuideSeenService,
            ILogger<IndexModel> logger)
        {
            _candidate = candidate;
            _candidatePoolAppService = candidatePoolAppService;
            _currentUser = currentUser;
            _userRepository = userRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
            _permissionChecker = permissionChecker;
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

        public CandidatePoolDto Pools { get; set; } = new();
        public Guid CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? scenid, bool? importing)
        {
            if (!IsUserAuthenticated())
                return RedirectToPage("/Account/Login");

            try
            {
                var currentUser = await GetOrCreateUser();
                CurrentUserId = currentUser.Id;
                Pools = await _candidatePoolAppService.ListAsync(scenid, importing, currentUser.Id) ?? new CandidatePoolDto();
            }
            catch
            {
                Pools = new CandidatePoolDto();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateLibraryAsync(CreateCandidatePoolDto libraryViewModel)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Create))
                return Forbid();

            if (string.IsNullOrWhiteSpace(libraryViewModel.Name))
                return BadRequest("Library name is required.");

            if (string.IsNullOrWhiteSpace(libraryViewModel.Description))
                return BadRequest("Library description is required.");

            libraryViewModel.IsEmptyLibrary = true;
            var currentUser = await GetOrCreateUser();

            try
            {
                await _candidatePoolAppService.CreateNewLibrary(libraryViewModel, currentUser.Id);
                TempData["SuccessMessage"] = $"Treatment Pool Library \"{libraryViewModel.Name}\" created successfully.";
            }
            catch (UserFriendlyException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Treatment Pool Library \"{LibraryName}\".", libraryViewModel.Name);
                TempData["ErrorMessage"] = $"Failed to create Treatment Pool Library \"{libraryViewModel.Name}\". Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditLibraryAsync(
            Guid libraryId,
            string libraryname,
            string librarydescription,
            bool isshared)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Edit))
                return Forbid();

            if (string.IsNullOrWhiteSpace(libraryname))
                return BadRequest("Library name is required.");

            if (string.IsNullOrWhiteSpace(librarydescription))
                return BadRequest("Library description is required.");

            var dto = new UpdateCandidatePoolDto
            {
                Id = libraryId,
                Name = libraryname.Trim(),
                Description = librarydescription.Trim(),
                IsShared = isshared
            };

            try
            {
                await _candidatePoolAppService.EditLibraryAsync(dto);
                var message = $"Treatment Pool Library \"{libraryname}\" updated successfully.";
                return new JsonResult(new { success = true, message = message });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { error = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostDeleteLibraryAsync(Guid libraryId)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Delete))
                return Forbid();

            var (deleted, error) = await _candidate.DeactivateCandidatePool(libraryId);
            if (deleted)
            {
                TempData["SuccessMessage"] = "Treatment Pool Library deleted successfully.";
            }
            else
            {
                // Surface the underlying reason (SQL/permission/not-found) — DataManager only
                // logs it via log4net, which isn't wired up, so otherwise it's lost entirely.
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(error)
                    ? "Could not delete the Treatment Pool Library."
                    : $"Could not delete the Treatment Pool Library: {error}";
            }
            return RedirectToPage();
        }

        // Enqueues a background copy job and returns { jobId } immediately.
        public async Task<IActionResult> OnPostCreateSharedLibraryAsync(
            Guid libraryId,
            string libraryname,
            string librarydescription,
            bool isshared)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Copy))
                return Forbid();

            if (string.IsNullOrWhiteSpace(libraryname))
                return BadRequest(new { error = "Library name is required." });

            if (!libraryname.Any(char.IsLetterOrDigit))
                return BadRequest(new { error = "Library name must contain at least one letter or number." });

            if (string.IsNullOrWhiteSpace(librarydescription))
                return BadRequest(new { error = "Library description is required." });

            var dto = new CreateCandidatePoolDto
            {
                Name = libraryname,
                Description = librarydescription,
                IsShared = isshared
            };

            var currentUser = await GetOrCreateUser();
            var jobId = _backgroundJobClient.Enqueue<CopyLibraryJob>(
                job => job.RunAsync(dto, currentUser.Id, libraryId));

            return new JsonResult(new { success = true, jobId });
        }

        // Saves the uploaded file to disk then enqueues a background import job.
        public async Task<IActionResult> OnPostImportTreatmentsAsync([FromForm] ImportTreatmentViewModel data)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Import))
                return Forbid();

            if (!TryValidateModel(data))
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return new JsonResult(new { error = string.Join(" ", errors) }) { StatusCode = 400 };
            }

            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            await using (var stream = new FileStream(filePath, FileMode.Create))
                await data.ExcelFile!.CopyToAsync(stream);

            var jobId = _backgroundJobClient.Enqueue<ImportLibraryJob>(
                job => job.RunAsync(filePath, data.LibraryId!.Value, data.ImportType, data.TabName, data.fromScratch, data.keepAll));

            return new JsonResult(new { success = true, jobId });
        }

        public IActionResult OnGetJobStatus(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return new JsonResult(new { status = "NotFound" });

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

        public async Task<IActionResult> OnPostCheckLibraryNameAsync(string libraryname, string libraryid)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Default))
                return Forbid();

            var isValid = await _candidatePoolAppService.CheckLibraryNameAsync(libraryname, libraryid);
            if (!isValid)
                return new JsonResult(new { error = "Name already exists" }) { StatusCode = 400 };
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostGetLibraryAsync([FromBody] Guid id)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Default))
                return Forbid();

            var library = await _candidatePoolAppService.GetAsync(id);
            if (library == null)
                return NotFound("Library not found.");
            return new JsonResult(new
            {
                id          = library.Id,
                name        = library.Name,
                description = library.Description,
                isShared    = library.IsShared
            });
        }

        public bool IsUserAuthenticated()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            return claimsPrincipal != null && claimsPrincipal.Identity != null && claimsPrincipal.Identity.IsAuthenticated;
        }

        private async Task<Volo.Abp.Identity.IdentityUser> GetOrCreateUser()
        {
            // Resolve the current ABP identity user (AbpUsers) by id, falling back to email.
            Volo.Abp.Identity.IdentityUser user = null;
            if (_currentUser.Id.HasValue)
                user = await _userManager.FindByIdAsync(_currentUser.Id.Value.ToString());

            if (user == null)
            {
                var email = _currentUser.Email
                    ?? HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? HttpContext.User.Identity?.Name;
                if (!string.IsNullOrEmpty(email))
                    user = await _userManager.FindByEmailAsync(email);
            }

            if (user == null)
                throw new Exception("User is not authenticated.");

            return user;
        }
    }
}
