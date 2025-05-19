using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Threading;
using ProjectBuilder.Core;

namespace ProjectBuilder.ViewModels
{
    public partial class RunScenarioViewModel : ViewModelBase,IRecipient<DataModelMessage<DialogResult>>,IRecipient<DataModelMessage<ScenarioModel>>
    {
        #region Fields
        private const string _runViewModelToken = "RunScenario";
        private readonly IRunScenarioUnitOfWork _runScenraioUnitOfWork;
        private readonly IDialogService _dialogService;
        private readonly IMessenger _messenger;
        private ScenarioParameterModel _selectedScenarioParameter;
        private ScenarioBudgetFlatModel _selectedBudget;
        private CancellationTokenSource _tokenSource;
        private bool _isloaded = false;
        private ScenarioModel _newScenarioModel = null;
        private bool _saveNeededBeforeRun = false;
        private bool _autoSave = false;
        private bool _errorOccured = false;
        private ConfigurationMessage _configurationMessage;

        #endregion

        #region Properties
        [ObservableProperty]
        private string _runContent;
        [ObservableProperty]
        private List<ScenarioModel> _scenarios;
        [ObservableProperty]
        private ObservableCollection<ScenarioParameterModel> _scenarioParameters;
        [ObservableProperty]
        private double _radius;
        [ObservableProperty]
        private bool _isHitTestVisible;
        [ObservableProperty]
        private bool _waitingIndicator;    
        [ObservableProperty]
        private ObservableCollection<ScenarioBudgetFlatModel> _budgetConstraints;

        private ScenarioModel _selectedScenario;
        public  ScenarioModel SelectedScenario
        {
            get { return _selectedScenario; }
            set
            {
                if(SetProperty(ref _selectedScenario, value) && _isloaded)
                {
                    SetLoadProperties(true);
                    RunScenarioCommand?.NotifyCanExecuteChanged();
                    GetScenarioInfo(value is not null ? value.ScenarioId : -1).ContinueWith(task => SetLoadProperties(false));
                }
            }
        }
        #endregion

        #region Commands
        public IAsyncRelayCommand RunScenarioCommand { get;}

        public IAsyncRelayCommand SaveChangesCommand { get;}

        #endregion

        #region Constructor
        public RunScenarioViewModel(IRunScenarioUnitOfWork runScenarioUnitofWork,IDialogService dialogService,IMessenger messenger)
        {
            _runScenraioUnitOfWork = runScenarioUnitofWork;
            _dialogService = dialogService;
            _messenger = messenger;
            RunContent = "Run";
            Radius = 0;
            _configurationMessage = new(_runViewModelToken);         
            RunScenarioCommand = new AsyncRelayCommand(RunScenario, () => SelectedScenario is not null);
            SaveChangesCommand = new AsyncRelayCommand(SaveChanges, () => _saveNeededBeforeRun);
            IsHitTestVisible = true;
            _messenger.Register<DataModelMessage<ScenarioModel>>(this);
        }
        #endregion

