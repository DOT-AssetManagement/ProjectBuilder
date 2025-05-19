using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class FilterBaseViewModel<TFilter> : DatabaseSourceViewModel<TFilter> where TFilter : class
    {
        private readonly IRepository<ProjectModel> _projectsRepo;
        private readonly ITreatmentRepository _treatmentRepo;
        private readonly IRepository<CountyModel> _countyRepo;
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private ProjectModel _projectFilter = new();
        private List<CountyModel> _availableCounties;
        [ObservableProperty]
        private ObservableCollection<ScenarioModel> _scenarios;
        [ObservableProperty]
        private ObservableCollection<ProjectModel> _projects;
        [ObservableProperty]
        private ObservableCollection<int?> _districts;
        [ObservableProperty]
        private ObservableCollection<CountyModel> _counties;
        [ObservableProperty]
        private ObservableCollection<int> _routes;
        [ObservableProperty]
        private ObservableCollection<string> _sections;
        [ObservableProperty]
        private List<int> _years;
        [ObservableProperty]
        private bool _isFilterOptionsEnabled;

        #region Filter Properties

        private ScenarioModel _selectedScenario;
        public ScenarioModel SelectedScenario
        {
            get { return _selectedScenario; }
            set
            {
                if (SetProperty(ref _selectedScenario, value) && IsLoaded)
                {
                    Projects?.Clear();
                    if (value is null)
                        return;
                    IsFilterOptionsEnabled = false;
                    _projectFilter.ScenarioId = value.ScenarioId;
                    _projectsRepo.ApplyFilter(_projectFilter);
                    _projectsRepo.GetAllAsync().ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                            Projects = new(task.Result);
                        IsFilterOptionsEnabled = true;
                    }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
        [ObservableProperty]
        private ProjectModel _selectedProject;

        private int? _selectedDistrict;
        public int? SelectedDistrict
        {
            get { return _selectedDistrict; }
            set
            {
                if (SetProperty(ref _selectedDistrict, value) && IsLoaded)
                {
                    Counties = new(_availableCounties.Where(c => c.District == value).OrderBy(c => c.CountyId));
                }
            }
        }
        private CountyModel _selectedCounty;
        public CountyModel SelectedCounty
        {
            get { return _selectedCounty; }
            set
            {
                if (SetProperty(ref _selectedCounty, value) && IsLoaded)
                {
                    IsFilterOptionsEnabled = false;
                    _treatmentRepo.GetRoutesAsync(SelectedDistrict, value?.CountyId).ContinueWith(task =>
                    {
                        Routes?.Clear();
                        if (task.IsCompletedSuccessfully)
                            Routes = new(task.Result);
                        IsFilterOptionsEnabled = true;
                    }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
        private int? _selectedRoute;
        public int? SelectedRoute
        {
            get { return _selectedRoute; }
            set
            {
                if (SetProperty(ref _selectedRoute, value) && IsLoaded)
                {
                    IsFilterOptionsEnabled = false;
                    _treatmentRepo.GetSectionsAsync(SelectedDistrict, SelectedCounty?.CountyId, SelectedRoute).ContinueWith(task =>
                    {
                        Sections?.Clear();
                        if (task.IsCompletedSuccessfully)
                            Sections = new(task.Result);
                        IsFilterOptionsEnabled = true;
                    }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
        [ObservableProperty]
        private int? _selectedYear;
        [ObservableProperty]
        private string _selectedSection;
        #endregion

        protected FilterBaseViewModel(IFilterUnitOfWork filter, IRepository<TFilter> repository, IDialogService dialogService, ISettingsService settingsService, IMessenger messenger)
                               : base(repository, dialogService, settingsService, messenger)
        {
            _projectsRepo = filter.ProjectRepo;
            _treatmentRepo = filter.TreatmentRepo;
            _countyRepo = filter.CountyRepo;
            _scenarioRepo = filter.ScenarioRepo;
            IsFilterOptionsEnabled = true;
        }
        protected override async Task Loaded()
        {
            _scenarioRepo.ErrorOccured += OnErrorOccured;
            InitializeTokenSource();
            if (!IsLoaded)
            {
                Years = new(Enumerable.Range(2010, 41));
                Scenarios = new(await _scenarioRepo.GetAllAsync());
                _availableCounties = new(await _countyRepo.GetAllAsync());
                Districts = new(_availableCounties.Select(c => c.District).OrderBy(d => d).Distinct());
            }
            await base.Loaded();
        }
        protected override Task Unloaded()
        {
            _scenarioRepo.ErrorOccured -= OnErrorOccured;
            return base.Unloaded();
        }
        protected override async Task Refresh()
        {
            IsFilterOptionsEnabled = false;
            await base.Refresh();
            IsFilterOptionsEnabled = true;
        }
        [RelayCommand]
        protected async virtual Task Filter()
        {
            ElementCount = await CurrentRepository.GetCountAsync(CurrentToken);
            InitializePaginationHelper();
            await LoadSource();
        }
    }
}
