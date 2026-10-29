using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_Goals")]
    public class GoalEntity : Entity<Guid>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public byte? Status { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
