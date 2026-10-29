using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{

    public class BudgetEntity : Entity<int?>
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
        public int? Id { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
    public class BudgetSpentEntity : Entity<int?>
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
        public int? Id { get ; set ; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
