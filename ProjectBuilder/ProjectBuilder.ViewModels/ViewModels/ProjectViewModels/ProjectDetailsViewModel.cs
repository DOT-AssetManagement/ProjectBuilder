using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Diagnostics.Metrics;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using ProjectBuilder.Core;

namespace ProjectBuilder.ViewModels
{
    public partial class ProjectDetailsViewModel : ViewModelBase, IRecipient<DataModelMessage<ProjectModel>>,IRecipient<DataModelMessage<DialogResult>>
    {
        #region Fields
        private readonly IRepository<ScenarioModel> _scenarioRepo;
        private readonly IRepository<CountyModel> _countiesRepo;
        private readonly IFilterUnitOfWork _filterUnitOfWork;
        private readonly IMessenger _messenger;
        private readonly ISettingsService _settings;
        private readonly IDialogService _dialogService;
        private readonly ITreatmentRepository _treatmentRepo;
        private readonly IRepository<ProjectModel> _projectsRepo;
        private bool _isloaded = false;
        private ProjectModel _editedProject = null;
        private bool _isSettingsValues = false;
        private List<CountyModel> _availableCounties;
        private CancellationTokenSource _tokenSource;
        private bool _tokenDisposed;
        private const string _modelToken = "ProjectDetailsViewModel";
        private Dictionary<string, object> _filter;
        private TreatmentModel _selectedRow;
        #endregion

        #region Observable Properties
        [ObservableProperty]
        private bool _waitingIndicator;
        [ObservableProperty]
        private bool _isPaginationEnabled;
        [ObservableProperty]
        private IEnumerable<int> _pages;
        [ObservableProperty]
        private int _dataGridRaduis;
        [ObservableProperty]
        private bool _isDataGridHitTestVisible;
        [ObservableProperty]
        private bool _isRefreshEnabled;    
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
        private ObservableCollection<int> _years;
        [ObservableProperty]
        private ObservableCollection<bool> _commitmentStatus;
        [ObservableProperty]
        [Required(AllowEmptyStrings =false,ErrorMessage ="Select Year")]
        private int? _selectedYear;
        [ObservableProperty]
        private int _selectedCommitmentStatusIndex;
        [ObservableProperty]
        ObservableCollection<TreatmentModel> _dataSource;
        [ObservableProperty]
        ObservableCollection<int?> _sections;
        #endregion

        #region Properties
        private ScenarioModel _selectedScenario;
        [Required(AllowEmptyStrings =false,ErrorMessage ="Select Scenario")]
        public  ScenarioModel SelectedScenario
        {
            get { return _selectedScenario; }
            set 
            {
               if(SetProperty(ref _selectedScenario,value) && _isloaded && !_isSettingsValues) 
               {
                    Projects?.Clear();
                    if (value is null)
                        return;
                   // _projectsRepo.ScenarioId = value.ScenarioId;
                    _projectsRepo.GetAllAsync().ContinueWith(task =>
                    {
                        if(task.IsCompletedSuccessfully)
                           Projects = new(task.Result.DistinctBy(p => p.ProjectId));
                    },CancellationToken.None,TaskContinuationOptions.None,TaskScheduler.FromCurrentSynchronizationContext());
               }
            }
        }