        #region Commands CallBacks
        [RelayCommand]
        private async Task Loaded()
        {
            //Refresh the scenarios after any scenario is deleted.
            _tokenSource = new();
            _runScenraioUnitOfWork.ErrorOccured += OnErrorOccured;
            SaveChangesCommand?.NotifyCanExecuteChanged();
            if (!_messenger.IsRegistered<DataModelMessage<DialogResult>, string>(this, _runViewModelToken))
                _messenger.Register<DataModelMessage<DialogResult>,string>(this, _runViewModelToken);
            if (!_isloaded)
            {           
                SetLoadProperties(true);
                Scenarios = await _runScenraioUnitOfWork.ScenariosRepo.GetAllAsync(_tokenSource.Token);        
                RunScenarioCommand?.NotifyCanExecuteChanged();
                SetLoadProperties(false);
                _isloaded = true;
            }
            if (_newScenarioModel is not null)
                SelectedScenario = Scenarios?.FirstOrDefault(sc => sc.ScenarioId == _newScenarioModel?.ScenarioId);
            else
                SelectedScenario = Scenarios?.FirstOrDefault();
        }
        [RelayCommand]
        private void CancelLoading()
        {
            CancelCurrentOperation();
        }
        [RelayCommand]
        private async Task Unloaded()
        {
            _runScenraioUnitOfWork.ErrorOccured -= OnErrorOccured;
            _newScenarioModel = null;
            if (_runScenraioUnitOfWork.IsPending)
            {
                _configurationMessage = _configurationMessage.SaveConfiguration(false);
                _autoSave = false;
                await _dialogService.ShowDialog(nameof(ConfirmationMessageViewModel), Constants.MAINDIALOGIDENTIFIER, OnSaveDialogOpen, OnSaveDialogClose);
            }
            _messenger.Unregister<DataModelMessage<DialogResult>, string>(this, _runViewModelToken);
            _tokenSource.Dispose();
        }
        [RelayCommand]
        private void SelectedScenarioParameterChanged(IList items)
        {
            if (items?.Count > 0)
                _selectedScenarioParameter = (ScenarioParameterModel)items[0];
        }
        [RelayCommand]
        private void SelectedBudgetRowChanged(IList items)
        {
            if (items?.Count > 0)
                _selectedBudget = (ScenarioBudgetFlatModel)items[0];
        }
        [RelayCommand]
        private async Task EditScenarioParameter()
        {
            if (_selectedScenarioParameter is not null)
                await _dialogService.ShowDialog(nameof(EditScenarioParameterViewModel), Constants.MAINDIALOGIDENTIFIER, OnEditScenarioParameterDialogOpen, OnEditScenarioParameterDialogClose);
        }    
        [RelayCommand]
        private async Task EditBudget()
        {
            if (_selectedBudget is not null)
             await _dialogService.ShowDialog(nameof(EditBudgetViewModel), Constants.MAINDIALOGIDENTIFIER, OnEditBudgetDialogOpen, OnEditBudgetDialogClose);
        }
        private async Task RunScenario()
        {
            if (SelectedScenario is not null)
               await _dialogService.ShowDialog(nameof(ConfirmationMessageViewModel), Constants.MAINDIALOGIDENTIFIER, OnRunScenraioDialogOpen, OnRunScenarioClose);
        }
        private async Task SaveChanges()
        {
            if (_runScenraioUnitOfWork.IsPending)
            {
                _errorOccured = false;
                _configurationMessage = _configurationMessage.SaveConfiguration(true);
                _autoSave = true;
                await _dialogService.ShowDialog(nameof(ConfirmationMessageViewModel), Constants.MAINDIALOGIDENTIFIER, OnSaveDialogOpen, OnSaveDialogClose);
            }
        }
        #endregion

        #region Private Methods
        private void SetLoadProperties(bool isLoading)
        {
            Radius = isLoading ? 10 : 0;
            IsHitTestVisible = !isLoading;
            WaitingIndicator = isLoading;
        }
        private async Task GetScenarioInfo(int scenarioId)
        {
            _runScenraioUnitOfWork.ScenarioId = scenarioId;
            var parameters = await _runScenraioUnitOfWork.ScenarioParametersRepo.GetAllAsync(_tokenSource.Token);
            ScenarioParameters = new(parameters.Where(sp => !string.IsNullOrEmpty(sp.ParameterName)));
            var budgets = await _runScenraioUnitOfWork.ScenariosBudgetsRepo.GetAllAsync(_tokenSource.Token);
            BudgetConstraints = new(budgets);
        }
        private async void OnErrorOccured(ErrorEventsArgs error)
        {
            _errorOccured = true;
            if (_dialogService.IsDialogOpen(Constants.MAINDIALOGIDENTIFIER))
            {
                _configurationMessage = _configurationMessage.ErrorConfiguration(error.ErrorMessage);
                _messenger.Send(_configurationMessage);
                return;
            } 
            ShowMessage(error.ErrorMessage,error.Level);
        }
        private void CancelCurrentOperation()
        {
            if(!_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = new();
            }
        }
        #endregion

        #region Dialog CallBacks

        #region Edit Scenario Paramerter CallBacks
        private void OnEditScenarioParameterDialogOpen()
        {
            if (_selectedScenarioParameter is null)
                return;
            _selectedScenarioParameter.ScenarioId = SelectedScenario.ScenarioId;
            _messenger.Send(new DataModelMessage<ScenarioParameterModel>(_selectedScenarioParameter));
        }
        private void OnEditScenarioParameterDialogClose(object obj)
        {
            if (obj is ScenarioParameterModel scenarioParameter && scenarioParameter is not null)
            {
                _saveNeededBeforeRun = true;
                RunContent = "Save & Run";
                SaveChangesCommand.NotifyCanExecuteChanged();
                for (int i = 0; i < ScenarioParameters.Count; i++)
                {
                    if (ScenarioParameters[i].ParameterId == scenarioParameter.ParameterId)
                    {
                        ScenarioParameters[i] = scenarioParameter;
                        break;
                    }
                }
            }
            else if(obj is DialogResult dialogResult && dialogResult == DialogResult.Cancel)
            {
                if (_runScenraioUnitOfWork.IsPending)
                    return;
                _saveNeededBeforeRun = false;
                RunContent = "Run";
            } 
        }
        #endregion

