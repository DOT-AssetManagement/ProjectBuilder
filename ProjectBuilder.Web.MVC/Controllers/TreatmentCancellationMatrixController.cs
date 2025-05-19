using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class TreatmentCancellationMatrixController : Controller
    {
        private readonly IRepository<TreatmentCancellationMatrixModel> _repository;

        public TreatmentCancellationMatrixController(IRepository<TreatmentCancellationMatrixModel> repository)
        {
            _repository = repository;
        }
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
            var treatmentCancellationMatrix = await _repository.GetAllAsync();
            var displayResult = treatmentCancellationMatrix.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = treatmentCancellationMatrix.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetTreatmentCancellationMatrix(char assetType, string treatment)
        {
            var treatmentCancellationMatrix = await _repository.FindAsync(assetType, treatment);
            var matrixViewModel = new TreatmentCancellationMatrixViewModel
            {
                AssetTypeA = treatmentCancellationMatrix.AssetTypeA,
                TreatmentA = treatmentCancellationMatrix.TreatmentA,
                AssetTypeB = treatmentCancellationMatrix.AssetTypeB,
                TreatmentB = treatmentCancellationMatrix.TreatmentB,
            };
            return Json(matrixViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditTreatmentCancellationMatrix(TreatmentCancellationMatrixViewModel model)
        {
            if (model.AssetTypeB != null && string.IsNullOrEmpty(model.TreatmentB))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a Role" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.AssetTypeB), model.AssetTypeB },
                { nameof(model.TreatmentB), model.TreatmentB }
            };
            await _repository.UpdateAsync(pairs, model.AssetTypeA, model.TreatmentA);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteTreatmentCancellationMatrix(TreatmentCancellationMatrixViewModel model)
        {
            var role = await _repository.DeleteAsync(model.AssetTypeA, model.TreatmentA);
            _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
