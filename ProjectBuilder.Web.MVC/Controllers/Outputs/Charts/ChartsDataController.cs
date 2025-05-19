using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Repositories;

namespace ProjectBuilder.Web.MVC.Controllers
{
    [Authorize]
    public class ChartsDataController : Controller
    {
        private readonly IMapper _mapper;

        public ChartsDataController(IMapper mapper)
        {
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<IActionResult> LoadNeedCharts(int? scenId, int? district, int? asset)
        {
            if (!scenId.HasValue && !district.HasValue)
                return BadRequest("You need to select a scenario and a district first");
            if (!scenId.HasValue)
                return BadRequest("You need to select a scenario");
            if (!district.HasValue)
                return BadRequest("You need to select a district");
            var needsUnit = HttpContext.RequestServices.GetRequiredService<IChartsNeedsUnitOfWork>();
            switch (asset)
            {
                case 0:
                    var bridgecharts = await LoadSelectedChart<BridgeNeedsModel>(scenId, district, needsUnit.BridgeNeedsRepo);
                    return Json(_mapper.Map<ChartsDataModel>(bridgecharts));
                case 1:
                    var pavementcharts = await LoadSelectedChart<PavementNeedsModel>(scenId, district, needsUnit.PavementNeedsRepo);
                    return Json(_mapper.Map<ChartsDataModel>(pavementcharts));
                default:
                    var allneeds = await LoadSelectedChart<AllNeedsModel>(scenId, district, needsUnit.AllNeedsRepo);
                    return Json(_mapper.Map<ChartsDataModel>(allneeds));
            }
        }
        [HttpPost]
        public async Task<IActionResult> LoadPotentialBenefitsCharts(int? scenId, int? district, int? asset)
        {
            if (!scenId.HasValue && !district.HasValue)
                return BadRequest("You need to select a scenario and a district first");
            if (!scenId.HasValue)
                return BadRequest("You need to select a scenario");
            if (!district.HasValue)
                return BadRequest("You need to select a district");
            var potentialBenefits = HttpContext.RequestServices.GetRequiredService<IChartsPotentialBenefitsUnitOfWork>();
            switch (asset)
            {
                case 0:
                    var bridgecharts = await LoadSelectedChart<BridgePotentialBenefitsModel>(scenId, district, potentialBenefits!.BridgePotentialBenefitsRepo);
                    return Json(_mapper.Map<ChartsDataModel>(bridgecharts));
                case 1:
                    var pavementcharts = await LoadSelectedChart<PavementPotentialBenefitsModel>(scenId, district, potentialBenefits!.PavementPotentialBenefitsRepo);
                    return Json(_mapper.Map<ChartsDataModel>(pavementcharts));
                default:
                    var allneeds = await LoadSelectedChart<AllPotentialBenefitsModel>(scenId, district, potentialBenefits!.AllPotentialBenefitsRepo);
                    return Json(_mapper.Map<ChartsDataModel>(allneeds));
            }
        }
        [HttpPost]
        public async Task<IActionResult> LoadBudgetCharts(int? scenId, int? district)
        {
            if (!scenId.HasValue && !district.HasValue)
                return BadRequest("You need to select a scenario and a district first");
            if (!scenId.HasValue)
                return BadRequest("You need to select a scenario");
            if (!district.HasValue)
                return BadRequest("You need to select a district");
            var budgetRepo = HttpContext.RequestServices.GetRequiredService<IRepository<BudgetModel>>();
            var result = await LoadSelectedChart(scenId, district, budgetRepo);
            if (result.Count <= 0)
                return Json(null);
            return Json(_mapper.Map<ChartsDataModel>(result));
        }
        [HttpPost]
        public async Task<IActionResult> LoadBudgetSpentCharts(int? scenId, int? district)
        {
            if (!scenId.HasValue && !district.HasValue)
                return BadRequest("You need to select a scenario and a district first");
            if (!scenId.HasValue)
                return BadRequest("You need to select a scenario");
            if (!district.HasValue)
                return BadRequest("You need to select a district");
            var budgetSpent = HttpContext.RequestServices.GetRequiredService<IRepository<BudgetSpentModel>>();
            var result = await LoadSelectedChart(scenId, district, budgetSpent);
            if (result.Count <= 0)
                return Json(null);
            return Json(_mapper.Map<ChartsDataModel>(result));
        }
        private async Task<List<T>> LoadSelectedChart<T>(int? scenId, int? district, IRepository<T> repository) where T : class
        {
            var filter = new Dictionary<string, object>()
            {
                { "ScenarioId", scenId },
                { "District", district }
            };
            repository.ApplyFilter(filter);
            var result = await repository.GetAllAsync();
            return result;
        }
    }
}
