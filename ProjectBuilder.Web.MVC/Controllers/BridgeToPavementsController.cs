using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;
using System.Linq;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class BridgeToPavementsController : Controller
    {
        private readonly IRepository<BridgeToPavementsModel> _BridgeToPavementsRepository;
        public BridgeToPavementsController(IRepository<BridgeToPavementsModel> BridgeToPavementsRepository) 
        { 
            _BridgeToPavementsRepository= BridgeToPavementsRepository;
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
            var bridgeToPavements = await _BridgeToPavementsRepository.GetAllAsync();
            if (param.sSearch != null)
            {
                bridgeToPavements = bridgeToPavements.Where(
                   t => 
                   (t.BrKey.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.BridgeId.ToString().Contains(param.sSearch)) ||
                   (t.District.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.County.ToLower().Contains(param.sSearch.ToLower())) ||
                   (t.CountyCode.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.Route.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.Segment.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.OffSet.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.CreatedBy.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.CreatedAt.ToString().Contains(param.sSearch.ToLower())) 
                    ).ToList();
            }
                var displayResult = bridgeToPavements.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength).ToList();
                var totalRecords = bridgeToPavements.Count();
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = displayResult
                });
        }
        public async Task<ActionResult> Display()
        {
            var bridgeToPavements = await _BridgeToPavementsRepository.GetAllAsync();
            var viewModelList = bridgeToPavements.Select(item => new BridgeToPavementsViewModel
            {
                BrKey = item.BrKey,
            }).ToList();

            return PartialView("~/Views/BridgeToPavements/_BridgeToPavements.cshtml", bridgeToPavements);

        }

    }
}
