using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_pb_BudgetConstraintsColumns — a dynamic budget column header.
    [Table("tbl_pb_BudgetConstraintsColumns")]
    public class BudgetConstraintColumn : Entity<int>
    {
        public int ScenId { get; set; }
        public string ColumnName { get; set; }
    }
}
