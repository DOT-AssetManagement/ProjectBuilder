using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class DefaultSlackController : Controller
    {
        private readonly IRepository<DefaultSlackModel> _defaultSlackrepository;
        public DefaultSlackController(IRepository<DefaultSlackModel> defaultSlackrepository)
        {
            _defaultSlackrepository= defaultSlackrepository;
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
            var defaultSlacks = await _defaultSlackrepository.GetAllAsync();
            var displayResult = defaultSlacks.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = defaultSlacks.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }


        public async Task<IActionResult> GetDefaultSlack(string AssetType)
        {
            var defaultSlack = await _defaultSlackrepository.FindAsync(AssetType);
            var defaultSlackViewModel = new DefaultSlackViewModel()
            {
                AssetType = defaultSlack.AssetType,
                MoveAfter = defaultSlack.MoveAfter,
                MoveBefore= defaultSlack.MoveBefore
            };
            return Json(defaultSlackViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditDefaultSlack(DefaultSlackViewModel model)
        {
            if (string.IsNullOrEmpty(model.MoveBefore.ToString()) || string.IsNullOrEmpty(model.MoveAfter.ToString()))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a  Slack Period" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.MoveAfter), model.MoveAfter },
                { nameof(model.MoveBefore), model.MoveBefore }
            };
            await _defaultSlackrepository.UpdateAsync(model.AssetType, pairs);
            await _defaultSlackrepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> DeleteDefaultSlack(string AssetType)
        {
            var county = await _defaultSlackrepository.DeleteAsync(AssetType);
            _defaultSlackrepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
