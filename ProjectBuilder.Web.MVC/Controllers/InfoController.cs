using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;

namespace ProjectBuilder.Web.MVC.Controllers
{
    public class InfoController : Controller
    {
        private readonly ITreatmentRepository _treatments;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectTreatmentRepository _projectTreatmentsRepo;

        public InfoController(ITreatmentRepository treatments, IProjectRepository projectRepository, IProjectTreatmentRepository projectTreatment)
        {
            _treatments = treatments;
            _projectRepository = projectRepository;
            _projectTreatmentsRepo = projectTreatment;
        }
        public async Task<IActionResult> GetDistricts(string caller, int? scenarioId)
        {
            switch (caller)
            {
                default:
                    var treatments = await _treatments.GetAllAsync();
                    var FTreatments = treatments.Select(a => a.District).OrderBy(d => d).Distinct().ToList();
                    return Json(FTreatments);
                case "/Projects":
                    return Json(await _projectRepository.GetDistricts(scenarioId));
                case "/ProjectTreatments":
                    return Json(await _projectTreatmentsRepo.GetDistricts(scenarioId));
            }
        }
        public async Task<IActionResult> GetCounties(string caller, int? scenarioId, int? district)
        {
            switch (caller)
            {
                default:
                    return Json(await _treatments.GetCounties(district));
                case "/Projects":
                    return Json(await _projectRepository.GetCounties(scenarioId, district));
                case "/ProjectTreatments":
                    return Json(await _projectTreatmentsRepo.GetCounties(scenarioId, district));
            }
        }
        public async Task<IActionResult> GetRoutes(string caller, int? scenarioId, int? district, int? county)
        {
            switch (caller)
            {
                default:
                    return Json(await _treatments.GetRoutes(district, county));
                case "/Projects":
                    return Json(await _projectRepository.GetRoutes(scenarioId, district, county));
                case "/ProjectTreatments":
                    return Json(await _projectTreatmentsRepo.GetRoutes(scenarioId, district, county));
            }
        }
        public async Task<IActionResult> GetSections(string caller, int? scenarioId, int? district, int? county, int? route)
        {
            switch (caller)
            {
                default:
                    return Json(await _treatments.GetSections(district, county, route));
                case "/Projects":
                    return Json(await _projectRepository.GetSections(scenarioId, district, county, route));
                case "/ProjectTreatments":
                    return Json(await _projectTreatmentsRepo.GetSections(scenarioId, district, county, route));
            }
        }

        public async Task<IActionResult> GetTreatmentTypes(string caller, int? scenarioId, int? district, int? county, int? route)
        {
            var treatmentFilter = new TreatmentSearchModel
            {
                District = (byte?)district,
                Cnty = (byte?)county,
                Route = route
            };
            switch (caller)
            {
                default:
                    return Json(await _treatments.FilterTreatments(treatmentFilter));
                case "/Outputs/Maps":
                    return Json(await _treatments.GetTreatmentTypes(district, county, route));
            }
        }

        public async Task<IActionResult> GetDirectionInterstate(string section, int? route, int? county, int? district)
        {
            if (district.HasValue && county.HasValue && route.HasValue)
            {
                var combinedsections = section.DecomposeString<int>('-');
                if (combinedsections.Length == 2)
                {
                    var interstate = await _treatments.GetDirectionInterstate(district, county, route, combinedsections[0], combinedsections[1]);
                    return Json(interstate);
                }
                else if(combinedsections.Length == 1)
                {
                    var interstate = await _treatments.GetDirectionInterstate(district, county, route, combinedsections[0], combinedsections[0]);
                    return Json(interstate);
                }
            }
            return NotFound("could not locate direction and interstate for the selected section.");
        }
        public async Task<IActionResult> GetDefaultSlackValues(string assettype)
        {
            if (string.IsNullOrEmpty(assettype))
                return NotFound("could not locate default slacks for the selected asset type.");
            var defaultSlack = await _treatments.GetAssetTypeDefaultSlack(assettype);
            return Json(defaultSlack);
        }
    }
}
