using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class EditBudgetViewModel : ObservableValidator,IRecipient<DataModelMessage<ScenarioBudgetFlatModel>>
    {
        private readonly IRepository<ScenarioBudgetFlatModel> _budgetRepo;
        private readonly IDialogService _dialogService;
        private ScenarioBudgetFlatModel _currentScenarioBudget;
        bool _isloaded = false;
        bool _hasPropertiesChanged = false;
        bool _settingProperties = false;
        Dictionary<string,decimal?> _modifiedProperties;
        [ObservableProperty]
        private int _district;
        [ObservableProperty]
        private int _year;
        [ObservableProperty, NotifyDataErrorInfo]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        [RegularExpression(Constants.REGEXFLOATNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        private string _bridgeInterstateBudget;
        [ObservableProperty, NotifyDataErrorInfo]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        [RegularExpression(Constants.REGEXFLOATNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        private string _bridgeNonInterstateBudget;
        [ObservableProperty, NotifyDataErrorInfo]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        [RegularExpression(Constants.REGEXFLOATNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        private string _pavementInterstateBudget;
        [ObservableProperty,NotifyDataErrorInfo]
        [Required(AllowEmptyStrings =false,ErrorMessage =Constants.REQUIREDFIELD)]
        [RegularExpression(Constants.REGEXFLOATNUMBER,ErrorMessage =Constants.NUMBEREXPECTED)]
        private string _pavementNonInterstateBudget;
        public IAsyncRelayCommand UpdateBudgetCommand { get;}
        public EditBudgetViewModel(IMessenger messenger,IRepository<ScenarioBudgetFlatModel> budgetRepo,
                                   IDialogService dialogService)
        {
            messenger.Register(this);
            _modifiedProperties = new(4)
            {
                { nameof(BridgeInterstateBudget), null},
                { nameof(BridgeNonInterstateBudget), null},
                { nameof(PavementInterstateBudget), null},
                { nameof(PavementNonInterstateBudget), null},
            };
            _budgetRepo = budgetRepo;
            _dialogService = dialogService;
            UpdateBudgetCommand = new AsyncRelayCommand(UpdateBudget,() => _hasPropertiesChanged);
        }
        [RelayCommand]
        private void Loaded()
        {
            //if (!_isloaded)
            //{
            //  _isloaded = true;
            //}
            _settingProperties = false;
        }
        [RelayCommand]
        private void Unloaded()
        {
            _hasPropertiesChanged = false;
            UpdateBudgetCommand.NotifyCanExecuteChanged();
        }   
        private async Task UpdateBudget()
        {
            ValidateAllProperties();
            if (HasErrors)
                return;
            //TODO- update only the modified properties
            _currentScenarioBudget.BridgeInterstateBudget = decimal.Parse(BridgeInterstateBudget);
            _currentScenarioBudget.BridgeNonInterstateBudget = decimal.Parse(BridgeNonInterstateBudget);
            _currentScenarioBudget.PavementInterstateBudget = decimal.Parse(PavementInterstateBudget);
            _currentScenarioBudget.PavementNonInterstateBudget = decimal.Parse(PavementNonInterstateBudget);
            var type = GetType();
            foreach (var property in _modifiedProperties.Keys)
            {
                if(!decimal.TryParse(type.GetProperty(property).GetValue(this).ToString(), out decimal currentValue))
                    continue;
                if (currentValue != _modifiedProperties[property])
                {
                    await _budgetRepo.UpdateAsync(_currentScenarioBudget, property);
                    _modifiedProperties[property] = currentValue;
                }
            }
            _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER, _currentScenarioBudget);
        }
        public void Receive(DataModelMessage<ScenarioBudgetFlatModel> message)
        {
            _settingProperties = true;
            _currentScenarioBudget = message.Parameter;
            if (_currentScenarioBudget is null)
                return;
            District = _currentScenarioBudget.District;
            Year = _currentScenarioBudget.YearWork;
            PopulateBudgetValues();
            BridgeInterstateBudget = _currentScenarioBudget.BridgeInterstateBudget.ToString();
            BridgeNonInterstateBudget = _currentScenarioBudget.BridgeNonInterstateBudget.ToString();
            PavementInterstateBudget = _currentScenarioBudget.PavementInterstateBudget.ToString();
            PavementNonInterstateBudget = _currentScenarioBudget.PavementNonInterstateBudget.ToString();
            UpdateBudgetCommand?.NotifyCanExecuteChanged();
            _settingProperties = false;
        }
        private void PopulateBudgetValues()
        {
            var budgetType = _currentScenarioBudget.GetType();
            foreach (var property in _modifiedProperties.Keys)
                _modifiedProperties[property] = budgetType.GetProperty(property).GetValue(_currentScenarioBudget) as decimal?;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if(!_settingProperties)
            {
                _hasPropertiesChanged = true;
                UpdateBudgetCommand?.NotifyCanExecuteChanged();
            }
            base.OnPropertyChanged(e);      
        }
    }
}
