using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Services;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.Projects
{
    [Authorize]
    public abstract class AnalysisPageModelBase : PageModel
    {
        private readonly ICurrentUser _currentUser;
        private readonly ITourGuideSeenService _tourGuideSeenService;
        private readonly IProjectRepository _projectRepository;

        protected AnalysisPageModelBase(ICurrentUser currentUser, ITourGuideSeenService tourGuideSeenService, IProjectRepository projectRepository)
        {
            _currentUser = currentUser;
            _tourGuideSeenService = tourGuideSeenService;
            _projectRepository = projectRepository;
        }

        public int Scenid { get; set; }
        public string ScenarioName { get; set; }
        public string ScenarioNotes { get; set; }
        public bool FromScenario { get; set; }

        // Defaults true so a lookup error never wrongly disables the Maps nav link.
        public bool HasProjects { get; set; } = true;

        public abstract string ActiveMenu { get; }

        protected async Task LoadScenarioAsync(IScenarioAppService scenarioAppService, int scenid)
        {
            Scenid = scenid;
            var scenario = (await scenarioAppService.GetAllAsync(""))
                .FirstOrDefault(s => s.Id == scenid);
            if (scenario != null)
            {
                ScenarioName = scenario.ScenarioName;
                ScenarioNotes = scenario.Notes;
            }

            try
            {
                HasProjects = await _projectRepository.HasProjectsAsync(scenid);
            }
            catch
            {
                HasProjects = true;
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
