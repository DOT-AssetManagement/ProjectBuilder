using System;

namespace ProjectBuilder.Core
{
    public class UserTreatmentModel
    {
        public Guid LibraryId { get; set; }
        public int ImportedTreatmentId { get; set; }
        public Guid ImportTimeGeneratedId { get; set; }

        public Guid SimulationId { get; set; }

        public Guid NetworkId { get; set; }

        public Guid AssetId { get; set; }
        public string AssetType { get; set; }

        public string Asset { get; set; }

        public byte? District { get; set; }

        public byte? CountyId { get; set; }

        public string County { get; set; }

        public int? Route { get; set; }

        public byte? Direction { get; set; }

        public int? FromSection { get; set; }

        public int? ToSection { get; set; }

        public string Section { get => $"{FromSection}-{ToSection}";  }

        public int? Offset { get; set; }

        public bool? Interstate { get; set; }

        public string Treatment { get; set; }

        public double? Benefit { get; set; }

        public double? Cost { get; set; }
        public double? Risk { get; set; }
        public byte? PriorityOrder { get; set; }

        public int? PreferredYear { get; set; }

        public int? SelectedYear { get; set; }

        public int? MinYear { get; set; }

        public int? MaxYear { get; set; }

        public string Brkey { get; set; }

        public long? BridgeId { get; set; }
        public double? RemainingLife { get; set; }
        public bool? TreatmentFundingIgnoresSpendingLimit { get; set; }

        public byte? TreatmentStatus { get; set; }

        public byte? TreatmentCause { get; set; }
        public bool? IsCommitted { get; set; }
        public bool? IsIsolatedBridge { get; set; }

        public bool? IsUserTreatment { get; set; }
        public int? UserTreatmentTypeNo { get; set; }
        public string UserTreatmentName { get; set; } = "";
        public double? IndirectCostDesign { get; set; }
        public double? IndirectCostRow { get; set; }

        public double? IndirectCostUtilities { get; set; }

        public double? IndirectCostOther { get; set; }

        public string? MPMSID { get; set; }

    }
}
