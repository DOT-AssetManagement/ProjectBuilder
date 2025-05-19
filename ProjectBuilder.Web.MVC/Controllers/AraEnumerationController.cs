using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class AraEnumerationController: Controller   
    {
        private readonly IRepository<AraEnumerationsModel> _ARAEnumerationsRepository;
        public AraEnumerationController(IRepository<AraEnumerationsModel> ARAEnumerationsRepository)
        {
            _ARAEnumerationsRepository= ARAEnumerationsRepository;
        }
        public async Task<IActionResult> Index()
        {
          if(User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
            var araEnumerations = await _ARAEnumerationsRepository.GetAllAsync();
            var displayResult = araEnumerations.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = araEnumerations.Count();
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
            var araEnumerations = await _ARAEnumerationsRepository.GetAllAsync();
            // Convert to view model type
            var viewModelList = araEnumerations.Select(item => new AraEnumerationViewModel
            {
                EnumFamily = item.EnumFamily,
                EnumName = item.EnumName,
                EnumInt = item.EnumInt
            }).ToList();

            return PartialView("~/Views/AraEnumeration/_AraEnumeration.cshtml", viewModelList);


        }
    }
}