        private ProjectModel _selectedProject;   
        [Required(AllowEmptyStrings = false,ErrorMessage ="Select Project")]
        public ProjectModel SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if(SetProperty(ref _selectedProject, value) && value is not null && _isloaded && !_isSettingsValues)
                {
                  //  SetLoadingProperties(true);
                    SelectedDistrict = value.District;
                    SelectedCounty = Counties.FirstOrDefault(c => c.CountyFullName == value.County);
                    SelectedYear = value.SelectedFirstYear;
                    SetCommittmentStatus(value.CommitmentStatus);             
                }
            }
        }
        private int? _selectedDistrict;
        [Required(AllowEmptyStrings = false,ErrorMessage ="Select District")]
        public int? SelectedDistrict
        {
            get { return _selectedDistrict; }
            set
            {
                if(SetProperty(ref _selectedDistrict,value) && _isloaded)
                {
                    Counties = new(_availableCounties.Where(c => c.District == value).OrderBy(c => c.CountyId));
                }
            }
        }
        private CountyModel _selectedCounty;
        [Required(AllowEmptyStrings =false,ErrorMessage ="Select County")]
        public CountyModel SelectedCounty
        {
            get { return _selectedCounty; }
            set
            {
                if(SetProperty(ref _selectedCounty,value) && _isloaded && !_isSettingsValues)
                {
                     _treatmentRepo.GetRoutesAsync(SelectedDistrict, value?.CountyId).ContinueWith(task =>
                     {
                         Routes?.Clear();
                         if (task.IsCompletedSuccessfully)
                             Routes = new(task.Result);
                     }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
        [ObservableProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select Route")]
        private int? _selectedRoute;
        protected CancellationToken CurrentCancellationToken
        {
            get
            {
                InitializeTokenSource();
                return _tokenSource.Token;
            }
        }

       

        #endregion


        public ProjectDetailsViewModel(IFilterUnitOfWork filterUnitOfWork,IMessenger messenger,ISettingsService settings,
                                       IDialogService dialogService)                                                               
        {
            _filterUnitOfWork = filterUnitOfWork;
            _scenarioRepo = filterUnitOfWork.ScenarioRepo;
            _countiesRepo = filterUnitOfWork.CountyRepo;
            _messenger = messenger;
            _settings = settings;
            _dialogService = dialogService;
            _treatmentRepo = filterUnitOfWork.TreatmentRepo;
            _projectsRepo = filterUnitOfWork.ProjectRepo;
            messenger.RegisterAll(this);
            Years = new(Enumerable.Range(2010,41));
        }
        public void Receive(DataModelMessage<ProjectModel> message)
        {
            _editedProject = message.Parameter;
        }
        [RelayCommand]
        protected  async Task Loaded()
        {
            _filterUnitOfWork.ErrorOccured += OnErrorOccured;
            if (!_messenger.IsRegistered<DataModelMessage<DialogResult>, string>(this, _modelToken))
                _messenger.Register<DataModelMessage<DialogResult>, string>(this, _modelToken);
            SetLoadingProperties(true);
            if (!_isloaded)
            {
              Scenarios = new(await _scenarioRepo.GetAllAsync());
              _availableCounties = await _countiesRepo.GetAllAsync();
              Districts = new(_availableCounties.OrderBy(d => d.District).Select(d => d.District).Distinct());      
            }
            _isloaded = true;
            await LoadSelectedProject();
            SetLoadingProperties(false);
        }
        private void InitializeTokenSource()
        {
            if (_tokenSource is null || _tokenSource.IsCancellationRequested || _tokenDisposed)
            {
                _tokenSource = new();
                _tokenDisposed = false;
            }
        }
        private void OnErrorOccured(ErrorEventsArgs error)
        {
            ShowMessage(error.ErrorMessage, error.Level);
        }

        [RelayCommand]
        protected void Unloaded()
        {
            _filterUnitOfWork.ErrorOccured -= OnErrorOccured;
            _messenger.Unregister<DataModelMessage<DialogResult>, string>(this, _modelToken);
            _tokenSource.Dispose();
            _tokenSource = null;
            _tokenDisposed= true;
            _editedProject = null;
        }
        private async Task LoadSelectedProject()
        {
            if (_editedProject is not null)
            {                
                _isSettingsValues = true;
                Projects = new(await _projectsRepo.GetAllAsync(CurrentCancellationToken));
                SelectedScenario = Scenarios.FirstOrDefault(sc => sc.ScenarioId == _editedProject.ScenarioId);
                SelectedProject = Projects.FirstOrDefault(p => p.ProjectId == _editedProject.ProjectId);
                SelectedDistrict = Districts.FirstOrDefault(d => d == _editedProject.District);
                Counties = new(_availableCounties.Where(c => c.District == SelectedDistrict).OrderBy(c => c.CountyId));
                SelectedCounty = Counties.FirstOrDefault(c => c.CountyFullName == _editedProject.County);
                var routes = await _treatmentRepo.GetRoutesAsync(SelectedDistrict, SelectedCounty?.CountyId, CurrentCancellationToken);
                Routes = new(routes);
                SelectedRoute = Routes.FirstOrDefault(r => r == _editedProject.Route,-1);
                SelectedYear = Years.FirstOrDefault(y => y == _editedProject.SelectedFirstYear,-1);
                SetCommittmentStatus(_editedProject.CommitmentStatus);
                var filter = new TreatmentModel
                {
                    ScenarioId = _editedProject.ScenarioId,
                    ProjectId = _editedProject.ProjectId,
                    District = _editedProject.District.HasValue ? _editedProject.District.Value : (byte)0,
                    CountyId = _editedProject.CountyId.HasValue  ? (byte)_editedProject.CountyId.Value : (byte)0,
                    SelectedYear = _editedProject.SelectedFirstYear,
                    Route = _editedProject.Route.HasValue ? _editedProject.Route.Value : 0,
                    IsCommitted = _editedProject.CommitmentStatus
                };
                _treatmentRepo.ApplyFilter(filter);
                DataSource = new(await _treatmentRepo.GetAllAsync(CurrentCancellationToken));
                _isSettingsValues = false;    
            }
        }
        private void SetCommittmentStatus(bool? value)
        {
            if (value.HasValue)
                SelectedCommitmentStatusIndex = value.Value ? 0 : 1;
            else SelectedCommitmentStatusIndex = -1;
        }
        protected void SetLoadingProperties(bool isOn)
        {
            IsPaginationEnabled = !isOn;
            IsRefreshEnabled = !isOn;
            WaitingIndicator = isOn;
            IsDataGridHitTestVisible = !isOn;
            DataGridRaduis = isOn ? 10 : 0;
        }
        [RelayCommand]
        private void CancelLoading()
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenDisposed= true;
                _tokenSource = null;
                InitializeTokenSource();
            }
            SetLoadingProperties(false);
        }
        [RelayCommand]
        private async Task FilterTreatments()
        {
            SetLoadingProperties(true);             
            var treatment = new TreatmentModel
            {
                ScenarioId = SelectedScenario is null ? 0 : SelectedScenario.ScenarioId,
                ProjectId = SelectedProject is null ? null : SelectedProject.ProjectId,
                District = (byte?)SelectedDistrict,
                CountyId = SelectedCounty is null ? (byte)0 : (byte)(SelectedCounty.CountyId.HasValue ? (byte)SelectedCounty.CountyId.Value : 0),
                SelectedYear = SelectedYear,
                Route = SelectedRoute.HasValue ? SelectedRoute.Value : 0,
                IsCommitted = SelectedCommitmentStatusIndex > -1 ? (SelectedCommitmentStatusIndex == 0) : null
            };
            _treatmentRepo.ApplyFilter(treatment);
            DataSource = new(await _treatmentRepo.GetAllAsync());
            if(DataSource.Count < 1)
                ShowMessage("No Treatments were Found.",LogLevel.Warning);
            SetLoadingProperties(false);
        }
        [RelayCommand]
        private void BackToProjects()
        {
            _messenger.Send(new ChangeCurrentViewMessage(CurrentView.ProjectsView));
        }
        [RelayCommand]
        private void SelectedRowChanged(IList items)
        {
            if (items?.Count > 0)
                _selectedRow = (TreatmentModel)items[0];
        }
        [RelayCommand]
        private void EditRow()
        {
            _messenger.Send(new ChangeCurrentViewMessage(CurrentView.EditTreatmentView));
            _messenger.Send(new DataModelMessage<TreatmentModel>(_selectedRow,CurrentView.ProjectDetailsView));
        }
        [RelayCommand]
        private async Task DeleteRow()
        {
            bool withConfirmation = _settings.GetSettingValue<bool>(SettingName.ShowDeleteConfirmationMessage);
            if (withConfirmation)
                await _dialogService.ShowDialog(CurrentView.ConfirmationMessageView, Constants.MAINDIALOGIDENTIFIER, OnDeleteDialogOpened, null);
        }
        [RelayCommand]
        private void CreateTreatment()
        {
            _messenger.Send(new ChangeCurrentViewMessage(CurrentView.NewTreatmentView));
            _messenger.Send(new DataModelMessage<TreatmentModel>(null, CurrentView.ProjectDetailsView));
        }
        private void OnDeleteDialogOpened()
        {
            var configuration = new ConfigurationMessage();
            configuration = configuration.DeleteConfiguration();    
            _messenger.Send(configuration);
        }

        public async void Receive(DataModelMessage<DialogResult> message)
        {
            if(message.Parameter == DialogResult.Ok)
            {
                _treatmentRepo.Delete(_selectedRow,CurrentCancellationToken);
                await _treatmentRepo.SaveChangesAsync(CurrentCancellationToken);
                _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER);
            }
        }
    }
}

