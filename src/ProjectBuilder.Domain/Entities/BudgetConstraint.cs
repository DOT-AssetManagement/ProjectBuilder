using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_pb_BudgetConstraints — a single editable budget cell (row x column) value.
    [Table("tbl_pb_BudgetConstraints")]
    public class BudgetConstraint : Entity<int>
    {
        public int ScenId { get; set; }
        public int BCCId { get; set; }
        public decimal BudgetValue { get; set; }
        public int RowNumber { get; set; }
    }
}
