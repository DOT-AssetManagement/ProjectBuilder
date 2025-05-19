using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class NewTreatmentViewModel : TreatmentBaseViewModel
    {
        private readonly IRepository<ScenarioModel> _scenariosRepo;
        private readonly IRepository<CountyModel> _countiesReposiroty;
        [ObservableProperty]
        private List<int?> _districts;
        [ObservableProperty]
        private bool _isComboBoxEnabled;
        [ObservableProperty]
        private ObservableCollection<CountyModel> _counties;
        [ObservableProperty]
        private ObservableCollection<int> _routes;
        [ObservableProperty]
        private ObservableCollection<string> _sections;
        [ObservableProperty]
        private string _countyHint;
        [ObservableProperty]
        private string _routeHint;
        [ObservableProperty]
        private string _sectionHint;
        [ObservableProperty]
        private List<ScenarioModel> _scenarios;
        private ScenarioModel _selectedScenario;
        [Required(AllowEmptyStrings =false,ErrorMessage ="Select a Scenario")]
        public ScenarioModel SelectedScenario
        {
            get { return _selectedScenario; }
            set 
            {
               SetProperty(ref _selectedScenario, value,true);
            }
        }

        protected List<CountyModel> AvailableCounties { get; set; }
        private int _assetType;
        [ComparsionValidation(-1, ErrorMessage = "Select an Asset type")]
        public int AssetType
        {
            get { return _assetType; }
            set
            {
                if (SetProperty(ref _assetType, value))
                {
                    IsBRIdVisible = value == 0;
                    CurrentAssetType = AssetType == 0 ? "B" : "P";
                }
            }
        }
        private int? _selectedDistrict;
        [Required(ErrorMessage = "Select a District")]
        public int? SelectedDistrict
        {
            get { return _selectedDistrict; }
            set
            {
                if (SetProperty(ref _selectedDistrict, value))
                {
                    Counties?.Clear();
                    if (value is null)
                        return;
                    Counties = new(AvailableCounties.Where(c => c.District == value).OrderBy(c => c.CountyId));
                    CountyHint = "Select a County";
                    RouteHint = "Select a County first";
                    SectionHint = "Select a Route first";
                }
            }
        }

        private CountyModel _selectedCounty;
        [Required(ErrorMessage = "Select a County")]
        public CountyModel SelectedCounty
        {
            get { return _selectedCounty; }
            set
            {
                if (SetProperty(ref _selectedCounty, value))
                {
                    Routes?.Clear();
                    if (value is null)
                        return;
                    IsComboBoxEnabled = false;
                    TreatmentRepo.GetRoutesAsync(SelectedDistrict, value.CountyId).ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            Routes = new(task.Result);
                            RouteHint = "Select a Route";
                            SectionHint = "Select a Route first";
                        }
                        IsComboBoxEnabled = true;
                    }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        private int? _selectedRoute;
        [Required(ErrorMessage = "Select a Route")]
        public int? SelectedRoute
        {
            get { return _selectedRoute; }
            set
            {
                if (SetProperty(ref _selectedRoute, value))
                {
                    Sections?.Clear();
                    if (value is null)
                        return;
                    IsComboBoxEnabled = false;
                    TreatmentRepo.GetSectionsAsync(SelectedDistrict, SelectedCounty?.CountyId, SelectedRoute).ContinueWith(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            Sections = new(task.Result);
                            SectionHint = "Select a Section";
                        }
                        IsComboBoxEnabled = true;
                    }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }
        private string _selectedSection;
        [Required(AllowEmptyStrings = false, ErrorMessage = "Select Section")]
        public string SelectedSection
        {
            get { return _selectedSection; }
            set
            {
                if (SetProperty(ref _selectedSection, value, true) && IsLoaded && value is not null)
                {
                    var values = value.DecomposeString<int>('-');
                    TreatmentRepo.GetDirectionInterstate(SelectedDistrict, SelectedCounty?.CountyId, SelectedRoute, values[0], values[1]).ContinueWith(task =>
                    {
                        Interstate = task.Result.isInterstate;
                        Direction = task.Result.Direction;
                    });
                }
            }
        }
        public NewTreatmentViewModel(IRepository<ScenarioModel> scenarioRepo,IRepository<CountyModel> countiesReposiroty, ITreatmentRepository treatmentRepository, IMessenger messenger, IDialogService dialogService) 
                              : base(treatmentRepository, messenger, dialogService)
        {
            CurrentViewModelToken = "NewTreatmentToken";
            _scenariosRepo = scenarioRepo;
            _countiesReposiroty = countiesReposiroty;
            HasPropertiesChanged = false;
            SaveChangesCommand?.NotifyCanExecuteChanged();
            SelectedDistrict = null;
            SelectedCounty = null;
            IsComboBoxEnabled = true;
            CountyHint = "Select a District first";
            RouteHint = "Select a County first";
            SectionHint = "Select a Route first";
            HasPropertiesChanged = true;
            SaveChangesCommand?.NotifyCanExecuteChanged();
        }
        protected override async Task SaveTreatment()
        {
            var newTreatment = CreateNewTreatment();
            TreatmentRepo.Insert(newTreatment);
            await TreatmentRepo.SaveChangesAsync(CurrentCancellationToken);
            HasPropertiesChanged = false;
            SaveChangesCommand.NotifyCanExecuteChanged();
            base.SaveTreatment();
            ClearPropertiesValues();
            ShowMessage("New treatment has been saved successfully", LogLevel.None, true,"CloudCheck");
        }
        private TreatmentModel CreateNewTreatment()
        {
            var newTreatemnt = new TreatmentModel()
            {
                AssetType = AssetType == 0 ? "B" : "P",
                Benefit = Math.Round(double.Parse(Benefit), 2) * 1000000,
                Cost = Math.Round(double.Parse(Cost), 2) * 1000000,
                County = SelectedCounty?.CountyFullName,
                District = SelectedDistrict is not null ? Convert.ToByte(SelectedDistrict) : default,
                IsCommitted = Committed,
                MaxYear = int.Parse(MaxYear),
                MinYear = int.Parse(MinYear),
                PreferredYear = SelectedPreferredYear,
                PriorityOrder = SelectedPriorityIndex,
                Risk = Math.Round(double.Parse(Risk), 2),
                Route = SelectedRoute.HasValue ? SelectedRoute.Value : 0,
                TreatmentName = TreatmentName,
                ScenarioId = SelectedScenario.ScenarioId,
                Interstate = Interstate,
                Direction = Convert.ToByte(Direction),
                CountyId = SelectedCounty.CountyId is not null ? Convert.ToByte(SelectedCounty.CountyId) : default,
                SelectedYear = SelectedPreferredYear
            };
            var sectionValues = SelectedSection.DecomposeString<int>('-');
            if (sectionValues.Length >= 2)
            {
                newTreatemnt.FromSection = sectionValues[0];
                newTreatemnt.ToSection = sectionValues[1];
            }
            if (AssetType == 0)
            {
                newTreatemnt.BridgeId = long.Parse(BridgeId);
                newTreatemnt.Brkey = BRKey;
            }
            return newTreatemnt;
        }
        private void ClearPropertiesValues()
        {
            SelectedDistrict = null;
            SelectedCounty = null;
            SelectedRoute = null;
            SelectedSection = null;
            BRKey = "";
            BridgeId = "";
            TreatmentName = "";
          //  MaxYear= "";
           // MinYear = "";
            SelectedPreferredYear = null;
            Cost = "";
            Risk = "";
            Benefit = "";
            ClearErrors();
        }
        protected override async Task Loaded()
        {
            await base.Loaded();
            if (!IsLoaded)
            {
                ShowMessage("Loading Scenarios please wait", LogLevel.None, false, "TimeSand");
                Scenarios = new(await _scenariosRepo.GetAllAsync(CurrentCancellationToken));
                ShowMessage("Loading Districts please wait", LogLevel.None, false, "TimerSand");
                AvailableCounties = await _countiesReposiroty.GetAllAsync(CurrentCancellationToken);
                Districts = new(AvailableCounties.OrderBy(cnt => cnt.District).Select(cnt => cnt.District).Distinct());
                HideMessage();
                IsLoaded = true;
            }
            else ClearPropertiesValues();            
        }
        protected override async Task Unloaded()
        {
            ValidateProperties();
            if (HasErrors)
            {
                ClearErrors();
                return;
            }
            await base.Unloaded(); 
        }
        protected override void ValidateProperties()
        {
            ValidateProperty(TreatmentName, nameof(TreatmentName));
            ValidateProperty(MinYear, nameof(MinYear));
            ValidateProperty(MaxYear, nameof(MaxYear));
            ValidateProperty(Cost, nameof(Cost));
            ValidateProperty(Benefit, nameof(Benefit));
            ValidateProperty(Risk, nameof(Risk));
            ValidateProperty(SelectedDistrict, nameof(SelectedDistrict));
            ValidateProperty(SelectedCounty, nameof(SelectedCounty));
            ValidateProperty(SelectedRoute, nameof(SelectedRoute));
            ValidateProperty(SelectedSection, nameof(SelectedSection));
            ValidateProperty(SelectedPreferredYear, nameof(SelectedPreferredYear));
            ValidateProperty(SelectedScenario,nameof(SelectedScenario));
            if (AssetType == 1)
                return;
            ValidateProperty(BridgeId, nameof(BridgeId));
            ValidateProperty(BRKey, nameof(BRKey));
        }
    }
}
