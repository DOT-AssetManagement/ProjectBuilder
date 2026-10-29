using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.ScenarioBudgetFlat.Dtos
{
    public class CreateScenarioBudgetFlatDto
    {
        public int ScenarioId { get; set; }
        public int District { get; set; }
        public decimal? BridgeInterstateBudget { get; set; }
        public decimal? BridgeNonInterstateBudget { get; set; }
        public decimal? PavementInterstateBudget { get; set; }
        public decimal? PavementNonInterstateBudget { get; set; }
        public List<budgetConstraintsModel> budgetConstraints { get; set; }

    }
    public class budgetConstraintsModel
    {
        public int YearofWork { get; set; }
        public int District { get; set; }
        public decimal? bridgeInterstateBudget { get; set; }
        public decimal? bridgeNonInterstateBudget { get; set; }
        public decimal? pavementInterstateBudget { get; set; }
        public decimal? pavementNonInterstateBudget { get; set; }
    }
}
