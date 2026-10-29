using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.CandidatePool;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.Scenarios
{
    public class CreateScenarioModel : PageModel
    {
        private readonly IScenarioAppService _scenarioAppService;
        private readonly ICandidatePoolAppService _candidatePoolAppService;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IdentityUserManager _userManager;
        private readonly ILogger<CreateScenarioModel> _logger;

        public CreateScenarioModel(
            IScenarioAppService scenarioAppService,
            ICandidatePoolAppService candidatePoolAppService,
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IdentityUserManager userManager,
            ILogger<CreateScenarioModel> logger)
        {
            _scenarioAppService = scenarioAppService;
            _candidatePoolAppService = candidatePoolAppService;
            _userRepository = userRepository;
            _currentUser = currentUser;
            _userManager = userManager;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnGetCandidatePoolsAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            var result = await _candidatePoolAppService.ListAsync(scenid: null, importing: false, userId);

            if (result?.Libraries == null)
                return new JsonResult(new { libraries = new List<object>() });

            // List every library that is not yet linked to any scenario (shared or not)
            // so the user can pick one to link. A newly created library has
            // ScenarioCount == 0, so it appears here until it gets attached to a scenario.
            var available = result.Libraries
                .Where(l => l.ScenarioCount == 0)
                .ToList();

            return new JsonResult(new { libraries = available });
        }

        public async Task<IActionResult> OnPostCheckScenarioNameAsync(string attemptedname)
        {
            var isAvailable = await _scenarioAppService.CheckScenarioNameAsync(attemptedname);

            if (isAvailable)
                return new JsonResult(new { success = true });

            return BadRequest("name already exists.");
        }

        public async Task<IActionResult> OnPostCheckLibraryNameAsync(string libraryname, string libraryid)
        {
            var isValid = await _candidatePoolAppService.CheckLibraryNameAsync(libraryname, libraryid);

            if (!isValid)
                return new JsonResult(new { error = "Name already exists" }) { StatusCode = 400 };

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostCreateLibraryAsync([FromBody] CreateCandidatePoolDto libraryViewModel)
        {
            if (libraryViewModel == null || string.IsNullOrWhiteSpace(libraryViewModel.Name))
                return new JsonResult(new { error = "Treatment Pool Library name is required." }) { StatusCode = 400 };

            if (string.IsNullOrWhiteSpace(libraryViewModel.Description))
                return new JsonResult(new { error = "Treatment Pool Library description is required." }) { StatusCode = 400 };

            var userId = await GetCurrentUserIdAsync();
            try
            {
                var libid = await _candidatePoolAppService.CreateNewLibrary(libraryViewModel, userId);
                return new JsonResult(libid);
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { error = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostCreateScenarioAsync([FromBody] CreateScenarioDto input)
        {
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

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            // Resolve the current ABP identity user (AbpUsers) by id, falling back to email.
            if (_currentUser.Id.HasValue)
                return _currentUser.Id.Value;

            var email = _currentUser.Email
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Guid.Empty;

            var user = await _userManager.FindByEmailAsync(email);
            return user?.Id ?? Guid.Empty;
        }
    }
}
