using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Permissions;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.Pool.Dtos;
using ProjectBuilder.Models.ProjectCandidate;
using ProjectBuilder.Models.ProjectCandidate.Dtos;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Models.WorkCandidates;
using ProjectBuilder.Models.WorkCandidates.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;

namespace ProjectBuilder.Web.Pages.Outputs
{
    [Authorize]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ProjectsModel : PageModel
    {
        private readonly IProjectCandidateAppService _projectCandidateAppService;
        private readonly IWorkCandidatesAppService _WorkCandidateAppService;
        private readonly IPoolAppService _poolsappservice;
        private readonly IDssScenarioRepository _dssScenarioRepository;
        private readonly IPermissionChecker _permissionChecker;
        private readonly string _connectionString;
        private readonly ILogger<ProjectsModel> _logger;
        public List<ProjectCandidateDto> ProjectCandidates { get; set; }
        public List<CandidatePoolDto> Pools { get; set; }
        public Guid? ScenarioId { get;  set; }
        public Guid? PoolId { get;  set; }
        public Guid? DssScenarioId { get; set; }
        public string SearchString { get; set; }
        public string ScenarioName { get;  set; }
        public string PoolName { get;  set; }
        public List<byte?> Districts { get; set; }
        public List<CountyFilterOption> Counties { get; set; }
        public List<int?> Years { get; set; }
        public byte? SelectedDistrict { get; set; }
        public byte? SelectedCounty { get; set; }
        public int? SelectedYear { get; set; }

        public WorkCandidatesDto project { get; set; }

        public class CountyFilterOption
        {
            public byte Cnty { get; set; }
            public string Name { get; set; }
            public string Display => string.IsNullOrWhiteSpace(Name) ? Cnty.ToString() : $"{Cnty} - {Name}";
        }
        public ProjectsModel(
            IProjectCandidateAppService projectCandidateAppService,
            IScenarioAppService scenarioAppService,
            IPoolAppService poolsappservice,
            IWorkCandidatesAppService WorkCandidateAppService,
            IDssScenarioRepository dssScenarioRepository,
            IConfiguration configuration,
            IPermissionChecker permissionChecker,
            ILogger<ProjectsModel> logger)
        {
            _projectCandidateAppService = projectCandidateAppService;
            _WorkCandidateAppService = WorkCandidateAppService;
            _poolsappservice = poolsappservice;
            _dssScenarioRepository = dssScenarioRepository;
            _connectionString = configuration.GetConnectionString("Default");
            _permissionChecker = permissionChecker;
            _logger = logger;
        }

        public async Task OnGet(Guid? poolId, string searchString = "", byte? district = null, byte? county = null, int? year = null)
        {
            SearchString = searchString;

            if (poolId.HasValue)
            {
                ProjectCandidates = await _projectCandidateAppService.GetAllProjectsAsync(poolId.Value);
                var pool = await _poolsappservice.GetAsync(poolId.Value);
                PoolId = poolId.Value;
                PoolName = pool.Name;

                foreach (var projectCandidate in ProjectCandidates)
                {
                    project = await _WorkCandidateAppService.GetByProjectId(projectCandidate.Id);
                    if (project != null)
                    {
                        projectCandidate.AssetType = project.AssetType;
                    }
                }

                PopulateDropDownValues(district, county);
                ApplyFilter(district, county, year);

                SelectedDistrict = district;
                SelectedCounty = county;
                SelectedYear = year;
            }
            else
            {
                ProjectCandidates = await _projectCandidateAppService.GetAllAsync();
                PoolId = ProjectCandidates.FirstOrDefault()?.LibraryId;
            }

            if (PoolId.HasValue)
            {
                var dssScenario = await _dssScenarioRepository.GetByPoolIdAsync(PoolId.Value);
                DssScenarioId = dssScenario?.Id;
            }

            Pools = await _poolsappservice.GetAllAsync("","");
        }

        public async Task<IActionResult> OnGetDropDownValue(Guid poolId, byte? district, byte? county)
        {
            ProjectCandidates = await _projectCandidateAppService.GetAllProjectsAsync(poolId);
            PopulateDropDownValues(district, county);
            return new JsonResult(new { Districts, Counties, Years });
        }

        private void PopulateDropDownValues(byte? district, byte? county)
        {
            Districts = ProjectCandidates.Select(pc => pc.District).Distinct().OrderBy(d => d).ToList();

            // Always build dependent lists from the unfiltered set so selected values remain
            // available in the dropdowns after Filter reloads the page.
            var countySource = district != null
                ? ProjectCandidates.Where(pc => pc.District == district)
                : ProjectCandidates;

            Counties = countySource
                .Where(pc => pc.Cnty.HasValue)
                .GroupBy(pc => pc.Cnty!.Value)
                .Select(g => new CountyFilterOption
                {
                    Cnty = g.Key,
                    Name = g.Select(x => x.CountyName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                })
                .OrderBy(c => c.Cnty)
                .ToList();

            var yearSource = county != null
                ? ProjectCandidates.Where(pc => pc.Cnty == county)
                : (district != null
                    ? ProjectCandidates.Where(pc => pc.District == district)
                    : ProjectCandidates);

            Years = yearSource
                .SelectMany(pc => new int?[] { pc.YearEarliest, pc.YearLatest })
                .Where(y => y.HasValue)
                .Distinct()
                .OrderBy(y => y)
                .ToList();
        }

        private void ApplyFilter(byte? district, byte? county, int? year)
        {
            if (district != null)
            {
                ProjectCandidates = ProjectCandidates.Where(pc => pc.District == district).ToList();
            }
            if (county != null)
            {
                ProjectCandidates = ProjectCandidates.Where(pc => pc.Cnty == county).ToList();
            }
            if (year != null)
            {
                ProjectCandidates = ProjectCandidates.Where(pc =>
                    pc.YearEarliest == year
                    || pc.YearLatest == year
                    || (pc.YearEarliest.HasValue && pc.YearLatest.HasValue
                        && year >= pc.YearEarliest && year <= pc.YearLatest)).ToList();
            }
        }

        public async Task<IActionResult> OnPostUpdateProject(UpdateProjectCandidateDto input)
        {
            await _projectCandidateAppService.UpdateAsync(input.Id, input);
            return RedirectToPage("/OutPuts/Projects", new { scenarioId = ScenarioId });
        }

        public async Task<IActionResult> OnPostCreateProject(CreateProjectCandidateDto input)
        {
            await _projectCandidateAppService.CreateAsync(input);
            return RedirectToPage("/Outputs/Projects", new { scenarioId = ScenarioId });
        }

        public async Task<IActionResult> OnPostExport(Guid poolId, double? effThreshold)
        {
            if (!await _permissionChecker.IsGrantedAsync(ReportsPermissions.Reports.ProjectsExport))
                return Forbid();

            string xlsxFilePath = Path.Combine(Path.GetTempPath(), $"Export_{Guid.NewGuid()}.xlsx");

            try
            {
                var ok = PBLogic.DataManager.ExportPoolData(_connectionString, poolId, effThreshold, xlsxFilePath, out string errorMessage);

                if (!ok)
                {
                    _logger.LogError("Failed to export pool {PoolId}: {Error}", poolId, errorMessage);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Export failed. Please try again.");
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(xlsxFilePath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export pool {PoolId}.", poolId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Export failed. Please try again.");
            }
            finally
            {
                if (System.IO.File.Exists(xlsxFilePath))
                {
                    System.IO.File.Delete(xlsxFilePath);
                }
            }
        }
    }
}
