using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class ProjectsViewModel : FilterBaseViewModel<ProjectModel>
    {     
        public ProjectsViewModel(IFilterUnitOfWork filterUnitOfWork,IDialogService dialogService,ISettingsService settings, IMessenger messenger)                             
                                : base(filterUnitOfWork,filterUnitOfWork.ProjectRepo,dialogService,settings,messenger)
        {
            CurrentModelToken = nameof(ProjectsViewModel);
        }

  
        protected override void EditRow()
        {
            Messenger.Send(new ChangeCurrentViewMessage(CurrentView.ProjectDetailsView));
            Messenger.Send(new DataModelMessage<ProjectModel>(SelectedRow));
        }
        protected override async Task Filter()
        {
            var projectFilter = new ProjectModel
            {
                ScenarioId = SelectedScenario is null ? -1 : SelectedScenario.ScenarioId,
                ProjectId = SelectedProject is null ? -1 : SelectedProject.ProjectId,
                District = (byte?)SelectedDistrict,
                CountyId = SelectedCounty is null ? null : SelectedCounty.CountyId,
                Route = SelectedRoute,
                SelectedFirstYear = SelectedYear
            };
            var sectionsValues = SelectedSection.DecomposeString<int>('-');
            if(sectionsValues.Length >= 2) 
            {
                projectFilter.MinSection = sectionsValues[0];
                projectFilter.MaxSection = sectionsValues[1].ToString();
            }
            CurrentRepository.ApplyFilter(projectFilter);
            await base.Filter();
            if (DataSource.Count < 1)
                ShowMessage("No Projects were Found", LogLevel.Warning);
        }
    }
}
