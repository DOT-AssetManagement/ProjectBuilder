using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class TreatmentTypeController : Controller
    {
        private readonly IRepository<TreatmentTypeModel> _repository;

        public TreatmentTypeController(IRepository<TreatmentTypeModel> repository)
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
            var treatmentType = await _repository.GetAllAsync();
            var displayResult = treatmentType.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = treatmentType.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetTreatmentType(string id)
        {
            var treatmentType = await _repository.FindAsync(id);
            var viewModel = new TreatmentTypeViewModel
            {
                TreatmentName = treatmentType.TreatmentName,
                MoveEarlier= treatmentType.MoveEarlier,
                MoveLater= treatmentType.MoveLater,
                AssetType= treatmentType.AssetType
            };
            return Json(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditTreatmentType(TreatmentTypeViewModel model)
        {
            if (!((model.MoveEarlier).HasValue && (model.MoveLater).HasValue) && (model.AssetType != null))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a Role" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.MoveEarlier), model.MoveEarlier },
                { nameof(model.MoveLater), model.MoveLater },
                { nameof(model.AssetType), model.AssetType }
            };
            await _repository.UpdateAsync(model.TreatmentName, pairs);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
