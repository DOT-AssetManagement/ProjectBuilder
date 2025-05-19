using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class BudgetSpentViewModel : ObservableObject
    {
        private readonly IRepository<BudgetSpentModel> _budgetSpentRepo;
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private readonly IRepository<CountyModel> _countyRepo;
        private Dictionary<string, object> _budgetSpentFilter;
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
        public BudgetSpentViewModel(IRepository<BudgetSpentModel> budgetSpentRepo, IRepository<ScenarioModel> scenarioRepo, IRepository<CountyModel> countiesRepo)
        {
            _budgetSpentRepo = budgetSpentRepo;
            _scenarioRepo = scenarioRepo;
            _countyRepo = countiesRepo;
            _budgetSpentFilter = new()
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
                await LoadBudgetSpent();
                _isLoaded = true;
            }
        }
        private async Task LoadBudgetData()
        {
            _budgetSpentRepo.ApplyFilter(_budgetSpentFilter);
            var budgetSpents = await _budgetSpentRepo.GetAllAsync();
            if (budgetSpents.Count <= 0)
            {
                IsChartVisible = false;
                IsDataTextVisible = true;
                return;
            }
            IsChartVisible = true;
            IsDataTextVisible = false;
            var labels = new List<string>(budgetSpents.Count);
            _interstateBridgeBudget.Clear();
            _nonInterstateBridgeBudget.Clear();
            _interstatePavementBudget.Clear();
            _nonInterstatePavementBudget.Clear();
            foreach (var budget in budgetSpents)
            {
                if (!budget.ScenarioYear.HasValue)
                    continue;
                if (!labels.Contains(budget.ScenarioYear.ToString()))
                    labels.Add(budget.ScenarioYear.ToString());
                _interstateBridgeBudget.Add(budget.BridgeBudgetInterstate);
                _nonInterstateBridgeBudget.Add(budget.BridgeBudgetNonInterstate);
                _interstatePavementBudget.Add(budget.PavementBudgetInterstate);
                _nonInterstatePavementBudget.Add(budget.PavementBudgetNonInterstate);
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
        private async Task LoadBudgetSpent()
        {
            _budgetSpentFilter["ScenarioId"] = SelectedScenario is not null ? SelectedScenario.ScenarioId : 0;
            _budgetSpentFilter["District"] = SelectedDistrict.HasValue ? SelectedDistrict.Value : 0;
            await LoadBudgetData();
        }
    }
}
