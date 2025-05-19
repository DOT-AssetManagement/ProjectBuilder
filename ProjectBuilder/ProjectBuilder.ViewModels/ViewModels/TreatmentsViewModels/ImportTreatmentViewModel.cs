using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class DatabaseAuthenticationViewModel : ObservableValidator
    {
        #region Fields
        private readonly IDatabaseService _databaseService;
        private readonly IMessenger _messenger;
        private bool _canClearErrors = true;
        private List<string> _testConncetionDependentPropertiesNames;
        private bool _isRuning;
        #endregion

        #region Properties
        protected string CurrentDatabaseName { get; set; }
        [ObservableProperty]
        [Required(AllowEmptyStrings =false,ErrorMessage ="Server Name is Required")]
        private string _serverName;
        [ObservableProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Database Name is Required")]
        private string _databaseName;
        [ObservableProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = "User Name is Required")]
        private string _userName;
        [ObservableProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is Required")]
        private string _password;       
        [ObservableProperty]
        private bool _indicatorVisibility;
        [ObservableProperty]
        private string _testConnectionContent;
        [ObservableProperty]
        private bool _passwordVisibility;
        [ObservableProperty]
        private string _passwordIcon;
        [ObservableProperty]
        private bool _textVisibility;
        [ObservableProperty]
        private bool _isUserAuthentication;
        
        private int _authenticationType;

        public int AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                if(SetProperty(ref _authenticationType, value)) 
                {
                    if(value == 0)
                    {
                        IsUserAuthentication = false;
                        ClearErrors(nameof(UserName));
                        ClearErrors(nameof(Password));
                    } else
                    {
                        IsUserAuthentication = true;
                    }
                }
            }
        }      
        #endregion

        #region Constructor
        protected DatabaseAuthenticationViewModel(IDatabaseService databaseService,IMessenger messenger)
        {
            _databaseService = databaseService;
            _messenger = messenger;
            _testConncetionDependentPropertiesNames = new(4)
            {
                nameof(ServerName),
                nameof(DatabaseName),
                nameof(UserName),
                nameof(Password)
            };
            Password = "";
            TestConnectionContent = "Test Connection";
            TextVisibility = false;
            PasswordVisibility = true;
            PasswordIcon = "Eye";
            ServerName = databaseService.GetCurrentServerName();
            ClearErrors(nameof(Password));
            ErrorsChanged += OnErrorChanged;
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void ImportDatabase()
        {
           var canImportDatabase = ValidateConnectionDependentProperties();
            if (!canImportDatabase)
                return;
        }
        [RelayCommand]
        private async Task TestConnection()
        {
            bool canTestConnection = ValidateConnectionDependentProperties();
            if (!canTestConnection || _isRuning)
                return;
            _isRuning = true;
            TestConnectionContent = "Connecting...";
            IndicatorVisibility = true;
            var connectionString = BuildConnectionString();
            var result = await _databaseService.TestConnection(connectionString);
            TestConnectionContent = "Test Connection";
            IndicatorVisibility = false;
            _messenger.Send(new DatabaseConnectionMessage(CurrentDatabaseName, result));
            _isRuning = false;
        }
        private string BuildConnectionString()
        {       
                if (IsUserAuthentication)
                    return $"Data Source={ServerName};Initial Catalog={DatabaseName};Persist Security Info=True;User ID={UserName};Password={Password};MultipleActiveResultSets=True";
                return $"Data Source={ServerName};Initial Catalog={DatabaseName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";                  
        }

        [RelayCommand]
        private void ShowPassword()
        {
            if (PasswordVisibility)
            {
                PasswordVisibility = false;
                PasswordIcon = "EyeOff";
                TextVisibility = true;
                return;
            }
            PasswordVisibility = true;
            PasswordIcon = "Eye";
            TextVisibility = false;
        }
        #endregion

        #region Private Methods
        private bool ValidateConnectionDependentProperties()
        {
            _canClearErrors = false;
            bool userAuthenticationValid = false;
            ValidateProperty(ServerName, nameof(ServerName));
            ValidateProperty(DatabaseName, nameof(DatabaseName));
            if(AuthenticationType == 1)
            {
               ValidateProperty(UserName, nameof(UserName));
               ValidateProperty(Password, nameof(Password));
                userAuthenticationValid = string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password);
            }
            _canClearErrors = true;
            return !(string.IsNullOrEmpty(ServerName) || string.IsNullOrEmpty(DatabaseName) ||
                     userAuthenticationValid);
        }
        private void OnErrorChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (_testConncetionDependentPropertiesNames.Contains(e.PropertyName) && _canClearErrors)
                ClearErrors(e.PropertyName);
        }
        #endregion
    }
    public class PAMSDatabaseViewModel : DatabaseAuthenticationViewModel
    {
        public PAMSDatabaseViewModel(IDatabaseService databaseService, IMessenger messenger)
                              : base(databaseService, messenger)
        {
            CurrentDatabaseName = "PAMS";
            DatabaseName = "PAMS";
        }
    }
    public class BAMSDatabaseViewModel : DatabaseAuthenticationViewModel
    {
        public BAMSDatabaseViewModel(IDatabaseService databaseService, IMessenger messenger)
                              : base(databaseService,messenger)
        {
            CurrentDatabaseName = "BAMS";
            DatabaseName = "BAMS";
        }
    }
    public partial class ImportTreatmentViewModel : ObservableObject,IRecipient<DatabaseConnectionMessage>
    {
        [ObservableProperty]
        private bool _isActive;
        [ObservableProperty]
        private string _messageIcon;
        [ObservableProperty]
        private string _message;

        public ImportTreatmentViewModel(IMessenger messenger)
        {
            messenger.Register(this);
        }

        public void Receive(DatabaseConnectionMessage message)
        {
            if (message.HasConnected)
            {
                Message = $"Connection to the {message.DatabaseName} Database has been successfully established";
                MessageIcon = "DatabaseCheck";
            }
            else
            {
                Message = $"Connection to the {message.DatabaseName} Database has failed";
                MessageIcon = "DatabaseRemove";
            }
            IsActive = true;
            Task.Delay(5000).ContinueWith( t => IsActive = false);
        }
    }
    
}
