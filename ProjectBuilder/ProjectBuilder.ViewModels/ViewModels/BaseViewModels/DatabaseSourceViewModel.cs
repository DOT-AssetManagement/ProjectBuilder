using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using System;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics.Metrics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using ProjectBuilder.Core;

namespace ProjectBuilder.ViewModels
{
    public class DatabaseSourceViewModel<TSource> : DataSourceHelper, IRecipient<DataModelMessage<DialogResult>>  where TSource : class
    {
        #region Fields
        private bool _isloaded = false;
        private bool _hasValueChangedFromCode = false;
        private PaginationHelper<TSource> _paginationHelper;
        private readonly ISettingsService _settingsService;
        private CancellationTokenSource _tokenSource;
        private ConfigurationMessage _configurationMessage;
        bool _tokenDisposed =false;
        int _parsedItemsPerPage = 10;    
        #endregion

        #region Properties
        public IMessenger Messenger { get;}
        public IDialogService DialogService { get;}
        public IRepository<TSource> CurrentRepository { get;}
        protected CancellationToken CurrentToken 
        {
            get
            {
                InitializeTokenSource();
                return _tokenSource.Token;
            }
        }
        public long ElementCount { get; protected set; }
        public bool RefreshNeeded { get; protected set; } = false;
        public bool IsLoaded { get { return _isloaded; } }

        #region Data Source Properties
        private ObservableCollection<TSource> _dataSource;
        public ObservableCollection<TSource> DataSource
        {
            get { return _dataSource; }
            set
            {
                SetProperty(ref _dataSource, value);
            }
        }
        public TSource SelectedRow { get; private set; }
        public string CurrentModelToken { get; set;}
        #endregion

        #region Pagination Properties

        private string _itemsPerPage;
        [RegularExpression(Constants.REGEXNUMBER, ErrorMessage = Constants.NUMBEREXPECTED)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Required")]
        [ComparsionValidation(200,10,ComparesionType.GreaterOrEqualLessOrEqual, ErrorMessage = "Greater or equal to 10")]
        public string ItemsPerPage
        {
            get { return _itemsPerPage; }
            set
            {
                if (SetProperty(ref _itemsPerPage, value, true) && _isloaded)
                {
                    if (GetErrors(nameof(ItemsPerPage)).Count() == 0 && int.TryParse(value, out _parsedItemsPerPage) && !_hasValueChangedFromCode)
                    {
                        InitializePaginationHelper(true);
                        LoadSource();
                    }
                }
            }
        }

        private int _currentPage;
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (SetProperty(ref _currentPage, value) && !_hasValueChangedFromCode)
                {
                    if (!_isloaded)
                        return;
                    LoadSource();                   
                }
            }
        }
        #endregion

        #endregion

        #region Commands  
        public IAsyncRelayCommand LoadedCommand { get; }
        public IAsyncRelayCommand UnloadedCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand DeleteRowCommand { get; }
        public IRelayCommand SelectedRowChangedCommand { get; }
        public IRelayCommand EditRowCommand { get; }
        public IRelayCommand CancelLoadingCommand { get; }
        public IRelayCommand GoToPageCommand { get; }
        #endregion

        #region Constructor
        public DatabaseSourceViewModel(IRepository<TSource> repository, IDialogService dialogService,
                                       ISettingsService settingsService,IMessenger messenger)
        {
            CurrentRepository = repository;
            DialogService = dialogService;
            _settingsService = settingsService;
            Messenger = messenger;
            _configurationMessage = new();
            CurrentModelToken = GetType().Name; 
            LoadedCommand = new AsyncRelayCommand(Loaded);
            UnloadedCommand = new AsyncRelayCommand(Unloaded);
            RefreshCommand = new AsyncRelayCommand(Refresh);
            DeleteRowCommand = new AsyncRelayCommand(DeleteDataRow);
            EditRowCommand = new RelayCommand(EditRow);
            GoToPageCommand = new RelayCommand(GoTo);
            SelectedRowChangedCommand = new RelayCommand<IList>(SelectedRowChanged);
            CancelLoadingCommand = new RelayCommand(CancelLoading);
            CurrentPage = 0;
            DataSource = new();
            IsPaginationEnabled = true;
            DataGridRaduis = 0;
            IsRefreshEnabled = true;
            IsItemsPerPageVisible = true;
            ItemsPerPage = "10";
        }

