using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.ComponentModel;

namespace ProjectBuilder.ViewModels
{
    public partial class ShellViewModel : ObservableObject,IRecipient<ChangeCurrentViewMessage>
    {
        public event Action<CurrentView> NavigationHasChanged;

        #region Properties
        [ObservableProperty]
        private CurrentView _currentView;
        [ObservableProperty]
        private CurrentView _navigatedToView;
        [ObservableProperty]
        private bool _canShutDownApplication;

        private bool _isShuttingDown;           
        public bool IsShuttingDown
        {
            get { return _isShuttingDown; }
            set 
            { 
                if(SetProperty(ref _isShuttingDown, value))
                {
                   // CanShutDownApplication = false;
                }
            }
        }

        #endregion

        #region Constructor
        public ShellViewModel(IMessenger messenger)
        {
            messenger.Register(this);
            CanShutDownApplication = true;
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void ViewChanged(CurrentView view)
        {
            CurrentView = view;
        }
        [RelayCommand]
        private void Closing(CancelEventArgs cancel)
        {
          //  cancel.Cancel = true;
        }
        #endregion

        public void Receive(ChangeCurrentViewMessage message)
        {
            if(NavigatedToView == message.CurrentView)
               NavigationHasChanged?.Invoke(NavigatedToView);
            NavigatedToView = message.CurrentView;
            CurrentView = NavigatedToView;
        }
    }
}
