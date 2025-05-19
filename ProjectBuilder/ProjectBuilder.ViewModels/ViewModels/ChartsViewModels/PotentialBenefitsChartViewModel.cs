using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class PotentialBenefitsChartViewModel : ObservableObject
    {
        private readonly IRepository<AllPotentialBenefitsModel> _allPotentialBenefits;
        private readonly IRepository<BridgePotentialBenefitsModel> _bridgeBenefitsRepo;
        private readonly IRepository<PavementPotentialBenefitsModel> _pavementBenefitsRepo;
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private readonly IRepository<CountyModel> _countyRepo;
        private Dictionary<string, object> _needsFilter;
        private List<double?> _interstateBenefits = new();
        private List<double?> _nonInterstateBenefits = new();
        [ObservableProperty]
        private List<ScenarioModel> _scenarios;
        [ObservableProperty]
        private List<int?> _districts;
        [ObservableProperty]
        private ScenarioModel _selectedScenario;
        [ObservableProperty]
        private int? _selectedDistrict;
        [ObservableProperty]
        private string[] _labels;
        [ObservableProperty]
        Dictionary<string, List<double?>> _seriesPoint;
        [ObservableProperty]
        private bool _isDataTextVisible;
        [ObservableProperty]
        private bool _isChartVisible;
        [ObservableProperty]
        private int _selectedNeedIndex;
        private bool _isLoaded;
        public PotentialBenefitsChartViewModel(IRepository<AllPotentialBenefitsModel> allPotentialBenefits, IRepository<BridgePotentialBenefitsModel> bridgeBenefitsRepo, IRepository<PavementPotentialBenefitsModel> pavementBenefitsRepo,
                                               IRepository<ScenarioModel> scenarioRepo, IRepository<CountyModel> countiesRepo)
        {
            _allPotentialBenefits = allPotentialBenefits;
            _bridgeBenefitsRepo = bridgeBenefitsRepo;
            _pavementBenefitsRepo = pavementBenefitsRepo;
            _scenarioRepo = scenarioRepo;
            _countyRepo = countiesRepo;
            _needsFilter = new()
            {
                {"ScenarioId", 9 },
                {"District",8 }
            };
        }
        [RelayCommand]
        private async Task Loaded()
        {
            if (!_isLoaded)
            {
                Scenarios = await _scenarioRepo.GetAllAsync();
                SelectedScenario = Scenarios?.FirstOrDefault();
                var counties = await _countyRepo.GetAllAsync();
                Districts = new(counties.Select(c => c.District).Distinct().OrderBy(d => d));
                SelectedDistrict = 8;
                await LoadCharts();
                _isLoaded = true;
            }
        }
        private void LoadCurrentSelectedChartData<T>(List<T> chartData) where T : AllPotentialBenefitsModel
        {
            if (chartData is null || chartData.Count <= 0)
            {
                IsChartVisible = false;
                IsDataTextVisible = true;
                return;
            }
            IsChartVisible = true;
            IsDataTextVisible = false;
            var labels = new List<string>(chartData.Count);
            _interstateBenefits.Clear();
            _nonInterstateBenefits.Clear();
            foreach (var data in chartData)
            {
                if (!data.TreatmentYear.HasValue)
                    continue;
                if (!labels.Contains(data.TreatmentYear.ToString()))
                    labels.Add(data.TreatmentYear.ToString());
                _interstateBenefits.Add(data.InterstateBenefit);
                _nonInterstateBenefits.Add(data.NonInterstateBenefit);
            }
            Labels = labels.ToArray();
            SeriesPoint = new()
            {
                {"Interstate", _interstateBenefits },
                {"Non-Interstate", _nonInterstateBenefits }
            };
        }
        private async Task LoadAllPotentialBenefits()
        {
            _allPotentialBenefits.ApplyFilter(_needsFilter);
            var allNeeds = await _allPotentialBenefits.GetAllAsync();
            LoadCurrentSelectedChartData(allNeeds);
        }
        private async Task LoadBridgePotentialBenefits()
        {
            _bridgeBenefitsRepo.ApplyFilter(_needsFilter);
            var bridgeNeeds = await _bridgeBenefitsRepo.GetAllAsync();
           LoadCurrentSelectedChartData(bridgeNeeds);
        }
        private async Task LoadPavementPotentialBenefits()
        {
            _pavementBenefitsRepo.ApplyFilter(_needsFilter);
            var pavementNeeds = await _pavementBenefitsRepo.GetAllAsync();
            LoadCurrentSelectedChartData(pavementNeeds);
        }
        private async Task LoadSelectedChart()
        {
            switch (SelectedNeedIndex)
            {
                case 1:
                    await LoadBridgePotentialBenefits();
                    break;
                case 2:
                    await LoadPavementPotentialBenefits();
                    break;
                default:
                    await LoadAllPotentialBenefits();
                    break;
            }
        }
        [RelayCommand]
        private async Task LoadCharts()
        {
            _needsFilter["ScenarioId"] = SelectedScenario is not null ? SelectedScenario.ScenarioId : 0;
            _needsFilter["District"] = SelectedDistrict.HasValue ? SelectedDistrict.Value : 0;
            await LoadSelectedChart();
        }
    }
}
