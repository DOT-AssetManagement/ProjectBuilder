using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        #region Fields
        private readonly IThemeManagerService _themeManager;
        private readonly ISettingsService _settingsService;
        bool _isLoaded = false;
        #endregion

        #region Properties
        [ObservableProperty]
        private int _currentTheme;

        private bool _showConfirmationMessage;
        public bool ShowConfirmationMessage
        {
            get { return _showConfirmationMessage; }
            set
            {
                if (SetProperty(ref _showConfirmationMessage, value))
                    _settingsService.UpdateSetting(SettingName.ShowDeleteConfirmationMessage, value);
            }
        }
        #endregion

        #region Constructor
        public SettingsViewModel(IThemeManagerService themeManager,ISettingsService settingsService)
        {
            _themeManager = themeManager;
            _settingsService = settingsService;               
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void ApplyTheme()
        {
            _themeManager.ChangeMainTheme((CurrentTheme)CurrentTheme);
            _settingsService.UpdateSetting(SettingName.CurrentTheme, (CurrentTheme)CurrentTheme);
        }
        [RelayCommand]
        private void Loaded()
        {
            ShowConfirmationMessage = _settingsService.GetSettingValue<bool>(SettingName.ShowDeleteConfirmationMessage);
            if (_isLoaded)
                return;
            CurrentTheme = _settingsService.GetSettingValue<int>(SettingName.CurrentTheme);
            _isLoaded = true;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
