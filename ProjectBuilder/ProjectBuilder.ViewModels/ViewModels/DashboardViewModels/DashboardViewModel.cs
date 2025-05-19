using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private bool _isLoaded = false;
        private readonly IDatabaseService _databaseService;
        private readonly IFilterUnitOfWork _filterUnitOfWork;
        const string ASSETTYPEPROPERTYNAME = "AssetType";
        private Dictionary<string, object> _assetTypeFilter;
        [ObservableProperty]
        private long _pavementsCount;
        [ObservableProperty]
        private long _bridgesCount;
        [ObservableProperty]
        private long _scenariosCount;
        [ObservableProperty]
        private long _projectsCount;
        public DashboardViewModel(IDatabaseService databaseService,IFilterUnitOfWork filterUnitOfWork)
        {
            _databaseService = databaseService;
            _filterUnitOfWork = filterUnitOfWork;
            _assetTypeFilter = new()
            {
                { ASSETTYPEPROPERTYNAME, "B" }
            };
        }

        [RelayCommand]
        private async Task Loaded()
        {
            if (!_isLoaded)
            {
              await CheckDatabaseConnection();
              _isLoaded = true;
            }
           await LoadCounts();
        }

        private async Task LoadCounts()
        {
            ScenariosCount = await _filterUnitOfWork.ScenarioRepo.GetCountAsync();
            ProjectsCount = await _filterUnitOfWork.ProjectRepo.GetCountAsync();
            _assetTypeFilter[ASSETTYPEPROPERTYNAME] = "B";
            _filterUnitOfWork.TreatmentRepo.ApplyFilter(_assetTypeFilter);
            BridgesCount = await _filterUnitOfWork.TreatmentRepo.GetCountAsync();
            _assetTypeFilter[ASSETTYPEPROPERTYNAME] = "P";
            _filterUnitOfWork.TreatmentRepo.ApplyFilter(_assetTypeFilter);
            PavementsCount = await _filterUnitOfWork.TreatmentRepo.GetCountAsync();
        }

        private async Task CheckDatabaseConnection()
        {
            var message = Constants.DATABASECONNECTIONSUCCEEDEDMESSAGE;
            var success = await _databaseService.TestConnection();
            if (!success)
            {
                message = Constants.DATABASECONNECTIONFAILEDMESSAGE;
                ShowMessage(message, LogLevel.Error, false);
                MessageIcon = "DatabaseRemove";
                return;
            }
            ShowMessage(message, LogLevel.None, true, "DatabaseCheck");
        }
    } 
}
