
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;

namespace ProjectBuilder.ViewModels
{
    public partial class ScenariosViewModel : DatabaseSourceViewModel<ScenarioModel>
    {
        #region Fields
        private readonly IRunScenarioUnitOfWork _runScenarioUnitOfWork;
        private readonly IViewResolverService _viewResolver;
        #endregion

        #region Constructor
        public ScenariosViewModel(IRunScenarioUnitOfWork runScenarioUnitOfWork, IDialogService dialogService,ISettingsService settingsService,
                                  IMessenger messenger,IViewResolverService viewResolver)
                                 : base(runScenarioUnitOfWork.ScenariosRepo,dialogService,settingsService,messenger)
        {
            _runScenarioUnitOfWork = runScenarioUnitOfWork;
            _viewResolver = viewResolver;
            CurrentModelToken = nameof(ScenariosViewModel);
        }

        #endregion
        [RelayCommand]
        private async Task CreateScenario()
        {
            await DialogService.ShowDialog(nameof(CreateScenarioViewModel), Constants.MAINDIALOGIDENTIFIER,null,OnCreatingScenarioClosed);
        }
        private async void OnCreatingScenarioClosed(object obj)
        {
            if (int.TryParse(obj?.ToString(),out int newScenarioId) && newScenarioId > -1)
            {
                _viewResolver.RefreshAllViews();
                await CurrentRepository.Refresh();
                var newScenario = await CurrentRepository.FindAsync(newScenarioId);
                Messenger.Send(new ChangeCurrentViewMessage(CurrentView.RunScenarioView));
                Messenger.Send(new DataModelMessage<ScenarioModel>(newScenario));
            }
            else if(obj is ErrorEventsArgs error)
                ShowMessage(error.ErrorMessage, error.Level);
        }
        protected override async Task DeleteRow(ScenarioModel row)
        {
            var success = await _runScenarioUnitOfWork.DeleteScenario(row.ScenarioId);
            if (success)
            {
                Messenger.Send(new DataModelMessage<DialogResult>(DialogResult.Retry));
                ShowMessage($"The scenario: \"{row.ScenarioFullName}\" has been deleted successfully.",LogLevel.None,true,"Delete");
            }
        }
        protected override Task Loaded()
        {
            InitializeTokenSource();
            return base.Loaded();   
        }
    }
    
}   
