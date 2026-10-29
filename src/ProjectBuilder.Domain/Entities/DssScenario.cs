using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_dss_Scenarios")]
    public class DssScenario : Entity<Guid>
    {
        [Column("Id")]
        public Guid Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScenNo { get; set; }

        public string ScenName { get; set; }
        public string? Description { get; set; }
        public string? DeaInputSproc { get; set; }
        public string? DeaOutputSproc { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? PoolId { get; set; }
    }
}
