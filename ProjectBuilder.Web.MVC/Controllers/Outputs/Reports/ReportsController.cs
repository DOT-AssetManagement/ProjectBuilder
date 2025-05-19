using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IRepository<ScenarioModel> _scenarios;
        private readonly IRepository<CountyModel> _counties;
        public ReportsController(IRepository<ScenarioModel> scenarios, IRepository<CountyModel> counties)
        {
            _scenarios = scenarios;
            _counties = counties;
        }
        public async Task<IActionResult> ProjectSummary(int? scenid,int? district)
        {
            var reportsViewModel = await LoadReportsViewModel<ProjectSummaryModel>(scenid, district);
            reportsViewModel.ReportsData = await LoadReportData<ProjectSummaryModel>(scenid, district, new());
            return View("~/Views/Outputs/Reports/ProjectSummary.cshtml",reportsViewModel);
        }
        public async Task<IActionResult> TreatmentSummary(int? scenid,int? district)
        {
            var reportsViewModel = await LoadReportsViewModel<TreatmentSummaryModel>(scenid, district);
            reportsViewModel.ReportsData = await LoadReportData<TreatmentSummaryModel>(scenid,district, new());
            return View("~/Views/Outputs/Reports/TreatmentSummary.cshtml",reportsViewModel);
        }
        public async Task<IActionResult> CombinedProjects(int? scenid, int? district, int? projecttype)
        {
            var reportsViewModel = await LoadReportsViewModel<CombinedProjectModel>(scenid,district);
            reportsViewModel.SelectedProjectType = projecttype;
            var projectFilter = new Dictionary<string, object>();
            switch (projecttype)
            {
                case 1:
                    projectFilter["ProjectType"] = "B";
                    break;
                case 2:
                    projectFilter["ProjectType"] = "P";
                    break;
                default:
                    projectFilter["ProjectType"] = "C";
                    break;
            }
            reportsViewModel.ReportsData = await LoadReportData<CombinedProjectModel>(scenid,district,projectFilter);
            return View("~/Views/Outputs/Reports/CombinedProjects.cshtml", reportsViewModel);
        }

        private async Task<ReportsViewModel<T>> LoadReportsViewModel<T>(int? scenid,int? district)
        {
            ReportsViewModel<T> reportsViewModel = new();
            reportsViewModel.ScenarioId = scenid;
            reportsViewModel.SelectedDistrict = district;
            if (scenid.HasValue)
            {
                var scenario = await _scenarios.FindAsync(scenid.Value);
                reportsViewModel.ScenarioName = scenario is null ? "" : scenario.ScenarioName;
            }
            var counties = await _counties.GetAllAsync();
            reportsViewModel.Districts = counties.Select(c => c.District).Distinct().OrderBy(d => d);
            return reportsViewModel;
        }

        private async Task<List<T>> LoadReportData<T>(int? scenid,int? district, Dictionary<string, object> projectFilter) where T : class
        {
            projectFilter["ScenarioId"] = scenid.HasValue ? scenid.Value : 0;
            if(district.HasValue)
            projectFilter["District"] = district.Value;
            var repository = HttpContext.RequestServices.GetRequiredService<IRepository<T>>();
            repository.ApplyFilter(projectFilter);
            return await repository.GetAllAsync();
        }
    }
}
