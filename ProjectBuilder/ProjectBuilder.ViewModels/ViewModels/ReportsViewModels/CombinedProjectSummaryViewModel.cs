using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class CombinedProjectViewModel : DatabaseSourceViewModel<CombinedProjectModel>
    {
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private readonly IRepository<CountyModel> _countyRepo;
        private readonly Dictionary<string, object> _combinedProjectFilter;
        [ObservableProperty]
        private List<ScenarioModel> _scenarios;
        [ObservableProperty]
        private List<int?> _districts;
        [ObservableProperty]
        private ScenarioModel _selectedScenario;
        [ObservableProperty]
        private int? _selectedDistrict;
        [ObservableProperty]
        private int _selectedProjectOptionIndex;
        public CombinedProjectViewModel(IRepository<CombinedProjectModel> combinedProjectSummaryRepo, IRepository<ScenarioModel> scenarioRepo, IRepository<CountyModel> countyRepo,
                                        IDialogService dialogService,ISettingsService settingsService,IMessenger messenger)
                                       : base(combinedProjectSummaryRepo,dialogService,settingsService,messenger)
        {
            _scenarioRepo = scenarioRepo;
            _countyRepo = countyRepo;
            CurrentModelToken = nameof(CombinedProjectViewModel);
            _combinedProjectFilter = new()
            {
                {"ScenarioId", 9 },
                {"District",8 },
                {"ProjectType","C" }
            };
        }
        protected async override Task Loaded()
        {
            if (!IsLoaded)
            {
                Scenarios = await _scenarioRepo.GetAllAsync();
                SelectedScenario = Scenarios?.FirstOrDefault();
                var counties = await _countyRepo.GetAllAsync();
                Districts = new(counties.Select(c => c.District).Distinct().OrderBy(d => d));
                SelectedDistrict = 8;
                ApplyFilterParameters();
                await base.Loaded();
            }
        }
        private void ApplyFilterParameters()
        {
            _combinedProjectFilter["ScenarioId"] = SelectedScenario is not null ? SelectedScenario.ScenarioId : 0;
            _combinedProjectFilter["District"] = SelectedDistrict.HasValue ? SelectedDistrict.Value : 0;
            switch (SelectedProjectOptionIndex)
            {
                case 1:
                    _combinedProjectFilter["ProjectType"] = "B";
                    break;
                case 2:
                    _combinedProjectFilter["ProjectType"] = "P";
                    break;
                default:
                    _combinedProjectFilter["ProjectType"] = "C";
                    break;
            }
            CurrentRepository.ApplyFilter(_combinedProjectFilter);
        }
        [RelayCommand]
        private async Task LoadSummaryData()
        {
            ApplyFilterParameters();
            await Refresh();
        }
    }
}
