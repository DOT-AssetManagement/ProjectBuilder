using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class TreatmentSummaryViewModel : DatabaseSourceViewModel<TreatmentSummaryModel>
    {
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private readonly IRepository<CountyModel> _countyRepo;
        private readonly Dictionary<string, object> _treatmentSummaryFilter;
        [ObservableProperty]
        private List<ScenarioModel> _scenarios;
        [ObservableProperty]
        private List<int?> _districts;
        [ObservableProperty]
        private ScenarioModel _selectedScenario;
        [ObservableProperty]
        private int? _selectedDistrict;
        public TreatmentSummaryViewModel(IRepository<TreatmentSummaryModel> treatmentSummaryRepo, IRepository<ScenarioModel> scenarioRepo, IRepository<CountyModel> countyRepo,
                                         IDialogService dialogService,ISettingsService settingsService,IMessenger messenger) 
                                        : base(treatmentSummaryRepo,dialogService,settingsService,messenger)
        {
            _scenarioRepo = scenarioRepo;
            _countyRepo = countyRepo;
            CurrentModelToken = nameof(TreatmentSummaryViewModel);
            _treatmentSummaryFilter = new()
            {
                {"ScenarioId", 9 },
                {"District",8 }
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
        [RelayCommand]
        private async Task LoadPTreatmentSummaryData()
        {
            ApplyFilterParameters();
            await Refresh();
        }
        private void ApplyFilterParameters()
        {
            _treatmentSummaryFilter["ScenarioId"] = SelectedScenario is not null ? SelectedScenario.ScenarioId : 0;
            _treatmentSummaryFilter["District"] = SelectedDistrict.HasValue ? SelectedDistrict.Value : 0;
            CurrentRepository.ApplyFilter(_treatmentSummaryFilter);
        }
    }
}
