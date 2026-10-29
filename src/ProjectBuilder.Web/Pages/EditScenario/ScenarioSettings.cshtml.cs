using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Web.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.EditScenario
{
    public class ScenarioSettingsModel : EditScenarioPageModelBase
    {
        public ScenarioSettingsModel(
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService)
            : base(scenarioAppService, currentUser, tourGuideSeenService)
        {
        }

        public override string ActiveMenu => "ScenarioSettings";

        [BindProperty]
        public ScenarioSettingsInput Input { get; set; }

        public ScenarioSettingsDto Settings { get; set; }
        public string ErrorMessage { get; set; }
        public bool Saved { get; set; }

        public async Task OnGetAsync(int scenid)
        {
            // Read the one-time "saved" flag from TempData (set on the redirect after a
            // successful POST) instead of a `saved=true` query string flag — a query string
            // flag stays in the URL, so refreshing the page kept re-showing the banner forever.
            // TempData is cleared once read, so a refresh of the same URL won't show it again.
            Saved = TempData["ScenarioSettingsSaved"] != null;
            await LoadAsync(scenid);
        }

        public async Task<IActionResult> OnPostAsync(int scenid)
        {
            Scenid = scenid;
            try
            {
                await ScenarioAppService.UpdateScenarioSettingsAsync(scenid, new ScenarioSettingsDto
                {
                    ScenarioName = Input?.ScenarioName,
                    FirstYear = Input?.FirstYear,
                    LastYear = Input?.LastYear,
                    IsShared = Input?.IsShared ?? false
                });

                TempData["ScenarioSettingsSaved"] = true;
                return RedirectToPage(new { scenid });
            }
            catch (UserFriendlyException ex)
            {
                ErrorMessage = ex.Message;
                await LoadAsync(scenid);
                return Page();
            }
        }

        private async Task LoadAsync(int scenid)
        {
            Settings = await ScenarioAppService.GetScenarioSettingsAsync(scenid);

            Scenid = scenid;
            ScenarioName = Settings?.ScenarioName;
            ScenarioNotes = Settings?.ScenarioNotes;
            Locked = Settings?.Locked ?? false;
            FirstYear = Settings?.FirstYear;
            LastYear = Settings?.LastYear;

            // Only seed the form on GET (and when not posting back invalid input).
            Input ??= new ScenarioSettingsInput
            {
                ScenarioName = Settings?.ScenarioName,
                FirstYear = Settings?.FirstYear,
                LastYear = Settings?.LastYear,
                IsShared = Settings?.IsShared ?? false
            };
        }

        public class ScenarioSettingsInput
        {
            [Required(ErrorMessage = "Scenario name is required.")]
            [MaxLength(255)]
            public string ScenarioName { get; set; }

            [Required(ErrorMessage = "First year is required.")]
            public int? FirstYear { get; set; }

            [Required(ErrorMessage = "Last year is required.")]
            public int? LastYear { get; set; }

            public bool IsShared { get; set; }
        }
    }
}
