using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.SymbolStore;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pb_ImportedTreatments")]
    public class Treatment :IEntity<int>
    {    
        [Column("TreatmentId")]
        public int EntityId { get; set; } = 0;
        public Guid ImportTimeGeneratedId { get; set; }
        public Guid SimulationId { get; set; }
        public Guid NetworkId { get; set; }
        public Guid AssetId { get; set; }
        public string AssetType { get; set; }
        public string Asset { get; set; }
        public byte? District { get; set; }
        [Column("Cnty")]
        public byte? CountyId { get; set; }
        [NotMapped]
        public  CountyEntity County { get; set; }
        public int? Route { get; set; }
        public byte? Direction { get; set; }
        public int? FromSection { get; set; }
        public int? ToSection { get; set; }
        public int? Offset { get; set; }
        public bool? Interstate { get; set; }
        [Column("Treatment")]
        public string TreatmentName { get; set; }
        public double? Benefit { get; set; }
        public double? Cost { get; set; }
        public double? Risk { get; set; }
        public int? PriorityOrder { get; set; }
        public int? PreferredYear { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        [Column("BRKEY")]
        public string Brkey { get; set; }
        [Column("BRIDGE_ID")]
        public long? BridgeId { get; set; }
        public double? RemainingLife { get; set; }
        public bool? TreatmentFundingIgnoresSpendingLimit { get; set; }
        public byte? TreatmentStatus { get; set; }
        public byte? TreatmentCause { get; set; }
        public bool? IsCommitted { get; set; }
        public bool? IsIsolatedBridge { get; set; }

        public string PopulatedBy { get; set; }

        public DateTime? PopulatedAt { get; set; }

    }
}
