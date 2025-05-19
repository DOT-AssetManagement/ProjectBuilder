using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class TreatmentParameterController : Controller
    {
        private readonly IRepository<TreatmentParameterModel> _repository;

        public TreatmentParameterController(IRepository<TreatmentParameterModel> repository)
        {
            _repository = repository;
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
            var treatmentParameter = await _repository.GetAllAsync();
            var displayResult = treatmentParameter.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = treatmentParameter.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetTreatmentParameter(int id)
        {
            var treatmentParam = await _repository.FindAsync(id);
            var paramViewModel = new TreatmentParameterViewModel
            {
                TreatmentParameterId = treatmentParam.TreatmentParameterId,
                UserTreatmentBenefitWeight = treatmentParam.UserTreatmentBenefitWeight,
            };
            return Json(paramViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditTreatmentParameter(TreatmentParameterViewModel model)
        {
            if (!(model.UserTreatmentBenefitWeight).HasValue)
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a Role" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.UserTreatmentBenefitWeight), model.UserTreatmentBenefitWeight }
            };
            await _repository.UpdateAsync(model.TreatmentParameterId, pairs);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteTreatmentParameter(TreatmentParameterViewModel model)
        {
            var role = await _repository.DeleteAsync(model.TreatmentParameterId);
            _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
