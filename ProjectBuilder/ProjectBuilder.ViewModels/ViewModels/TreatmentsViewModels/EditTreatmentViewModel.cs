using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public partial class EditTreatmentViewModel : TreatmentBaseViewModel
    {
        #region Fields
        private TreatmentModel _editedtreatment;
        List<string> _dependentProperties;
        #endregion

        #region Observable Properties
        [ObservableProperty]
        private string _assetType;
        [ObservableProperty]
        private string _district;
        [ObservableProperty]
        private string _county;
        [ObservableProperty]
        private string _route;
        [ObservableProperty]
        private string _section;
        #endregion

        #region Commands
        public IRelayCommand ResetCommand { get; }
        #endregion

        #region Constructor
        public EditTreatmentViewModel(ITreatmentRepository repository, IMessenger messenger, IDialogService dialogService)
                                     : base(repository, messenger, dialogService)
        {
            ResetCommand = new RelayCommand(Reset, () => HasPropertiesChanged);
            CurrentViewModelToken = "EditTreatment";
            _dependentProperties = new List<string>()
            {
                nameof(SelectedPreferredYear),
                nameof(MaxYear),
                nameof(MinYear),
                nameof(SelectedPriorityIndex),
                nameof(Committed),
                nameof(Cost),
                nameof(Risk),
                nameof(Benefit)
            };
        }
        #endregion

        #region Private Methods
        protected override async Task Loaded()
        {
            await base.Loaded();
            HideMessage();
            LoadSelectedTreatment();
            IsLoaded = true;
        }
        protected override async Task SaveTreatment()
        {
            var editedTreatment = new TreatmentModel
            {
                TreatmentId = _editedtreatment.TreatmentId,
                AssetType = AssetType == "Bridge" ? "B" : "P",
                Benefit = double.Parse(Benefit) * 1_000_000,
                Cost = double.Parse(Cost) * 1_000_000,
                County = _editedtreatment.County,
                CountyId = _editedtreatment.CountyId,
                District = _editedtreatment.District,
                IsCommitted = Committed,
                MaxYear = int.Parse(MaxYear),
                MinYear = int.Parse(MinYear),
                PreferredYear = SelectedPreferredYear,
                Risk = double.Parse(Risk),
                PriorityOrder = SelectedPriorityIndex,
                Route = _editedtreatment.Route,
                FromSection = _editedtreatment.FromSection,
                ToSection = _editedtreatment.ToSection,
                TreatmentName = _editedtreatment.TreatmentName,
                Interstate = _editedtreatment.Interstate,
                Direction = _editedtreatment.Direction,
                BenefitCostRatio = _editedtreatment.BenefitCostRatio,
                ImportedBenefit = _editedtreatment.ImportedBenefit,
                Offset = _editedtreatment.Offset,
                ScenarioId = _editedtreatment.ScenarioId,
                ProjectId = _editedtreatment.ProjectId,
                BridgeId = _editedtreatment.BridgeId,        
                Brkey = _editedtreatment.Brkey,
                ImportedTreatmentId = _editedtreatment.ImportedTreatmentId,
                SelectedYear = _editedtreatment.SelectedYear
            };
            TreatmentRepo.Update(_editedtreatment, editedTreatment);
            await TreatmentRepo.SaveChangesAsync(CurrentCancellationToken);
            HasPropertiesChanged = false;
            _editedtreatment = editedTreatment;
            SaveChangesCommand?.NotifyCanExecuteChanged();
            ResetCommand?.NotifyCanExecuteChanged();
            base.SaveTreatment();
            ShowMessage("The treatment has been updated successfully.", LogLevel.None, true, "CloudCheck");
        }
        private void Reset()
        {
            LoadSelectedTreatment();
            ValidateAllProperties();
            HasPropertiesChanged = false;
            SaveChangesCommand.NotifyCanExecuteChanged();
            ResetCommand.NotifyCanExecuteChanged();
        }
        private void LoadSelectedTreatment()
        {
            if (_editedtreatment is null)
                return;
            AssetType = _editedtreatment.AssetType == "B" ? "Bridge" : "Pavement";
            IsBRIdVisible = _editedtreatment.AssetType == "B";
            TreatmentName = _editedtreatment.TreatmentName;
            Committed = _editedtreatment.IsCommitted.HasValue ? _editedtreatment.IsCommitted.Value : false;
            Cost = _editedtreatment.Cost.HasValue ? Math.Round(_editedtreatment.Cost.Value / 1_000_000, 2).ToString() : "";
            Benefit = _editedtreatment.Benefit.HasValue ? Math.Round(_editedtreatment.Benefit.Value / 1_000_000, 2).ToString() : "";
            Risk = _editedtreatment.Risk.HasValue ? _editedtreatment.Risk.Value.ToString() : "";
            District = _editedtreatment.District?.ToString();
            SelectedPreferredYear = _editedtreatment.PreferredYear;
            MinYear = _editedtreatment.MinYear?.ToString();
            MaxYear = _editedtreatment.MaxYear?.ToString();
            SelectedPriorityIndex = _editedtreatment.PriorityOrder.HasValue ? _editedtreatment.PriorityOrder.Value : -1;
            BridgeId = _editedtreatment.BridgeId?.ToString();
            BRKey = _editedtreatment.Brkey;
            County = _editedtreatment.County;
            Route = _editedtreatment.Route.ToString();
            Section = _editedtreatment.Section;
            Interstate = _editedtreatment.Interstate.HasValue ? _editedtreatment.Interstate.Value : false;
            Direction = _editedtreatment.Direction.HasValue ? Convert.ToBoolean(_editedtreatment.Direction.Value) : false;
            CurrentAssetType = _editedtreatment.AssetType;
            HasPropertiesChanged = false;
            ClearErrors();
            SaveChangesCommand?.NotifyCanExecuteChanged();
            ResetCommand?.NotifyCanExecuteChanged();
        }
        protected override void OnTreatementReceived(TreatmentModel treatment)
        {
            _editedtreatment = treatment;
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (IsLoaded && !HasPropertiesChanged && _dependentProperties.Contains(e.PropertyName))
            {
                HasPropertiesChanged = true;
                SaveChangesCommand?.NotifyCanExecuteChanged();
                ResetCommand?.NotifyCanExecuteChanged();
            }
            base.OnPropertyChanged(e);
        }
        protected override void ValidateProperties()
        {
            ValidateProperty(SelectedPreferredYear, nameof(SelectedPreferredYear));
            ValidateProperty(MaxYear, nameof(MaxYear));
            ValidateProperty(MinYear, nameof(MinYear));
            ValidateProperty(SelectedPriorityIndex, nameof(SelectedPriorityIndex));
            ValidateProperty(Committed, nameof(Committed));
            ValidateProperty(Cost, nameof(Cost));
            ValidateProperty(Risk, nameof(Risk));
            ValidateProperty(Benefit, nameof(Benefit));
        }
        protected override async Task Unloaded()
        {
            if (HasErrors)
                return;
            await base.Unloaded();
            HasPropertiesChanged = false;
        }
        #endregion
    }
}
