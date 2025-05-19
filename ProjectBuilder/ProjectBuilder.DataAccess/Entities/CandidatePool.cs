using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    public class CandidatePool : IEntity<Guid>
    {
        [Column("Id")]
        public Guid EntityId { get; set; }      
        [Column("LibNo")]
        public int CandidatePoolNumber { get; set; }
        public long UserId { get; set; }
        [NotMapped]
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public int TreatmentsCount { get; set; }
        [NotMapped]
        public int BridgeTreatmentsCount { get; set; }
        [NotMapped]
        public int PavementTreatmentsCount { get; set; }
        [NotMapped]
        public int ScenarioCount { get; set; }
        [NotMapped]
        public DateTime? PopulatedAt { get; set; }
        [NotMapped]
        public string Source { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        [Column("IsSharedLibrary")]
        public bool IsShared { get; set; }
    }
}
