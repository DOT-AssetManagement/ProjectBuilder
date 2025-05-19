using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class ViewModelBase : ObservableValidator
    {
        [ObservableProperty]
        private string _actionContent;
        [ObservableProperty]
        private string _message;
        [ObservableProperty]
        private string _messageIcon;
        [ObservableProperty]
        private bool _isActive;
        [ObservableProperty]
        private string _messageForeground;
        [RelayCommand]
        protected virtual void Action()
        {
            IsActive = false;
        }
        protected void ShowMessage(string message,LogLevel severity, bool isTimedout = true,string icon = "Information")
        {
            IsActive = true;
            Message = message;
            ActionContent = "";
            switch (severity)
            {
                case LogLevel.Error:
                    MessageIcon = "Error";
                    MessageForeground = Constants.ERRORBRUSH;
                    break;
                case LogLevel.Information:
                    MessageIcon = "Information";
                    MessageForeground = Constants.PAPERBRUSh;
                    break;
                case LogLevel.Critical:
                    MessageIcon = "CreditCardRemove";
                    MessageForeground = Constants.WARNINGFOREGROUNDBRUSH;
                    break;
                case LogLevel.Warning:
                    MessageIcon = "Alert";
                    MessageForeground = Constants.WARNINGFOREGROUNDBRUSH;
                    break;
                    case LogLevel.None:
                    MessageForeground = Constants.PAPERBRUSh;
                    MessageIcon = icon;
                    break;

            }
            if(isTimedout)
               Task.Delay(4000).ContinueWith(task => IsActive = false);
        }
        protected void HideMessage() => IsActive = false;
    }
}
