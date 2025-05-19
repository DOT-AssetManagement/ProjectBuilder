using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectBuilder.DataAccess
{
    public class TreatmentSummaryEntity : IEntity<int>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public int? SelectedYear { get; set; }
        public double? Cost { get; set; }
        public double? Benefit { get; set; }
        [Column("Projects")]
        public int? ProjectId { get; set; }
        [Column("TreatmentDesc")]
        public string TreatmentDescription { get; set; }
        public string AssetType { get; set; }
        [Column("Treatments")]
        public int EntityId { get; set ; }
    }
}
