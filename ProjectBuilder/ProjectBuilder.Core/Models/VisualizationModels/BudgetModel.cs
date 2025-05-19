using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class BudgetModel
    { 
        public int ScenarioId { get; set; }
        public int District { get; set; }  
        public decimal? BridgeBudgetInterstate { get; set; }
        public decimal? BridgeBudgetNonInterstate { get; set; }
        public decimal? PavementBudgetInterstate { get; set; }
        public decimal? PavementBudgetNonInterstate { get; set; }
        public int? ScenarioYear { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class BudgetSpentModel
    {
        public int? ScenarioId { get; set; }
        public int? District { get; set; }
        public double? BridgeBudgetInterstate { get; set; }
        public double? BridgeBudgetNonInterstate { get; set; }
        public double? PavementBudgetInterstate { get; set; }
        public double? PavementBudgetNonInterstate { get; set; }
        public int? ScenarioYear { get; set; }
    }
}
