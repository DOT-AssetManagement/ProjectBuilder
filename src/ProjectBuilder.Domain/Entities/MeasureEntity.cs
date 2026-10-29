using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_Measures")]
    public class MeasureEntity : Entity<Guid>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public byte? Status { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
