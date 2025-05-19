using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class TreatmentBaseViewModel : ViewModelBase, IRecipient<DataModelMessage<DialogResult>>, IRecipient<DataModelMessage<TreatmentModel>>
    {
        #region Fields
        private readonly IDialogService _dialogService;
        private bool _autoSaveContent = true;
        private CancellationTokenSource _tokenSource;
        private bool _tokenDisposed;
        private ConfigurationMessage _configurationMessage;
        private List<DefaultSlackModel> _defaultSlackModels;
        #endregion

        #region Properties

        #region Observable Properties
        [ObservableProperty]
        private int _selectedPriorityIndex;
        [ObservableProperty]
        private List<int> _preferredYears;
        [ObservableProperty, NotifyDataErrorInfo, ComparsionValidation(nameof(SelectedPreferredYear), ComparesionType.LessOrEqaul, ErrorMessage = "this should be less than Preferred Year")]
        [RegularExpression(Constants.REGEXNUMBER, ErrorMessage = Constants.NUMBEREXPECTED), Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        private string _minYear;
        [ObservableProperty, NotifyDataErrorInfo, ComparsionValidation(nameof(SelectedPreferredYear), ComparesionType.GreateOrEqual, ErrorMessage = "this should be greater than Preferred Year")]
        [RegularExpression(Constants.REGEXNUMBER, ErrorMessage = Constants.NUMBEREXPECTED), Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        private string _maxYear;
        [ObservableProperty]
        [NotifyDataErrorInfo, Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        private string _treatmentName;
        [ObservableProperty]
        [NotifyDataErrorInfo, Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD), RegularExpression(Constants.REGEXFLOATNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        private string _cost;
        [ObservableProperty]
        [NotifyDataErrorInfo, Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD), RegularExpression(Constants.REGEXFLOATNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        private string _benefit;
        [ObservableProperty]
        [NotifyDataErrorInfo, Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD), RegularExpression(Constants.REGEXFLOATNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        private string _risk;
        [ObservableProperty]
        [NotifyDataErrorInfo, Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD)]
        private string _bRKey;
        [ObservableProperty]
        [NotifyDataErrorInfo, Required(AllowEmptyStrings = false, ErrorMessage = Constants.REQUIREDFIELD), RegularExpression(Constants.REGEXNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        private string _bridgeId;
        [ObservableProperty]
        private bool _isMaxMinYearEnabled;
        [ObservableProperty]
        private bool _isBRIdVisible;
        [ObservableProperty]
        private bool _interstate;
        [ObservableProperty]
        private bool _direction;
        [ObservableProperty]
        private string _backToContent;
        #endregion 

        private int? _selectedPreferredYear;
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select a Preferred Year")]
        public int? SelectedPreferredYear
        {
            get { return _selectedPreferredYear; }
            set
            {
                if (SetProperty(ref _selectedPreferredYear, value) && IsLoaded)
                {
                    if ((SelectedPriorityIndex == 0 || SelectedPriorityIndex == 4) && Committed.HasValue)
                        SetMinMaxYear(Committed.Value);
                }
            }
        }
        private bool? _committed;
        public bool? Committed
        {
            get { return _committed; }
            set
            {
                if (SetProperty(ref _committed, value) && value is not null)
                {
                    SelectedPriorityIndex = value.Value ? 0 : 4;
                    SetMinMaxYear(value.Value);
                }
            }
        }
        protected IMessenger Messenger { get; }
        public ITreatmentRepository TreatmentRepo { get; }
        protected string CurrentViewModelToken { get; set; }
        protected CancellationToken CurrentCancellationToken
        {
            get
            {
                InitializeTokenSource();
                return _tokenSource.Token;
            }
        }
        public bool HasPropertiesChanged { get; protected set; }
        public bool IsLoaded { get; protected set; }
        protected CurrentView NavigateFromView { get; set; } = CurrentView.None;
        public string CurrentAssetType { get; protected set; } = "B";
        #endregion

        #region Commands
        public IAsyncRelayCommand SaveChangesCommand { get; }
        #endregion

        #region Constructor
        public TreatmentBaseViewModel(ITreatmentRepository treatmentRepo, IMessenger messenger, IDialogService dialogService)
        {
            TreatmentRepo = treatmentRepo;
            _dialogService = dialogService;
            Messenger = messenger;
            messenger.Register<DataModelMessage<TreatmentModel>>(this);
            IsLoaded = false;
            SaveChangesCommand = new AsyncRelayCommand(SaveChanges, () => HasPropertiesChanged);
            PreferredYears = new List<int>(Enumerable.Range(2010, 41));
            _configurationMessage = new(CurrentViewModelToken);
            SelectedPreferredYear = null;
            Committed = true;
            CurrentAssetType = "B";
            CurrentViewModelToken = nameof(TreatmentBaseViewModel);
            BackToContent = "Back To Treatments";
            IsBRIdVisible = true;
        }
        #endregion

        #region Dialog Callbacks
        private void OnCloseSaveDialog(object obj)
        {
            if (obj is DialogResult result && result == DialogResult.Cancel)
            {
                CancelPendingOperation();
                TreatmentRepo.ClearPendingChanges();
            }
        }
        private async void OnOpenSaveDialog()
        {
            _configurationMessage = _configurationMessage.SaveConfiguration(_autoSaveContent);
            Messenger.Send(_configurationMessage);
            if (_autoSaveContent)
            {
                await SaveTreatment();
                _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER);
            }
        }
        #endregion

        #region Private Methods
        private void InitializeTokenSource()
        {
            if (_tokenSource is null || _tokenSource.IsCancellationRequested || _tokenDisposed)
            {
                _tokenSource = new();
                _tokenDisposed = false;
            }
        }
        private void CancelPendingOperation()
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenDisposed = true;
                InitializeTokenSource();
            }
        }
        protected override void Action()
        {
            CancelPendingOperation();
        }
        private async Task SaveChanges()
        {
            ValidateProperties();
            if (HasErrors)
                return;
            _autoSaveContent = true;
            await _dialogService.ShowDialog(nameof(ConfirmationMessageViewModel), Constants.MAINDIALOGIDENTIFIER, OnOpenSaveDialog, OnCloseSaveDialog);
        }
        [RelayCommand]
        protected virtual async Task Loaded()
        {
            TreatmentRepo.ErrorOccured += OnErrorOccured;
            if (!IsLoaded)
            {
                InitializeTokenSource();
                ActionContent = "Cancel";
                ShowMessage("Loading Data please wait", LogLevel.None, false, "TimerSand");
                _defaultSlackModels = new();
                _defaultSlackModels.Add(await TreatmentRepo.GetAssetTypeDefaultSlack("B"));
                _defaultSlackModels.Add(await TreatmentRepo.GetAssetTypeDefaultSlack("P"));
            }
            if (!Messenger.IsRegistered<DataModelMessage<DialogResult>, string>(this, CurrentViewModelToken))
                Messenger.Register<DataModelMessage<DialogResult>, string>(this, CurrentViewModelToken);
        }
        [RelayCommand]
        protected virtual async Task Unloaded()
        {
            if (HasPropertiesChanged)
            {
                _autoSaveContent = false;
                await _dialogService.ShowDialog(nameof(ConfirmationMessageViewModel), Constants.MAINDIALOGIDENTIFIER, OnOpenSaveDialog, OnCloseSaveDialog);
            }
            Messenger.Unregister<DataModelMessage<DialogResult>, string>(this, CurrentViewModelToken);
            TreatmentRepo.ErrorOccured -= OnErrorOccured;
            _tokenSource.Dispose();
            _tokenDisposed = true;
            _tokenSource = null;
        }
        [RelayCommand]
        private void BackToTreatments()
        {
            Messenger.Send(new ChangeCurrentViewMessage(NavigateFromView));
        }

        private void OnErrorOccured(ErrorEventsArgs error)
        {
            ShowMessage(error.ErrorMessage, error.Level);
        }
        public async void Receive(DataModelMessage<DialogResult> message)
        {
            if (message.Parameter == DialogResult.Ok)
            {
                await SaveTreatment();
                _dialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER, DialogResult.Ok);
            }
        }
        protected void SetMinMaxYear(bool isCommitted)
        {
            IsMaxMinYearEnabled= !isCommitted;
            if (SelectedPreferredYear is null)
            {
                ClearMinMaxYearValues();
                return;
            }
            if (isCommitted)
            {
                MinYear = SelectedPreferredYear.ToString();
                MaxYear = SelectedPreferredYear.ToString();
                return;
            }
            var defaultSlack = _defaultSlackModels.FirstOrDefault(s => s.AssetType == CurrentAssetType);
            if (defaultSlack is null)
                return;
            MinYear = $"{SelectedPreferredYear - defaultSlack.MoveBefore}";
            MaxYear = $"{SelectedPreferredYear + defaultSlack.MoveAfter}";
        }
        private void ClearMinMaxYearValues()
        {
            MaxYear = "";
            MinYear = "";
            ClearErrors(nameof(MaxYear));
            ClearErrors(nameof(MinYear));
        }
        #endregion

        #region Virtual Methods
        protected virtual Task SaveTreatment()
        {
            Messenger.Send(new RefreshNeededMessage(true));
            return Task.CompletedTask;
        }
        protected virtual void ValidateProperties()
        {
            ValidateAllProperties();
        }
        protected virtual void OnTreatementReceived(TreatmentModel treatment) { }
        public void Receive(DataModelMessage<TreatmentModel> message)
        {
            NavigateFromView = message.SenderView;
            if (NavigateFromView == CurrentView.ProjectDetailsView)
                BackToContent = "Back To Edit Project";
            else
                BackToContent = "Back To Treatments";
            OnTreatementReceived(message.Parameter);
        }
        #endregion
    }
}
