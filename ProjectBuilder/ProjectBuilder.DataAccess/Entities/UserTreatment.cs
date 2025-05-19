using ProjectBuilder.Core;
using ProjectBuilder.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class UserTreatment : IEntity<Guid>
    {
        public Guid LibraryId { get; set; }

        public int ImportedTreatmentId { get; set; }
        [Column("ImportTimeGeneratedId")]
        public Guid EntityId { get; set; }

        public Guid SimulationId { get; set; }

        public Guid NetworkId { get; set; }

        public Guid AssetId { get; set; }
        public string AssetType { get; set; }
        public string Asset { get; set; }

        public byte? District { get; set; }
        [Column("Cnty")]
        public byte? CountyId { get; set; }
        [NotMapped]
        public CountyEntity County { get; set; }

        public int? Route { get; set; }

        public byte? Direction { get; set; }

        public int? FromSection { get; set; }

        public int? ToSection { get; set; }

        public int? Offset { get; set; }

        public bool? Interstate { get; set; }

        public string Treatment { get; set; }
        public double? Benefit { get; set; }
        public double? Cost { get; set; }
        public double? Risk { get; set; }
        public byte? PriorityOrder { get; set; }
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
        public DateTime? PopulatedAt { get; set; }
        public bool? IsUserTreatment { get; set; }
        public int? UserTreatmentTypeNo { get; set; }
        [NotMapped]
        public string UserTreatmentName { get; set; }
        public double? IndirectCostDesign { get; set; }
        [Column("IndirectCostROW")]
        public double? IndirectCostRow { get; set; }

        public double? IndirectCostUtilities { get; set; }

        public double? IndirectCostOther { get; set; }
    }
}
