using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class ParameterController : Controller
    {
        private readonly IRepository<ParameterModel> _repository;
        private readonly IRepository<ScenarioParameterModel> _scenarioParameterRepository;

        public ParameterController(IRepository<ParameterModel> repository, IRepository<ScenarioParameterModel> scenarioParameterRepository)
        {
            _repository = repository;
            _scenarioParameterRepository = scenarioParameterRepository;
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
            var parameter = await _repository.GetAllAsync();
            //parameter = parameter.Take(4).ToList();
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
        public async Task<IActionResult> GetAllparameters()
        {
            var parameters = await _repository.GetAllAsync();
            return Json(parameters);
        }

        [HttpGet]
        public async Task<IActionResult> GetParameter(string parameterId, string parmfamily)
        {
            var param = await _repository.FindAsync(parameterId, parmfamily);
            var paramViewModel = new ParameterViewModel
            {
                ParameterId = param.ParameterId,
                ParmDescription = param.ParmDescription,
                Parmfamily = param.Parmfamily,
                ParmName = param.ParmName,
                DefaultValue = param.DefaultValue
            };
            return Json(paramViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditParameter(ParameterViewModel model)
        {
            if (string.IsNullOrEmpty(model.ParmDescription) && string.IsNullOrEmpty(model.ParmName) && model.DefaultValue != null)
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a Role" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.ParmDescription), model.ParmDescription },
                { nameof(model.ParmName), model.ParmName },
                { nameof(model.DefaultValue), model.DefaultValue }
            };
            await _repository.UpdateAsync(pairs, model.ParameterId, model.Parmfamily);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> InsertParameter(List<ParameterViewModel> models, int scenarioId)
        {
            foreach (var model in models)
            {
                if (string.IsNullOrEmpty(model.ParmDescription) && string.IsNullOrEmpty(model.ParmName) && model.DefaultValue != null)
                    return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a Role" });
                var paramter = new ScenarioParameterModel
                { 
                    ParameterId = model.ParameterId,
                    ScenarioId = scenarioId,
                    ParameterValue = model.DefaultValue,
                    ParameterDescription = model.ParmDescription,
                    ParameterName = model.ParmName
                };
                
                _scenarioParameterRepository.Insert(paramter);
            }

            await _scenarioParameterRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteParameter(string parameterId, string parmfamily)
        {
            var role = await _repository.DeleteAsync(parameterId, parmfamily);
            _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