        #region Edit Scenario Budget CallBacks
        private void OnEditBudgetDialogOpen()
        {
            _messenger.Send(new DataModelMessage<ScenarioBudgetFlatModel>(_selectedBudget));
        }
        private void OnEditBudgetDialogClose(object obj)
        {
            if (obj is ScenarioBudgetFlatModel budgetModel && budgetModel is not null)
            {
                _saveNeededBeforeRun = true;
                RunContent = "Save & Run";
                SaveChangesCommand.NotifyCanExecuteChanged();
                for (int i = 0; i < BudgetConstraints.Count; i++)
                {
                    ScenarioBudgetFlatModel budget = BudgetConstraints[i];
                    if (ReferenceEquals(budget, _selectedBudget))
                    {
                        BudgetConstraints.RemoveAt(i);
                        BudgetConstraints.Insert(i, budgetModel);
                        break;
                    }
                }
            }
            else if (obj is DialogResult dialogResult && dialogResult == DialogResult.Cancel)
            {
                if (_runScenraioUnitOfWork.IsPending)
                    return;
                _saveNeededBeforeRun = false;
                RunContent = "Run";
            }
        }
        #endregion

        #region Running Scenario CallBacks
        private async void OnRunScenraioDialogOpen()
        {
            if (_saveNeededBeforeRun)
            {
                _configurationMessage = _configurationMessage.SaveConfiguration(true);
                _messenger.Send(_configurationMessage);
                if (_runScenraioUnitOfWork.IsPending)
                    await _runScenraioUnitOfWork.SaveChangesAsync(_tokenSource.Token);
                RunContent = "Run";
            }
            _configurationMessage = _configurationMessage.RunScenarioConfiguration(SelectedScenario.ScenarioName);
            _messenger.Send(_configurationMessage);
            var result = await _runScenraioUnitOfWork.RunScenario(SelectedScenario.ScenarioId, true);
            if(result)
            {
              _configurationMessage = _configurationMessage.SuccessConfiguration($"the scenario: \"{SelectedScenario.ScenarioFullName}\" has been run successfully.");
              _messenger.Send(_configurationMessage);
            }
        }
        private void OnRunScenarioClose(object obj)
        {
            if (obj is DialogResult dialogResult && dialogResult == DialogResult.Cancel)
            {
                CancelCurrentOperation();
            }
        }
        #endregion

        #region Save Changes CallBack
        private async void OnSaveDialogOpen()
        { 
            _messenger.Send(_configurationMessage);
            if (_autoSave)
            {
                if (_runScenraioUnitOfWork.IsPending)
                {
                   await _runScenraioUnitOfWork.SaveChangesAsync(_tokenSource.Token);         
                }
                if (!_errorOccured)
                {
                   _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER, DialogResult.Ok);  
                }
                else await GetScenarioInfo(SelectedScenario.ScenarioId);
                if (_runScenraioUnitOfWork.IsPending)
                    return;
                _saveNeededBeforeRun = false;
                SaveChangesCommand.NotifyCanExecuteChanged();
                RunContent = "Run";
            }
        }
        private void OnSaveDialogClose(object obj)
        {
            _saveNeededBeforeRun = false;
            SaveChangesCommand.NotifyCanExecuteChanged();
            if (((DialogResult)obj) == DialogResult.Cancel)
            {
                CancelCurrentOperation();
                if (_runScenraioUnitOfWork.IsPending)
                {
                   _runScenraioUnitOfWork.ClearPendingOperations();
                    RunContent = "Run";
                    // just to refresh any changes in the local values
                   _isloaded = false;
                }
            }
        }    
        #endregion

        #endregion

        #region Messenger CallBacks
        public async void Receive(DataModelMessage<DialogResult> message)
        {
            if (message.Parameter == DialogResult.Ok)
            {
                _configurationMessage = _configurationMessage.SaveConfiguration(true);
                _messenger.Send(_configurationMessage);
                await _runScenraioUnitOfWork.SaveChangesAsync(_tokenSource.Token);
                _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER, DialogResult.Ok);
            }
        }
        public void Receive(DataModelMessage<ScenarioModel> message)
        {
            _newScenarioModel = message.Parameter;
        }
        #endregion

    }
}
