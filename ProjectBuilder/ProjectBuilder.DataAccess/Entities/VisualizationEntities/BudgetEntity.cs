using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace ProjectBuilder.DataAccess
{

    public class BudgetEntity : IEntity<int?>
    {
        [Column("Scenario")]
        public int ScenarioId { get; set; }
        public int District { get; set; }
        [Column("BridgeInter")]
        public decimal? BridgeBudgetInterstate { get; set; }
        [Column("BridgeNonInter")]
        public decimal? BridgeBudgetNonInterstate { get; set; }
        [Column("PaveInter")]
        public decimal? PavementBudgetInterstate { get; set; }
        [Column("PaveNonInter")]
        public decimal? PavementBudgetNonInterstate { get; set; }
        [Column("ScenarioYear")]
        public int? EntityId { get; set; }
    }
    public class BudgetSpentEntity : IEntity<int?>
    {
        [Column("Scenario")]
        public int? ScenarioId { get; set; }
        public int? District { get; set; }
        [Column("BridgeInter")]
        public double? BridgeBudgetInterstate { get; set; }
        [Column("BridgeNonInter")]
        public double? BridgeBudgetNonInterstate { get; set; }
        [Column("PaveInter")]
        public double? PavementBudgetInterstate { get; set; }
        [Column("PaveNonInter")]
        public double? PavementBudgetNonInterstate { get; set; }
        [Column("ScenarioYear")]
        public int? EntityId { get ; set ; }
    }
}
