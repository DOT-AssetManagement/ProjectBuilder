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
    public class CombinedProjectModel
    {
        public int ScenarioId { get; set; }
        public string ProjectType { get; set; }
        public byte District { get; set; }
        public int? Projects { get; set; }
        public int? Treatments { get; set; }
        public double? TotalCost { get; set; }
        public double? Benefit { get; set; }
        public int? SelectedYear { get; set; }
    }
}
