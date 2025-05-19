using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    [Authorize]
    public class ChartsController : Controller
    {
        private readonly IRepository<ScenarioModel> _scenarios;
        private readonly IRepository<CountyModel> _counties;

        public ChartsController(IRepository<ScenarioModel> scenarios,IRepository<CountyModel> counties)
        {
           _scenarios = scenarios;
           _counties = counties;
        }
        public async Task<IActionResult> Needs(int? scenid)
        {
            var chartsViewModel = await LoadChartViewModel(scenid);
            return View("~/Views/Outputs/Charts/Needs.cshtml",chartsViewModel);
        }
        public async Task<IActionResult> PotentialBenefits(int? scenid)
        {
            var chartsViewModel = await LoadChartViewModel(scenid);
            return View("~/Views/Outputs/Charts/PotentialBenefits.cshtml",chartsViewModel);
        }
        public async Task<IActionResult> Budget(int? scenid)
        {
            var chartsViewModel = await LoadChartViewModel(scenid);
            return View("~/Views/Outputs/Charts/Budget.cshtml",chartsViewModel);
        }

        public async Task<IActionResult> BudgetSpent(int? scenid)
        {
            var chartsViewModel = await LoadChartViewModel(scenid);
            return View("~/Views/Outputs/Charts/BudgetSpent.cshtml",chartsViewModel);
        }

        private async Task<ChartsViewModel> LoadChartViewModel(int? scenid)
        {
            ChartsViewModel chartsViewModel = new();
            chartsViewModel.ScenarioId = scenid;
            if (scenid.HasValue)
            {
                var scenario = await _scenarios.FindAsync(scenid.Value);
                chartsViewModel.ScenarioName = scenario is null ? "" : scenario.ScenarioName;
            }
            var counties = await _counties.GetAllAsync();
            chartsViewModel.Districts = counties.Select(c => c.District).Distinct().OrderBy(d => d);
            return chartsViewModel;
        }
    }
}
