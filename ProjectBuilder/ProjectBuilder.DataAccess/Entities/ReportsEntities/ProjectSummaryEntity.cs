using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class ProjectSummaryEntity : IEntity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public int? SelectedYear { get; set; }
        public double? Cost { get; set; }
        public double? Benefit { get; set; }
        [Column("Projects")]
        public int? EntityId { get; set; }
        [Column("Treatments")]
        public int TreatmentId { get; set; }
    }
}
