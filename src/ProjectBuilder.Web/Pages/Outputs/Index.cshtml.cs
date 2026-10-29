using ProjectBuilder.Models.ProjectCandidate;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.Outputs
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IScenarioAppService _scenarioAppService;
        public string ScenarioName { get; set; }  

        public IndexModel(IScenarioAppService scenarioAppService)
        {
            _scenarioAppService = scenarioAppService;
        }

        public async Task OnGetAsync(Guid? scenarioId)
        {
            if (scenarioId.HasValue)
            {
                var scenario = await _scenarioAppService.GetAsync(scenarioId.Value);
                ScenarioName = scenario?.ScenarioName;
            }
        }
    }
}
