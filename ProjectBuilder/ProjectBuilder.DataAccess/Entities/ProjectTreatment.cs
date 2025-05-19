using ProjectBuilder.DataAccess.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    public class ProjectTreatment : IEntity<long>
    {
        [Column("ExtendedTreatmentId")]
        public long EntityId { get; set; }
        [Column("ScenId")]
        public int? ScenarioId { get; set; }
        public int? ProjectId { get; set; }
        public string AssetType { get; set; }
        public byte? District { get; set; }
        public byte? CountyId { get; set; }
        public string County { get; set; }
        public int? Route { get; set; }
        public byte? Direction { get; set; }
        public string? Section { get; set; }
        [Column("BRKEY")]
        public string Brkey { get; set; }
        [Column("BRIDGE_ID")]
        public long? BridgeId { get; set; }
        public bool? Interstate { get; set; }
        public string TreatmentType { get; set; }
        public double? TotalCost { get; set; }
        [Column("Cost")]
        public double? DirectCost { get; set; }
        public double? IndirectCostDesign { get; set; }
        [Column("IndirectCostROW")]
        public double? IndirectCostRow { get; set; }
        public double? IndirectCostUtilities { get; set; }
        public double? IndirectCostOther { get; set; }
        public double? Benefit { get; set; }
        public double? Risk { get; set; }
        public bool? IsCommitted { get; set; }
        public int? PriorityOrder { get; set; }
        public bool? IsUserCreated { get; set; }

        public int? PreferredYear { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int? SelectedYear { get; set; }
        //public DateTime? CreatedAt { get; set; }
        public int? UserTreatmentTypeNo { get; set; }
        [NotMapped]
        public string UserTreatmentName { get; set; }

        [Column("ImportTimeGeneratedId")]
        public Guid ImportTimeGeneratedId { get; set; }
    }
}
