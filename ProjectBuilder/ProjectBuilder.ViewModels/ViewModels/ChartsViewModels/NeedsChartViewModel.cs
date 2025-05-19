using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class NeedsChartViewModel : ObservableObject
    {
        private readonly IRepository<AllNeedsModel> _allNeedsRepo;
        private readonly IRepository<BridgeNeedsModel> _bridgeNeedsRepo;
        private readonly IRepository<PavementNeedsModel> _pavementNeedsRepo;
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private readonly IRepository<CountyModel> _countyRepo;
        private Dictionary<string, object> _needsFilter;
        private List<double?> _interstateCost = new();
        private List<double?> _nonInterstateCost = new();
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
        Dictionary<string,List<double?>> _seriesPoint;
        [ObservableProperty]
        private bool _isDataTextVisible;
        [ObservableProperty]
        private bool _isChartVisible;
        [ObservableProperty]
        private int _selectedNeedIndex;
        private bool _isLoaded;
        public NeedsChartViewModel(IRepository<AllNeedsModel> allNeedsRepo, IRepository<BridgeNeedsModel> bridgeNeedsRepo, IRepository<PavementNeedsModel> pavementNeedsRepo,
                                   IRepository<ScenarioModel> scenarioRepo,IRepository<CountyModel> countiesRepo)
        {
            _allNeedsRepo = allNeedsRepo;
            _bridgeNeedsRepo = bridgeNeedsRepo;
            _pavementNeedsRepo = pavementNeedsRepo;
            _scenarioRepo = scenarioRepo;
            _countyRepo= countiesRepo;
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
        private void LoadCurrentSelectedChartData<T>(List<T> chartdata) where T : AllNeedsModel
        {
            if (chartdata is null || chartdata.Count <= 0)
            {
                IsChartVisible = false;
                IsDataTextVisible = true;
                return;
            }
            IsChartVisible = true;
            IsDataTextVisible = false;
            var labels = new List<string>(chartdata.Count);
            _interstateCost.Clear();
            _nonInterstateCost.Clear();
            foreach (var data in chartdata)
            {
                if (!data.TreatmentYear.HasValue)
                    continue;
                if (!labels.Contains(data.TreatmentYear.ToString()))
                    labels.Add(data.TreatmentYear.ToString());
                _interstateCost.Add(data.InterstateCost);
                _nonInterstateCost.Add(data.NonInterstateCost);
            }
            Labels = labels.ToArray();
            SeriesPoint = new()
            {
                {"Interstate", _interstateCost },
                {"Non-Interstate", _nonInterstateCost }
            };
        }
        private async Task LoadAllNeeds()
        {
            _allNeedsRepo.ApplyFilter(_needsFilter);
            var allNeeds = await _allNeedsRepo.GetAllAsync();
            LoadCurrentSelectedChartData(allNeeds);
        }
        private async Task LoadBridgeNeeds()
        {
            _bridgeNeedsRepo.ApplyFilter(_needsFilter);
            var bridgeNeeds = await _bridgeNeedsRepo.GetAllAsync();
            LoadCurrentSelectedChartData(bridgeNeeds);
        }
        private async Task LoadPavementNeeds()
        {
            _pavementNeedsRepo.ApplyFilter(_needsFilter);
            var pavementNeeds = await _pavementNeedsRepo.GetAllAsync();
            LoadCurrentSelectedChartData(pavementNeeds);
        }
        private async Task LoadSelectedChart()
        {
            switch (SelectedNeedIndex)
            {
                case 1:
                    await LoadBridgeNeeds();
                    break;
                case 2:
                    await LoadPavementNeeds();
                    break;
                default:
                    await LoadAllNeeds();
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
