using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_pb_ScenarioTreatmentShift — Parameters page "Treatment Shift" tab.
    [Table("tbl_pb_ScenarioTreatmentShift")]
    public class ScenarioTreatmentShift : Entity
    {
        [Column("ScenId")]
        public int ScenId { get; set; }
        public string Treatment { get; set; }
        public int MaxDelay { get; set; }
        public int MaxAccelerate { get; set; }

        public override object?[] GetKeys() => new object[] { ScenId, Treatment };
    }
}
