using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Web.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.EditScenario
{
    /// <summary>
    /// Shared base for the EditScenario section pages (Scenario Settings,
    /// Analysis Method, Parameters, Budget Constraints). Holds the scenario
    /// id/name used by the shared left-side menu and loads the scenario header.
    /// </summary>
    [Authorize]
    public abstract class EditScenarioPageModelBase : PageModel
    {
        protected readonly IScenarioAppService ScenarioAppService;
        private readonly ICurrentUser _currentUser;
        private readonly ITourGuideSeenService _tourGuideSeenService;

        protected EditScenarioPageModelBase(
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService)
        {
            ScenarioAppService = scenarioAppService;
            _currentUser = currentUser;
            _tourGuideSeenService = tourGuideSeenService;
        }

        public int Scenid { get; set; }
        public string ScenarioName { get; set; }
        public string ScenarioNotes { get; set; }
        public bool Locked { get; set; }
        public int? FirstYear { get; set; }
        public int? LastYear { get; set; }

        /// <summary>Key of the active left-menu item (matches the page name).</summary>
        public abstract string ActiveMenu { get; }

        protected async Task LoadScenarioAsync(int scenid)
        {
            Scenid = scenid;

            try
            {
                var settings = await ScenarioAppService.GetScenarioSettingsAsync(scenid);
                ScenarioName = settings.ScenarioName;
                ScenarioNotes = settings.ScenarioNotes;
                Locked = settings.Locked;
                FirstYear = settings.FirstYear;
                LastYear = settings.LastYear;
            }
            catch (UserFriendlyException)
            {
                // Scenario not found — leave the header fields at their defaults.
            }
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
    }
}
