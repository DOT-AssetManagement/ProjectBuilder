using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess.Entities;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class CountyController : Controller
    {
        private readonly IRepository<CountyModel> _countyRepository;
        private readonly ITreatmentRepository _treatments;

        public CountyController(IRepository<CountyModel> countyRepository, ITreatmentRepository treatments) 
        {
            _countyRepository= countyRepository;
            _treatments = treatments;
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
            var counties = await _countyRepository.GetAllAsync();
            var treatments = await _treatments.GetAllAsync();
            var ListNewCounty = new List<CountyDisplayVM>();
            foreach (var item in counties)
            {
                var NewCounty = new CountyDisplayVM();
                NewCounty.CountyId = item.CountyId;
                NewCounty.CountyName = item.CountyName;
                NewCounty.District = item.District;
                NewCounty.CountyFullName = item.CountyFullName;
                {
                    var Contain = treatments.Any(a => a.CountyId == item.CountyId);
                    if (Contain)
                    {
                        NewCounty.IsDeleteable = true;
                    }
                    else
                    {
                        NewCounty.IsDeleteable = false;
                    }
                }
                ListNewCounty.Add(NewCounty);
            }
            if (param.sSearch != null)
            {
                ListNewCounty = ListNewCounty.Where(
                   t =>
                   (t.CountyName.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.District.ToString().Contains(param.sSearch.ToLower())) ||
                   (t.CountyFullName.ToLower().Contains(param.sSearch.ToLower())) 
                    ).ToList();
            }

            var displayResult = ListNewCounty.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = ListNewCounty.Count();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = displayResult
            });
        }
        public async Task<ActionResult> NewCounty()
        {
            var counties = await _countyRepository.GetAllAsync();
            var treatments = await _treatments.GetAllAsync();
            var ListNewCounty = new List<CountyDisplayVM>();
            foreach (var item in counties)
            {
                var NewCounty = new CountyDisplayVM();
                NewCounty.CountyId = item.CountyId;
                NewCounty.CountyName = item.CountyName;
                NewCounty.District = item.District;
                NewCounty.CountyFullName = item.CountyFullName;
                {
                    var Contain = treatments.Any(a => a.CountyId == item.CountyId);
                    if (Contain)
                    {
                        NewCounty.IsDeleteable = true;
                    }
                    else
                    {
                        NewCounty.IsDeleteable = false;
                    }
                }
                ListNewCounty.Add(NewCounty);
            }
            return PartialView("~/Views/County/_County.cshtml", ListNewCounty);
        }

        public async Task<IActionResult> GetCounty(int id, int district)
        {
            var County = await _countyRepository.FindAsync(district,id);
            var countyViewModel = new CountyViewModel()
            {
                CountyName = County.CountyName,
                District=County.District,
                CountyId = County.CountyId
            };
            return Json(countyViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditCounty(CountyViewModel model)
        {
            if (string.IsNullOrEmpty(model.CountyName.ToString()))
                return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to edit a County" });
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.CountyName), model.CountyName },
            };
            await _countyRepository.UpdateAsync(pairs, model.District, model.CountyId);
            await _countyRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteCounty(int countyId, int district)
        {
            var county = await _countyRepository.DeleteAsync(district, countyId);
            _countyRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<JsonResult> CheckCounty(int counId)
        {
            var treatments = await _treatments.GetAllAsync();
            var NewTreat = treatments.Any(a => a.CountyId == counId);
            return Json(NewTreat);
        }

    }
}
