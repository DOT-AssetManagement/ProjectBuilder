using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class CreateScenarioViewModel : ObservableValidator
    {
        private readonly IRunScenarioUnitOfWork _runScenarioUnit;
        private readonly IDialogService _dialogService;
        private ErrorEventsArgs _error = null;

        [ObservableProperty]
        [Required(AllowEmptyStrings =false,ErrorMessage =Constants.REQUIREDFIELD)]
        private string _scenarioName;
        [ObservableProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        [RegularExpression(Constants.REGEXNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        [ComparsionValidation(nameof(LastYear),ComparesionType.LessThen ,ErrorMessage = "this has to be less than Last Year")]
        private string _firstYear;
        [ObservableProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        [RegularExpression(Constants.REGEXNUMBER, ErrorMessage =Constants.NUMBEREXPECTED)]
        [ComparsionValidation(nameof(FirstYear),ComparesionType.GreaterThen, ErrorMessage = "this has to be greater than First Year")]
        private string _lastYear;
        [ObservableProperty]
        private string _createContent;
        [ObservableProperty]
        private bool _isCreating;
        public CreateScenarioViewModel(IRunScenarioUnitOfWork runScenarioUnit,IDialogService dialogService)
        {
            _runScenarioUnit = runScenarioUnit;
            _dialogService = dialogService;
            CreateContent = "Create";
            IsCreating = false;
        }
        [RelayCommand]
        private void Loaded()
        {
            _runScenarioUnit.ErrorOccured += OnErrorOccurred;
        }

        private void OnErrorOccurred(ErrorEventsArgs error)
        {
            _error = error;
        }

        [RelayCommand]
        private void Unloaded()
        {

            _runScenarioUnit.ErrorOccured -= OnErrorOccurred;
            ScenarioName = "";
            FirstYear = "";
            LastYear = "";
            CreateContent = "Create";
            IsCreating = false;
            ClearErrors();
        }
        [RelayCommand]
        private async Task CreateScenario()
        {
            //TODO- when we create scenario should we check if the scenario name already exists in the data base
            ValidateAllProperties();
            if (HasErrors)
                return;
            CreateContent = "Creating...";
            IsCreating = true;
            var  newScenarioId =  await _runScenarioUnit.CreateScenario(ScenarioName, int.Parse(FirstYear), int.Parse(LastYear));
            object parameter = newScenarioId > -1 ? newScenarioId : _error;
            _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER, parameter);
        }
    }
}
