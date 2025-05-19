using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;

namespace ProjectBuilder.ViewModels
{
    public partial class TreatmentsViewModel : FilterBaseViewModel<TreatmentModel>,IRecipient<RefreshNeededMessage>
    {
        [ObservableProperty]
        private int _selectedAssetIndex;

        #region Constructor
        public TreatmentsViewModel(IFilterUnitOfWork filterUnitOfWork,IDialogService dialogService,ISettingsService settingsService,
                                   IMessenger messenger)  
                                  : base(filterUnitOfWork, filterUnitOfWork.TreatmentRepo,dialogService,settingsService,messenger)                                
        {
            Messenger.Register<RefreshNeededMessage>(this);
            CurrentModelToken = nameof(TreatmentsViewModel);
            SelectedAssetIndex = -1;
        }

        #endregion

        protected override void EditRow()
        {
            Messenger.Send(new ChangeCurrentViewMessage(CurrentView.EditTreatmentView));
            Messenger.Send(new DataModelMessage<TreatmentModel>(SelectedRow,CurrentView.TreatmentsView));
        }
        [RelayCommand]
        private void CreateTreatment()
        {
            Messenger.Send(new ChangeCurrentViewMessage(CurrentView.NewTreatmentView));
            Messenger.Send(new DataModelMessage<TreatmentModel>(null, CurrentView.TreatmentsView));
        }
        protected override async Task Filter()
        {
            var filter = new TreatmentModel
            {
                ScenarioId = SelectedScenario is null ? -1 : SelectedScenario.ScenarioId,
                ProjectId = SelectedProject is null ? null : SelectedProject.ProjectId,
                District = (byte?)SelectedDistrict,
                CountyId = SelectedCounty is null ? (byte)0 : (byte)(SelectedCounty.CountyId.HasValue ? (byte)SelectedCounty.CountyId.Value : 0),
                Route = SelectedRoute.HasValue ? SelectedRoute.Value : 0,
                SelectedYear = SelectedYear
            };
            var sectionsValues = SelectedSection.DecomposeString<int>('-');
            if (sectionsValues.Length >= 2)
            {
                filter.FromSection = sectionsValues[0];
                filter.ToSection = sectionsValues[1];
            }
            switch (SelectedAssetIndex)
            {
                case 0:
                    filter.AssetType = "B";
                    break;
                case 1:
                    filter.AssetType = "P";
                    break;
                default:
                    filter.AssetType = null;
                    break;
            }
            CurrentRepository.ApplyFilter(filter);
            await base.Filter();
            if (DataSource.Count <= 0)
                ShowMessage("No Treatments were Found.", LogLevel.Warning);
        }
        public void Receive(RefreshNeededMessage message)
        {
            RefreshNeeded = message.RefreshNeeded;
        }
    }
}