        private void GoTo()
        {
            ValidateProperty(GoToPage,nameof(GoToPage));
            if(GetErrors(nameof(GoToPage)).Count() == 0)
            {
               var success = int.TryParse(GoToPage, out int pageIndex);
                if(success && pageIndex != CurrentPage)
                   CurrentPage = pageIndex;
            }
        }

        private void CancelLoading()
        {
            if (_tokenSource is not null && !_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = new();
            }
            SetLoadingProperties(false);
        }
        private void SelectedRowChanged(IList items)
        {
            if (items?.Count > 0)
                SelectedRow = (TSource)items[0];
        }

        private async Task DeleteDataRow()
        {
            bool withConfirmation = _settingsService.GetSettingValue<bool>(SettingName.ShowDeleteConfirmationMessage);
            if (withConfirmation)
                await DialogService.ShowDialog(CurrentView.ConfirmationMessageView, Constants.MAINDIALOGIDENTIFIER, OnDeleteDialogOpened, null);
            else
            {
                ShowMessage("Deleting the selected record please wait.", LogLevel.None, false, "TimeSand");
                await DeleteRowInternal();
                if (CurrentRepository.IsPending)
                    await CurrentRepository.SaveChangesAsync(CurrentToken);
                ShowMessage("the selected record has been deleted successfully.", LogLevel.None, true, "Delete");
            }
        }

        private void OnDeleteDialogOpened()
        {
            _configurationMessage = _configurationMessage.DeleteConfiguration();
          //  _configurationMessage.SenderToken = CurrentModelToken;
            Messenger.Send(_configurationMessage);
        }
        private async Task DeleteRowInternal()
        {
            DataSource.Remove(SelectedRow);
            await DeleteRow(SelectedRow);
        }
        #endregion

        #region Private Methods
        protected virtual async Task Unloaded()
        {
            CurrentRepository.ErrorOccured -= OnErrorOccured;
            if (CurrentRepository.IsPending)
                await DialogService.ShowDialog(nameof(ConfirmationMessageViewModel), Constants.MAINDIALOGIDENTIFIER, OnSaveDialogOpened, OnSaveClose);
            Messenger.Unregister<DataModelMessage<DialogResult>, string>(this, CurrentModelToken);
            _tokenSource?.Dispose();
            _tokenSource= null;
            _tokenDisposed= true;  
        }

