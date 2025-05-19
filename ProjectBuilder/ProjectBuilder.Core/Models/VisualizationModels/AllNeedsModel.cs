using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class AllNeedsModel
    {
        public int ScenarioId { get; set; }
        public byte District { get; set; }
        public int? TreatmentYear { get; set; }
        public double? InterstateCost { get; set; }
        public double? NonInterstateCost { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class BridgeNeedsModel : AllNeedsModel { }
    [ExcludeFromCodeCoverage]
    public class PavementNeedsModel : AllNeedsModel { } 
}
