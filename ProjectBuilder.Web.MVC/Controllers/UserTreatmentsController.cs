using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class UserTreatmentsController : Controller
    {
        private readonly IRepository<UserTreatmentTypeModel> _repository;
        public UserTreatmentsController(IRepository<UserTreatmentTypeModel> repository)
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
            var userTreatments = await _repository.GetAllAsync();
            var displayResult = userTreatments.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = userTreatments.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserTreatment(int id)
        {
            var userTreatment = await _repository.FindAsync(id);
            var userTreatmentViewModel = new UserTreatmentsViewModel
            {
                UserTreatmentsId = userTreatment.UserTreatmentsId,
                UserTreatmentName = userTreatment.UserTreatmentName,
            };
            return Json(userTreatmentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserTreatments(UserTreatmentsViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserTreatmentName))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a User Treatments" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.UserTreatmentName), model.UserTreatmentName }
            };
            await _repository.UpdateAsync(model.UserTreatmentsId, pairs);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteUserTreatments(UserTreatmentsViewModel model)
        {
            var role = await _repository.DeleteAsync(model.UserTreatmentsId);
            _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}