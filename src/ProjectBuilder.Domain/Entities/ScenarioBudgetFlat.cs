
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    public class ScenarioBudgetFlat  : Entity<int>
    {
        public int Id { get; set; }
        public int ScenarioId { get; set; }
        public int District { get; set; }  
        public decimal? BridgeInterstateBudget { get; set; }
        public decimal? BridgeNonInterstateBudget { get; set; }
        public decimal? PavementInterstateBudget { get; set; }
        public decimal? PavementNonInterstateBudget { get; set; }
        public int YearWork { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
