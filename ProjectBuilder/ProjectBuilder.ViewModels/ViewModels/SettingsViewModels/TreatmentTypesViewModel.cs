using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public class TreatmentTypesViewModel : DatabaseSourceViewModel<TreatmentTypeModel>
    {
     
        public TreatmentTypesViewModel(IRepository<TreatmentTypeModel> repository,IDialogService dialog,ISettingsService settings,IMessenger messenger)                                        
                                 :base(repository,dialog,settings,messenger)
        {
            CurrentModelToken = nameof(TreatmentTypesViewModel);
        }
        protected override void EditRow()
        {
            
        }
        protected override Task Loaded()
        {
            //InitializeTokenSource();
            return base.Loaded();
        }
    }
}
