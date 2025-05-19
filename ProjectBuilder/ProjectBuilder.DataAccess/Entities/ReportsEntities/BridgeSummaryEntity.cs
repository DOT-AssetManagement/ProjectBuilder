using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class CombinedProjectSummaryEntity : IEntity<int?> 
    {
        [Column("ScenId")]
        public int ScenarioId { get; set; }
        public string ProjectType { get; set; }
        public byte District { get; set; }
        public int? Projects { get; set; }
        public int? Treatments { get; set; }
        public double? Cost { get; set; }
        public double? Benefit { get; set; }

        [Column("SelectedYear")]
        public int? EntityId { get; set; }
    }
}

