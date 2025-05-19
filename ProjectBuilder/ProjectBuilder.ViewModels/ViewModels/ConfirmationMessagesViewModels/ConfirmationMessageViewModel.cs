using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;

namespace ProjectBuilder.ViewModels
{
    public partial class ConfirmationMessageViewModel : ObservableObject,IRecipient<ConfigurationMessage>
    {
        private readonly ISettingsService _settingsService;
        private readonly IMessenger _messenger;
        private string _senderToken;
        private bool _isSaving = false;
        //
        [ObservableProperty]
        private string _icon;
        [ObservableProperty]
        private string _title;
        [ObservableProperty]
        private string _message;
        [ObservableProperty]
        private string _foreground;
        [ObservableProperty]
        private double _value;
        [ObservableProperty]
        private bool _hasNeverShowMessage;
        [ObservableProperty]
        private bool _canCancel;
        [ObservableProperty]
        private string _saveContent;
        [ObservableProperty]
        private bool _hasSave;
        [ObservableProperty]
        private string _cancelContent;

        private bool _showMessage;
        public bool ShowMessage
        {
            get { return _showMessage; }
            set 
            {
                if (SetProperty(ref _showMessage, value))
                    _settingsService?.UpdateSetting(SettingName.ShowDeleteConfirmationMessage, !value);
            }
        }
        public ConfirmationMessageViewModel(ISettingsService settingsSerice,IMessenger messenger)
        {
            _settingsService = settingsSerice;
            Icon = "Delete";
            Title = "Delete Record";
            Foreground = "ErrorDarkBrush";
            CancelContent = "Cancel";
            SaveContent = "Delete";
            _messenger = messenger;
            messenger.Register(this);
        }

        public void Receive(ConfigurationMessage message)
        {
            Icon = message.Icon;
            Title = message.Title;
            Message = message.Message;
            Value = message.IsWaiting ? 0 : 100;
            HasNeverShowMessage = false;
            CanCancel = message.HasCancelButton;
            HasSave = message.HasSaveButton;
            Foreground = message.Foreground;
            SaveContent = message.ButtonContent;
            CancelContent = message.CancelContent;
            HasNeverShowMessage = message.HasNeverShowMessage;
            _senderToken = message.SenderToken;
        }   
        [RelayCommand]
        private void OK()
        {
            if (_isSaving)
                return;        
            _isSaving = true;
            Value = 0;
            if(string.IsNullOrEmpty(_senderToken))
            _messenger.Send(new DataModelMessage<DialogResult>(DialogResult.Ok));
            else _messenger.Send(new DataModelMessage<DialogResult>(DialogResult.Ok),_senderToken);
        }
        [RelayCommand]
        private void Unloaded()
        {
            _isSaving = false;
            Value = 100;
        }
    }
}
