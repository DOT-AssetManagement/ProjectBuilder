using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class ScenarioBudgetFlatModel
    {
        public int District { get; set; }
        public int YearWork { get; set; }
        public int ScenarioId { get; set; }
        public decimal? BridgeInterstateBudget { get; set; }
        public decimal? BridgeNonInterstateBudget { get; set; }
        public decimal? PavementInterstateBudget { get; set; }
        public decimal? PavementNonInterstateBudget { get; set; }    
    }
}
