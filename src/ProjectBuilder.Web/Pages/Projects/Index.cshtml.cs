using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models.Project;
using ProjectBuilder.Models.Project.Dtos;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.Projects
{
    public class IndexModel : AnalysisPageModelBase
    {
        private readonly IProjectAppService _projectAppService;
        private readonly IScenarioAppService _scenarioAppService;

        public bool AutoExport { get; set; }

        // Scenario-wide and independent of the cascade, so loaded once up front instead of via ajax.
        public List<byte?> Districts { get; set; } = new();
        public List<int?> Years { get; set; } = new();

        [BindProperty]
        public ExportProjectDto ExportInput { get; set; } = new();

        public override string ActiveMenu => "Projects";

        public IndexModel(
            IProjectAppService projectAppService,
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService,
            IProjectRepository projectRepository)
            : base(currentUser, tourGuideSeenService, projectRepository)
        {
            _projectAppService = projectAppService;
            _scenarioAppService = scenarioAppService;
        }

        public async Task<IActionResult> OnGetAsync(int scenid, bool fromscenario = false, bool export = false)
        {
            FromScenario = fromscenario;
            AutoExport = export;
            await LoadScenarioAsync(_scenarioAppService, scenid);
            Districts = await _projectAppService.GetDistrictsAsync(scenid);
            Years = await _projectAppService.GetYearsAsync(scenid);
            return Page();
        }

        // Backs the Tabulator grid's remote pagination — one page of rows at a time.
        // [FromQuery] on page/size is required: "page" collides with Razor Pages' own reserved route-value key for page routing, which otherwise wins over the query string and silently binds to the parameter's default.
        public async Task<IActionResult> OnGetProjectsPageAsync(
            int scenid, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, string sortDir, bool unfunded = false, string committed = null, [FromQuery] int page = 1, [FromQuery] int size = 20)
        {
            var result = await _projectAppService.GetProjectsPageAsync(
                scenid, district, countyId, route, section, year, sortField, sortDir, page, size, unfunded, committed);

            var data = result.Data.Select(p => new
            {
                id            = p.Id,
                sequenceNo    = p.SequenceNo,
                assetType     = p.AssetType,
                district      = p.District,
                countyId      = p.CountyId,
                county        = p.County,
                route         = p.Route,
                section       = p.Section,
                direction     = p.Direction,
                firstYear     = p.SelectedFirstYear,
                totalCost     = p.TotalCost,
                benefit       = p.Benefit,
                treatments    = p.NumberOfTreatments,
                hasCommitted  = p.CommitmentStatus,
                selected      = p.Selected,
                preferredYear = p.PreferredStartingYear,
                userCreated   = p.UserCreated
            });

            var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)Math.Max(1, size)));
            return new JsonResult(new { data, last_page = lastPage, totalCount = result.TotalCount });
        }

        public async Task<IActionResult> OnGetCountiesAsync(int scenid, int? district)
        {
            var counties = await _projectAppService.GetCountiesAsync(scenid, district);
            return new JsonResult(counties.Select(c => new { countyId = c.Id, countyName = c.CountyName }));
        }

        public async Task<IActionResult> OnGetRoutesAsync(int scenid, int? district, int? county)
        {
            var routes = await _projectAppService.GetRoutesAsync(scenid, district, county);
            return new JsonResult(routes);
        }

        public async Task<IActionResult> OnGetSectionsAsync(int scenid, int? district, int? county, int? route)
        {
            var sections = await _projectAppService.GetSectionsAsync(scenid, district, county, route);
            return new JsonResult(sections);
        }

        // Backs "Search Project ID" — resolves the display-only sequence number to a real project id.
        public async Task<IActionResult> OnGetProjectIdBySequenceAsync(int scenid, int sequenceNo, bool unfunded = false)
        {
            var projectId = await _projectAppService.GetProjectIdBySequenceNoAsync(scenid, sequenceNo, unfunded);
            return new JsonResult(new { projectId });
        }

        // Backs Export — resolves the ids passing the active filter server-side.
        public async Task<IActionResult> OnGetExportProjectIdsAsync(
            int scenid, byte? district, byte? countyId, int? route, string section, int? year, bool unfunded = false, string committed = null)
        {
            var ids = await _projectAppService.GetProjectIdsAsync(scenid, district, countyId, route, section, year, unfunded, committed);
            return new JsonResult(new { ids });
        }

        public async Task<IActionResult> OnPostDeleteProjectAsync(int projectId, int scenId)
        {
            await _projectAppService.DeleteProjectAsync(projectId, scenId);
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var result = await _projectAppService.GetExportAsync(ExportInput);
            return File(result.Bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.Filename);
        }
    }
}
