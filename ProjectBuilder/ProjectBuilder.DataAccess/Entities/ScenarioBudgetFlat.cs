
namespace ProjectBuilder.DataAccess
{
    public class ScenarioBudgetFlat  : IEntity<int>
    {
        public int EntityId { get; set; }
        public int ScenarioId { get; set; }
        public int District { get; set; }  
        public decimal? BridgeInterstateBudget { get; set; }
        public decimal? BridgeNonInterstateBudget { get; set; }
        public decimal? PavementInterstateBudget { get; set; }
        public decimal? PavementNonInterstateBudget { get; set; }
    }
}
