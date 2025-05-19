using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class ProjectSummaryModel
    {
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public int? SelectedYear { get; set; }
        public double? Cost { get; set; }
        public double? Benefit { get; set; } 
        public int? ProjectId { get; set; } 
        public int TreatmentId { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class TreatmentSummaryModel
    {     
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public int? SelectedYear { get; set; }
        public double? Cost { get; set; }
        public double? Benefit { get; set; }  
        public int? ProjectId { get; set; }
        public string TreatmentDescription { get; set; }
        public string AssetType { get; set; }
        public int TreatmentId { get; set; }
    }
}
