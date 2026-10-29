using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    // tbl_pb_BudgetConstraintsCriteria — the criteria string attached to a budget column.
    [Table("tbl_pb_BudgetConstraintsCriteria")]
    public class BudgetConstraintCriteria : Entity<int>
    {
        public int ScenId { get; set; }
        public int BCCId { get; set; }
        public string Criteria { get; set; }
    }
}
