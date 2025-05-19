using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;
using System.Data.Entity.Migrations.Model;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class EditScenarioController : Controller
    {
        private readonly IRunScenarioUnitOfWork _runScenarioUnit;
        private readonly IRepository<ParameterModel> _repository;
        private readonly ITreatmentRepository _counties;


        public EditScenarioController(IRunScenarioUnitOfWork runScenario, ITreatmentRepository counties)
        {
            _runScenarioUnit = runScenario;
            _counties = counties;
        }

        public async Task<IActionResult> ScenarioParameters(int? scenid)
        {
            if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {
                string scenarioName = "";
                if (scenid.HasValue)
                {
                    var scenario = await _runScenarioUnit.ScenariosRepo.FindAsync(scenid);
                    _runScenarioUnit.ScenarioId = scenid.Value;
                    scenarioName = scenario is null ? "" : scenario.ScenarioName;
                }
                var scenarioParameters = await _runScenarioUnit.ScenarioParametersRepo.GetAllAsync();
                return View("~/Views/Scenarios/ScenarioParameters.cshtml", new ScenarioParametersViewModel { ScenarioParameters = scenarioParameters, ScenarioName = scenarioName, ScenarioId = scenid });

            }
            return RedirectToAction("Index", "Scenarios");

        }
        #region NewScenarioPage
        public async Task<IActionResult> PScenarioParameters()
        {
            //var parameter = await _repository.GetAllAsync();
            return PartialView("~/Views/Scenarios/_ScenarioParameters.cshtml");
        }
        public async Task<IActionResult> PBudConstraints()
        {
            return PartialView("~/Views/Scenarios/_ScenarioBudgetConstraints.cshtml");
        }
        #endregion
       

        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
            var parameter = await _repository.GetAllAsync();
            var displayResult = parameter.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = parameter.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }
        public async Task<List<ScenarioBudgetFlatModel>> PBudgetConstraints(int? scenid)
        {
            string scenarioName = "";
            if (scenid.HasValue)
            {
                var scenario = await _runScenarioUnit.ScenariosRepo.FindAsync(scenid);
                _runScenarioUnit.ScenarioId = scenid.Value;
                scenarioName = scenario is null ? "" : scenario.ScenarioName;
            }

            var scenarioBudgets = await _runScenarioUnit.ScenariosBudgetsRepo.GetAllAsync();

            // Filter out repeated records based on ScenarioName
            var uniqueScenarioBudgets = scenarioBudgets
                .GroupBy(sb => sb.YearWork)
                .Select(group => group.First())
                .ToList();

            return uniqueScenarioBudgets;
        }
        public async Task<IActionResult> ReviseBudgetConstarints(int? firstYear, int? lastYear)
        {
            var counties = await _counties.GetAllCounties();
            var districts = counties.Select(a => a.District).OrderBy(d => d).Distinct().ToList();
            var model = new List<ReviseBudgetConstraintsViewModel>();
            int x = 1;

            foreach (var district in districts)
            {
                for (int year = firstYear.GetValueOrDefault(); year <= lastYear.GetValueOrDefault(); year++)
                {
                    var item = new ReviseBudgetConstraintsViewModel();
                    item.District = district;
                    item.YearOfWork = year;
                    item.BridgeInterstate = 1000000;
                    item.BridgeNonInterstate = 1000000;
                    item.PavementInterstate = 1000000;
                    item.PavementNonInterstate = 1000000;
                    model.Add(item);
                }
            }
            return Json(model);
        }







        public async Task<IActionResult> BudgetConstraints(int? scenid)
        {
            if(User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {
                string scenarioName = "";
                if (scenid.HasValue)
                {
                    var scenario = await _runScenarioUnit.ScenariosRepo.FindAsync(scenid);
                    _runScenarioUnit.ScenarioId = scenid.Value;
                    scenarioName = scenario is null ? "" : scenario.ScenarioFullName;
                }
                var scenarioBudgets = await _runScenarioUnit.ScenariosBudgetsRepo.GetAllAsync();
                return View("~/Views/Scenarios/BudgetConstraints.cshtml", new ScenarioBudgetConstraintsViewModel { BudgetConstraints = scenarioBudgets, SelectedScenario = scenarioName, ScenarioId = scenid });

            }
            return RedirectToAction("Index", "Scenarios");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateScenarioParameter(int? scenid, ScenarioParameterModel scenparam)
        {
            var target = await  _runScenarioUnit.ScenarioParametersRepo.FindAsync(scenid, scenparam.ParameterId);
            if (target is null)
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    Title = "Bad Request",
                    SubTitle = "Couldn't get the Selected Scenario Paramter",
                    ErrorMessage = "in order to update the selected Scenario Parameter make sure to select a scenario, go back to the scenario page and select a scenario then try again.",
                    Type = ErrorType.BadRequest,
                    HasBackBtn = true,
                    BtnContent = "Back to Scenario Page",
                    BtnUrl = "/Scenarios"
                });
            target.ParameterValue = scenparam.ParameterValue;
            await _runScenarioUnit.ScenarioParametersRepo.UpdateAsync(target,nameof(target.ParameterValue));
            await _runScenarioUnit.SaveChangesAsync();
            return RedirectToAction(nameof(ScenarioParameters),new {scenid = scenid });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateBudgetConstraints(int? scenid, ScenarioBudgetFlatModel scenarioBudget)
        {
            if (scenid is null)
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    Title = "Bad Request",
                    SubTitle = "Scenario is missing",
                    ErrorMessage = "in order to update the selected budget make sure to select a scenario, go back to the scenario page and select a scenario.",
                    Type = ErrorType.BadRequest,
                    HasBackBtn = true,
                    BtnContent = "Back to Scenario Page",
                    BtnUrl = "/Scenarios"
                });
            if (!ModelState.IsValid)
                return View("~/Views/Shared/Error.cshtml",new ErrorViewModel
                {
                    Title = "Error",
                    SubTitle = "One of budget constraint info is missing",
                    ErrorMessage = "in order to update the selected budget make sure to fill all the budget values, go back to the budget constraints page and try again.",
                    Type = ErrorType.Error,
                    HasBackBtn = true,
                    BtnContent = "Back to Budget Constraints Page",
                    BtnUrl = $"/EditScenario/BudgetConstraints?scenid={scenid}"
                });
            scenarioBudget.ScenarioId = scenid.Value;
            await _runScenarioUnit.ScenariosBudgetsRepo.UpdateAsync(scenarioBudget,nameof(scenarioBudget.BridgeInterstateBudget));
            await _runScenarioUnit.ScenariosBudgetsRepo.UpdateAsync(scenarioBudget, nameof(scenarioBudget.BridgeNonInterstateBudget));
            await _runScenarioUnit.ScenariosBudgetsRepo.UpdateAsync(scenarioBudget, nameof(scenarioBudget.PavementInterstateBudget));
            await _runScenarioUnit.ScenariosBudgetsRepo.UpdateAsync(scenarioBudget, nameof(scenarioBudget.PavementNonInterstateBudget));
            await _runScenarioUnit.SaveChangesAsync();
            return  RedirectToAction(nameof(BudgetConstraints), new { scenid = scenid });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteScenarioParameter(int? scenid, string scenparamid)
        {
            if (!scenid.HasValue || string.IsNullOrEmpty(scenparamid))
                return BadRequest("couldn't find the selected scenario paramter, make that you have selected a scenario then try again.");
            var success = await _runScenarioUnit.ScenarioParametersRepo.DeleteAsync(scenid,scenparamid);
            await _runScenarioUnit.SaveChangesAsync();
            if(success)
                return Ok($"the scenario paramter with id: {scenparamid} has been successfully deleted.");
            return BadRequest("couldn't delete the selected scenario parameter, please try again.");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteScenarioBudget(int? scenid, int? yearofwork,int? district)
        {
            if (!scenid.HasValue || !yearofwork.HasValue || !district.HasValue)
                return BadRequest("couldn't find the selected scenario budget, make that you have selected a scenario then try again.");
            var rows = await _runScenarioUnit.ScenariosBudgetsRepo.DeleteAsync(new ScenarioBudgetFlatModel() { District = district.Value,ScenarioId = scenid.Value,YearWork = yearofwork.Value});
            if (rows > 0)
                return Ok($"the scenario budget has been successfully deleted.");
            return BadRequest("couldn't delete the selected scenario parameter, please try again.");
        }
    }
}
