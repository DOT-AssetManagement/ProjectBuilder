using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.ScenarioFilter.Dtos;
using ProjectBuilder.Web.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.EditScenario
{
    public class AnalysisMethodModel : EditScenarioPageModelBase
    {
        public AnalysisMethodModel(
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService)
            : base(scenarioAppService, currentUser, tourGuideSeenService)
        {
        }

        public override string ActiveMenu => "AnalysisMethod";

        public AnalysisMethodDto Data { get; set; } = new();

        public async Task OnGetAsync(int scenid)
        {
            await LoadScenarioAsync(scenid);
            Data = await ScenarioAppService.GetAnalysisMethodAsync(scenid);
        }

        public async Task<IActionResult> OnGetAttributeValuesAsync(int scenid, string assetType, string attributeName)
        {
            var values = await ScenarioAppService.GetAttributeValuesAsync(scenid, assetType, attributeName);
            return new JsonResult(values);
        }

        public async Task<IActionResult> OnPostSaveCriteriaAsync([FromBody] SaveCriteriaInput input)
        {
            try
            {
                await ScenarioAppService.SaveCriteriaAsync(input.ScenId, input.Filters ?? new List<ScenarioFilterDto>(), input.Notes);
                return new JsonResult(new { success = true });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCheckOutputAsync([FromBody] CheckOutputInput input)
        {
            try
            {
                var count = await ScenarioAppService.GetMatchedTreatmentCountAsync(input.ScenId, input.Filters ?? new List<ScenarioFilterDto>());
                return new JsonResult(new { success = true, count });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public class SaveCriteriaInput
        {
            public int ScenId { get; set; }
            public List<ScenarioFilterDto> Filters { get; set; } = new();
            public string Notes { get; set; }
        }

        public class CheckOutputInput
        {
            public int ScenId { get; set; }
            public List<ScenarioFilterDto> Filters { get; set; } = new();
        }
    }
}
