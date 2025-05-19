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
    public partial class BudgetChartViewModel : ObservableObject
    {
        private readonly IRepository<BudgetModel> _budgetRepo;
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private readonly IRepository<CountyModel> _countyRepo;
        private Dictionary<string, object> _budgetFilter;
        private List<double?> _interstateBridgeBudget = new();
        private List<double?> _nonInterstateBridgeBudget = new();
        private List<double?> _interstatePavementBudget = new();
        private List<double?> _nonInterstatePavementBudget = new();
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
        public BudgetChartViewModel(IRepository<BudgetModel> budgetRepo, IRepository<ScenarioModel> scenarioRepo, IRepository<CountyModel> countiesRepo)
        {
            _budgetRepo = budgetRepo;
            _scenarioRepo = scenarioRepo;
            _countyRepo = countiesRepo;
            _budgetFilter = new()
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
        private async Task LoadBudgetData()
        {
            _budgetRepo.ApplyFilter(_budgetFilter);
            var budgets = await _budgetRepo.GetAllAsync();
            if (budgets.Count <= 0)
            {
                IsChartVisible = false;
                IsDataTextVisible = true;
                return;
            }
            IsChartVisible = true;
            IsDataTextVisible = false;
            var labels = new List<string>(budgets.Count);
            _interstateBridgeBudget.Clear();
            _nonInterstateBridgeBudget.Clear();
            _interstatePavementBudget.Clear();
            _nonInterstatePavementBudget.Clear();
            foreach (var budget in budgets)
            {
                if (!budget.ScenarioYear.HasValue)
                    continue;
                if (!labels.Contains(budget.ScenarioYear.ToString()))
                    labels.Add(budget.ScenarioYear.ToString());
                _interstateBridgeBudget.Add(budget.BridgeBudgetInterstate.HasValue ? (double)budget.BridgeBudgetInterstate.Value : null);
                _nonInterstateBridgeBudget.Add(budget.BridgeBudgetNonInterstate.HasValue ? (double)budget.BridgeBudgetNonInterstate.Value : null);
                _interstatePavementBudget.Add(budget.PavementBudgetInterstate.HasValue ? (double)budget.PavementBudgetInterstate.Value : null);
                _nonInterstatePavementBudget.Add(budget.PavementBudgetNonInterstate.HasValue ? (double)budget.PavementBudgetNonInterstate.Value : null);
            }
            Labels = labels.ToArray();
            SeriesPoint = new()
            {
                {"Bridge Interstate", _interstateBridgeBudget },
                {"Bridge Non-Interstate", _nonInterstateBridgeBudget },
                {"Pavement Interstate", _interstatePavementBudget },
                {"Pavement Non-Interstate", _nonInterstatePavementBudget }
            };
        }
        [RelayCommand]
        private async Task LoadCharts()
        {
            _budgetFilter["ScenarioId"] = SelectedScenario is not null ? SelectedScenario.ScenarioId : 0;
            _budgetFilter["District"] = SelectedDistrict.HasValue ? SelectedDistrict.Value : 0;
            await LoadBudgetData();
        }
    }
}
