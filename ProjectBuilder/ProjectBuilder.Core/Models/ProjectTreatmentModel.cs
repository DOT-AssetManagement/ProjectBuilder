using System;

namespace ProjectBuilder.Core
{
    public class ProjectTreatmentModel
    {
        public long? ProjectTreatmentId { get; set; }
        public int? ScenarioId { get; set; }
        public int? ProjectId { get; set; }
        public string AssetType { get; set; }
        public byte? District { get; set; }
        public byte? CountyId { get; set; }
        public string County { get; set; }
        public int? Route { get; set; }
        public byte? Direction { get; set; }
        public string Section { get; set; }
        public bool? Interstate { get; set; }
        public string Brkey { get; set; }
        public long? BridgeId { get; set; }
        public string TreatmentType { get; set; }
        public double? TotalCost { get; set; }
        public double? DirectCost { get; set; }
        public double? IndirectCostDesign { get; set; }
        public double? IndirectCostRow { get; set; }
        public double? IndirectCostUtilities { get; set; }
        public double? IndirectCostOther { get; set; }
        public double? Benefit { get; set; }
        public double? Risk { get; set; }
        public bool? IsCommitted { get; set; }
        public int? PriorityOrder { get; set; }
        public int? PreferredYear { get; set; }
        public int? MinYear { get; set; }
        public bool? IsUserCreated { get; set; }
        //public DateTime? CreatedAt { get; set; }
        public int? MaxYear { get; set; }
        public int? SelectedYear { get; set; }
        public int? UserTreatmentTypeNo { get; set; }
    }
}
