using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models.ProjectTreatment;
using ProjectBuilder.Models.ProjectTreatment.Dtos;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Pages.Projects;
using ProjectBuilder.Web.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.ProjectTreatments
{
    // Scenario-wide "Project Treatments" list — the legacy /ProjectTreatments/List page,
    // linked from the Projects list page. Shows every treatment across every project in
    // the scenario (unlike Index.cshtml, which is scoped to a single project).
    public class ListModel : AnalysisPageModelBase
    {
        private readonly IProjectTreatmentAppService _projectTreatmentAppService;
        private readonly IScenarioAppService _scenarioAppService;

        // Scenario-wide, independent of the cascade, so loaded once up front instead of via ajax.
        public List<byte?> Districts { get; set; } = new();
        public List<int?> Years { get; set; } = new();

        public override string ActiveMenu => "Projects";

        public ListModel(
            IProjectTreatmentAppService projectTreatmentAppService,
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService,
            IProjectRepository projectRepository)
            : base(currentUser, tourGuideSeenService, projectRepository)
        {
            _projectTreatmentAppService = projectTreatmentAppService;
            _scenarioAppService = scenarioAppService;
        }

        public async Task OnGetAsync(int scenid)
        {
            await LoadScenarioAsync(_scenarioAppService, scenid);
            Districts = await _projectTreatmentAppService.GetScenarioDistrictsAsync(scenid);
            Years = await _projectTreatmentAppService.GetScenarioYearsAsync(scenid);
        }

        // Backs the Tabulator grid's remote pagination — one page of rows at a time.
        // [FromQuery] on page/size is required: "page" collides with Razor Pages' own reserved route-value key for page routing, which otherwise wins over the query string and silently binds to the parameter's default.
        public async Task<IActionResult> OnGetTreatmentsPageAsync(
            int scenid, string assetType, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, string sortDir, [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            var result = await _projectTreatmentAppService.GetScenarioTreatmentsPageAsync(
                scenid, assetType, district, countyId, route, section, year, sortField, sortDir, page, size);

            var data = result.Data.Select(t => new
            {
                id = t.Id,
                assetType = t.AssetType,
                district = t.District,
                countyId = t.CountyId,
                county = t.County,
                route = t.Route,
                section = t.Section,
                brkey = t.Brkey,
                bridgeId = t.BridgeId,
                treatment = t.TreatmentType,
                interstate = t.Interstate,
                direction = t.Direction,
                cost = t.DirectCost,
                benefit = t.Benefit,
                risk = t.Risk,
                priorityOrder = t.PriorityOrder,
                preferredYear = t.PreferredYear,
                minYear = t.MinYear,
                maxYear = t.MaxYear,
                isCommitted = t.IsCommitted,
                category = t.Category,
                indirectCostDesign = t.IndirectCostDesign,
                indirectCostRow = t.IndirectCostRow,
                indirectCostUtilities = t.IndirectCostUtilities,
                indirectCostOther = t.IndirectCostOther
            });

            var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)Math.Max(1, size)));
            return new JsonResult(new { data, last_page = lastPage, totalCount = result.TotalCount });
        }

        public async Task<IActionResult> OnGetCountiesAsync(int scenarioId, int district)
        {
            var counties = await _projectTreatmentAppService.GetScenarioCountiesAsync(scenarioId, district);
            return new JsonResult(counties.Select(c => new { countyId = c.Id, countyName = c.CountyName }));
        }

        public async Task<IActionResult> OnGetRoutesAsync(int scenarioId, int district, int county)
        {
            var routes = await _projectTreatmentAppService.GetScenarioRoutesAsync(scenarioId, district, county);
            return new JsonResult(routes);
        }

        public async Task<IActionResult> OnGetSectionsAsync(int scenarioId, int district, int county, int route)
        {
            var sections = await _projectTreatmentAppService.GetScenarioSectionsAsync(scenarioId, district, county, route);
            return new JsonResult(sections);
        }

        public async Task<IActionResult> OnPostEditAsync(UpdateProjectTreatmentDto input)
        {
            try
            {
                var success = await _projectTreatmentAppService.EditProjectTreatment(input);
                return new JsonResult(new { success, message = success ? "Treatment was updated successfully." : "Could not update the selected treatment." });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(long treatmentId, int scenarioId)
        {
            var success = await _projectTreatmentAppService.DeleteProjectTreatment(
                treatmentId,
                new TreatmentDto { ScenarioId = scenarioId });
            return new JsonResult(new { success });
        }

        // "Treatment Type" pick-list for the Edit modal — matches the per-project ProjectTreatments page.
        public async Task<IActionResult> OnGetTreatmentTypeCategoriesAsync(int scenarioId, string assetType)
        {
            var categories = await _projectTreatmentAppService.GetTreatmentTypeCategoriesAsync(scenarioId, assetType);
            return new JsonResult(categories);
        }
    }
}
