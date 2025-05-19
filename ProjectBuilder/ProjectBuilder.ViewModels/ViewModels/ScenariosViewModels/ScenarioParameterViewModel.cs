using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class EditScenarioParameterViewModel : ObservableValidator, IRecipient<DataModelMessage<ScenarioParameterModel>>
    {
        private readonly IRepository<ScenarioParameterModel> _scenarioParameterRepo;
        private readonly IDialogService _dialogService;
        private ScenarioParameterModel _currentParameter;
        private CancellationTokenSource _tokenSource;
        [ObservableProperty]
        private string _parameterName;
        [ObservableProperty,NotifyDataErrorInfo]
        [RegularExpression(Constants.REGEXNUMBER, ErrorMessage=Constants.NUMBEREXPECTED),Required(AllowEmptyStrings =false,ErrorMessage =Constants.REQUIREDFIELD),ComparsionValidation(1,ErrorMessage ="this needs to be greater then 1")]
        [NotifyCanExecuteChangedFor(nameof(UpdateParameterValueCommand))]
        private string _parameterValue;
        [ObservableProperty]
        private bool _isUpdating;
        [ObservableProperty]
        private string _content;
        [ObservableProperty]
        private bool _isEditParameterVisible;
        [ObservableProperty]
        private bool _isEditCheckBoxVisible;
        [ObservableProperty]
        private bool _booleanParameterValue;
        public IAsyncRelayCommand UpdateParameterValueCommand { get;}
        public EditScenarioParameterViewModel(IMessenger messsenger,IRepository<ScenarioParameterModel> scenarioParameterRepo, IDialogService dialogService)
        {
            _scenarioParameterRepo = scenarioParameterRepo;
            _dialogService = dialogService;
            messsenger.Register(this);
            UpdateParameterValueCommand = new AsyncRelayCommand(UpdateParameterValue, () => !HasErrors);
            Content = "Update";
            IsEditCheckBoxVisible= false;
            IsEditParameterVisible= true;
        }

        private async Task UpdateParameterValue()
        {
            Content = "Updating...";
            IsUpdating = true;
            var newValue = new ScenarioParameterModel
            {
                ScenarioId = _currentParameter.ScenarioId,
                ParameterName = _currentParameter.ParameterName,
                ParameterDescription = _currentParameter.ParameterDescription,
                ParameterId = _currentParameter.ParameterId
            };
            if (_currentParameter.ParameterValue <= 1)
                newValue.ParameterValue = Convert.ToDouble(BooleanParameterValue);
            else newValue.ParameterValue = double.Parse(ParameterValue);
            await _scenarioParameterRepo.UpdateAsync(newValue, nameof(newValue.ParameterValue));
            IsUpdating = false;
            ParameterName = "";
            ParameterValue = "";
            Content = "Update";
            _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER,newValue);
        }
        [RelayCommand]
        private void Loaded()
        {
            _tokenSource = new();
        }
        [RelayCommand]
        private void Unloaded()
        {
            ParameterValue = "";
            ParameterName = "";
            _tokenSource.Dispose();
        }
        [RelayCommand]
        private void CloseDialog()
        {
            if(!_tokenSource.IsCancellationRequested && IsUpdating)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();             
            }
            _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER,DialogResult.Cancel);
        }
        public void Receive(DataModelMessage<ScenarioParameterModel> message)
        {
            _currentParameter = message.Parameter;
            ParameterName = _currentParameter.ParameterName;
            IsEditParameterVisible = !(_currentParameter.ParameterValue <= 1);
            IsEditCheckBoxVisible= _currentParameter.ParameterValue <= 1;
            BooleanParameterValue= _currentParameter.ParameterValue == 1;
            ParameterValue = _currentParameter.ParameterValue.ToString();
        }
    }
}
