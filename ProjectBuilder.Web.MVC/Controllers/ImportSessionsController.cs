using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class ImportSessionsController : Controller
    {
        private readonly IRepository<ImportSessionsModel> _ImportSessionsRepository;
        public ImportSessionsController(IRepository<ImportSessionsModel> ImportSessionsRepository) 
        {
            _ImportSessionsRepository= ImportSessionsRepository;
        }
        public async  Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
            var ImportSessions = await _ImportSessionsRepository.GetAllAsync();
            var displayResult = ImportSessions.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = ImportSessions.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }
    }
}
