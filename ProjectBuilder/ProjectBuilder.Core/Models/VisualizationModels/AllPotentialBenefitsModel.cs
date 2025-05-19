using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core 
{
    [ExcludeFromCodeCoverage]
    public class AllPotentialBenefitsModel
    {
       public int ScenarioId { get; set; }
       public byte District { get; set; }
       public int? TreatmentYear { get; set; }
       public double? InterstateBenefit { get; set; }
       public double? NonInterstateBenefit { get; set; }
    }
    public class BridgePotentialBenefitsModel : AllPotentialBenefitsModel { }
    public class PavementPotentialBenefitsModel : AllPotentialBenefitsModel { }
}