        private void OnSaveClose(object obj)
        {
            if(((DialogResult)obj) == DialogResult.Cancel)
                CancelLoading();
        }
        private async void OnSaveDialogOpened()
        {
            _configurationMessage = _configurationMessage.SaveConfiguration(true);
          //  _configurationMessage.SenderToken = CurrentModelToken;
            Messenger.Send(_configurationMessage);
            await CurrentRepository.SaveChangesAsync(CurrentToken);
            DialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER, DialogResult.Ok);
        }
        protected void SetLoadingProperties(bool isOn)
        {
            IsPaginationEnabled = !isOn;
            IsRefreshEnabled = !isOn;
            WaitingIndicator = isOn;
            IsDataGridHitTestVisible = !isOn;
            DataGridRaduis = isOn ? 10 : 0;
        }

        #region Loading Data
        protected virtual async Task Loaded()
        {
            CurrentRepository.ErrorOccured += OnErrorOccured;
            _configurationMessage.SenderToken = CurrentModelToken;
            if (!Messenger.IsRegistered<DataModelMessage<DialogResult>, string>(this, CurrentModelToken))
                Messenger.Register(this, CurrentModelToken);
            if (_isloaded && !RefreshNeeded)
                return;
            SetLoadingProperties(true);
            ElementCount = await CurrentRepository.GetCountAsync(CurrentToken);
            InitializePaginationHelper();
            await LoadSource();
            _isloaded = true;
            RefreshNeeded = false;
        }
        protected virtual async Task Refresh()
        {
            SetLoadingProperties(true);
            if (CurrentRepository.IsPending)
                await CurrentRepository.SaveChangesAsync(CurrentToken);
            ElementCount = await CurrentRepository.GetCountAsync();
            InitializePaginationHelper();
            await LoadSource();
        }
        protected void InitializeTokenSource()
        {
            if (_tokenSource is null || _tokenSource.IsCancellationRequested || _tokenDisposed)
            {
                _tokenSource = new();
                _tokenDisposed= false;
            }
        }
        protected void OnErrorOccured(ErrorEventsArgs error)
        {
            SetLoadingProperties(false);
            ShowMessage(error.ErrorMessage, error.Level);
        }
        protected async Task LoadSource()
        {                  
            SetLoadingProperties(true);
            var source = await _paginationHelper?.CreatePageAsync(CurrentPage -1,CurrentToken);
            DataSource = new(source);
            SetLoadingProperties(false);
            IsPaginationEnabled = DataSource?.Count > 0;
        }
        protected void InitializePaginationHelper(bool hasUserChangedItemsPerPage = false)
        {
            if (_paginationHelper is null)
            {
                _paginationHelper = new(ElementCount, CurrentRepository.GetRangeAsync);
                _paginationHelper.CalculteItemsPerPage();
                _hasValueChangedFromCode = true;
                _parsedItemsPerPage = _paginationHelper.ItemsPerPage;
                ItemsPerPage = _paginationHelper.ItemsPerPage.ToString();
                _hasValueChangedFromCode = false;
            }
            else
            {
                _paginationHelper.SourceCount = ElementCount;
                if (!hasUserChangedItemsPerPage)
                {
                    _paginationHelper.CalculteItemsPerPage();
                    _hasValueChangedFromCode = true;
                    ItemsPerPage = _paginationHelper.ItemsPerPage.ToString();
                    _hasValueChangedFromCode = false;
                }
                else _paginationHelper.ItemsPerPage = _parsedItemsPerPage;
            }
            SetPagesCountCurrentPage();
        }
        protected void SetPagesCountCurrentPage()
        {
            _hasValueChangedFromCode = true;
            PagesCount = _paginationHelper.PagesCount;
            IsItemsPerPageVisible = ElementCount > 10;
            GoToPageHint = $"1-{PagesCount}";
            CurrentPage = _paginationHelper.PagesCount < CurrentPage || CurrentPage == -1 ? _paginationHelper.PagesCount - 1 : CurrentPage;
            _hasValueChangedFromCode = false;
        }
        #endregion

        #endregion

        #region Virtual Methods
        protected virtual async Task DeleteRow(TSource row) 
        {
            CurrentRepository.Delete(row);
            await Task.CompletedTask;
        }       
        protected virtual void EditRow() { }
        #endregion

        public async void Receive(DataModelMessage<DialogResult> message)
        {
            if (message.Parameter == DialogResult.Ok)
            {
                await DeleteRowInternal();
                if(CurrentRepository.IsPending)
                await CurrentRepository.SaveChangesAsync(CurrentToken);
                DialogService.CloseDialog(Constants.MAINDIALOGIDENTIFIER, DialogResult.Ok);     
            }
        }
    }
    public partial class DataSourceHelper : ViewModelBase
    {
        #region Properties
        [ObservableProperty]
        private bool _waitingIndicator;
        [ObservableProperty]
        private bool _isPaginationEnabled;
        [ObservableProperty]
        private int _dataGridRaduis;
        [ObservableProperty]
        private bool _isDataGridHitTestVisible;
        [ObservableProperty]
        private bool _isRefreshEnabled;
        [ObservableProperty]
        private int _pagesCount;
        [ObservableProperty]
        [RegularExpression(Constants.REGEXNUMBER,ErrorMessage ="N°:Expected")]
        [ComparsionValidation(1,nameof(PagesCount),ComparesionType.GreaterOrEqualLessOrEqual)]
        private string _goToPage;
        [ObservableProperty]
        private string _goToPageHint;
        [ObservableProperty]
        private bool _isItemsPerPageVisible;
        #endregion
    }
}
