using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.Charts;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Models.VisualizationModels;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(ChartsPermissions.Charts.Default)]
    public class ChartsAppService
    {
        private readonly IRunScenarioRepository _runScenarioRepository;
        private readonly ICountyRepository _countyRepository;
        private readonly IChartsNeedsRepository _chartsNeedsRepository;
        private readonly IChartsPotentialBenefitsUnitOfWork _chartsPotentialBenefitsUnitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IObjectMapper _mapper;
        public ChartsAppService(IRunScenarioRepository runScenarioRepository, ICountyRepository countyRepository, IChartsNeedsRepository chartsNeedsRepository, IChartsPotentialBenefitsUnitOfWork chartsPotentialBenefitsUnitOfWork, IHttpContextAccessor httpContextAccessor, IObjectMapper mapper)

        {
            _runScenarioRepository = runScenarioRepository;
            _countyRepository = countyRepository;
            _chartsNeedsRepository = chartsNeedsRepository;
            _chartsPotentialBenefitsUnitOfWork = chartsPotentialBenefitsUnitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<ChartsDto> LoadChartViewModel(int? scenid)
        {
            ChartsDto chartsViewModel = new();
            chartsViewModel.ScenarioId = scenid;
            if (scenid.HasValue)
            {
                var scenario = await _runScenarioRepository.FindAsync(scenid.Value);
                chartsViewModel.ScenarioName = scenario is null ? "" : scenario.ScenarioName;
            }
            var counties = await _countyRepository.GetListAsync();
            chartsViewModel.Districts = counties.Select(c => c.District).Distinct().OrderBy(d => d);
            return chartsViewModel;
        }
        public async Task<ChartsDataModel> LoadNeedCharts(int? scenId, int? district, int? asset)
        {
            if (!scenId.HasValue && !district.HasValue)
                throw new UserFriendlyException("You need to select a scenario and a district first");
            if (!scenId.HasValue)
                throw new UserFriendlyException("You need to select a scenario");
            if (!district.HasValue)
                throw new UserFriendlyException("You need to select a district");
            var needsUnit = _chartsNeedsRepository;
            switch (asset)
            {
                case 0:
                    var bridgecharts = await LoadSelectedChart<BridgeNeedsModel>(scenId, district, needsUnit.BridgeNeedsRepo);
                    return _mapper.Map<List<BridgeNeedsModel>, List<ChartsDataModel>>(bridgecharts).FirstOrDefault();

                case 1:
                    var pavementcharts = await LoadSelectedChart<PavementNeedsModel>(scenId, district, needsUnit.PavementNeedsRepo);
                    return _mapper.Map<List<PavementNeedsModel>, List<ChartsDataModel>>(pavementcharts).FirstOrDefault();
                default:
                    var allneeds = await LoadSelectedChart<AllNeedsModel>(scenId, district, needsUnit.AllNeedsRepo);
                    return _mapper.Map<List<AllNeedsModel>, List<ChartsDataModel>>(allneeds).FirstOrDefault();
            }
        }
        public async Task<ChartsDataModel> LoadPotentialBenefitsCharts(int? scenId, int? district, int? asset)
        {
            if (!scenId.HasValue && !district.HasValue)
                throw new UserFriendlyException("You need to select a scenario and a district first");
            if (!scenId.HasValue)
                throw new UserFriendlyException("You need to select a scenario");
            if (!district.HasValue)
                throw new UserFriendlyException("You need to select a district");
            var potentialBenefits = _chartsPotentialBenefitsUnitOfWork;
            switch (asset)
            {
                case 0:
                    var bridgecharts = await LoadSelectedChart<BridgePotentialBenefitsModel>(scenId, district, potentialBenefits!.BridgePotentialBenefitsRepo);
                    return _mapper.Map<List<BridgePotentialBenefitsModel>, List<ChartsDataModel>>(bridgecharts)
                        .FirstOrDefault();
                case 1:
                    var pavementcharts = await LoadSelectedChart<PavementPotentialBenefitsModel>(scenId, district, potentialBenefits!.PavementPotentialBenefitsRepo);
                    return _mapper.Map<List<PavementPotentialBenefitsModel>, List<ChartsDataModel>>(pavementcharts)
                          .FirstOrDefault();
                default:
                    var allneeds = await LoadSelectedChart<AllPotentialBenefitsModel>(scenId, district, potentialBenefits!.AllPotentialBenefitsRepo);
                    return _mapper.Map<List<AllPotentialBenefitsModel>, List<ChartsDataModel>>(allneeds)
                         .FirstOrDefault();
            }
        }
        public async Task<ChartsDataModel> LoadBudgetCharts(int? scenId, int? district)
        {
            if (!scenId.HasValue && !district.HasValue)
                throw new UserFriendlyException("You need to select a scenario and a district first");
            if (!scenId.HasValue)
                throw new UserFriendlyException("You need to select a scenario");
            if (!district.HasValue)
                throw new UserFriendlyException("You need to select a district");
            var budgetRepo = _httpContextAccessor.HttpContext?.RequestServices.GetRequiredService<ICustomRepository<BudgetModel>>();
            var result = await LoadSelectedChart(scenId, district, budgetRepo);
            if (result.Count <= 0)
                return null;
            var mapped = _mapper.Map<List<BudgetModel>, List<ChartsDataModel>>(result);
            return mapped.FirstOrDefault();
        }
        //public async Task<ChartsDataModel> LoadBudgetSpentCharts(int? scenId, int? district)
        //{
        //    if (!scenId.HasValue && !district.HasValue)
        //        throw new UserFriendlyException("You need to select a scenario and a district first");
        //    if (!scenId.HasValue)
        //        throw new UserFriendlyException("You need to select a scenario");
        //    if (!district.HasValue)
        //        throw new UserFriendlyException("You need to select a district");
        //    var budgetSpent = _httpContextAccessor.HttpContext?.RequestServices.GetRequiredService<ICustomRepository<BudgetSpentModel>>();
        //    var result = await LoadSelectedChart(scenId, district, budgetSpent);
        //    if (result.Count <= 0)
        //        return null;
        //    return _mapper.Map<ChartsDataModel>(result);
        //}
        private async Task<List<T>> LoadSelectedChart<T>(int? scenId, int? district, ICustomRepository<T> repository) where T : class
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
