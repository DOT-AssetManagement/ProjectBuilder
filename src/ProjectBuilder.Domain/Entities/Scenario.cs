using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_Scenarios")]
    public class Scenario : Entity<int>
    {
        [Column("ScenId")]
        public int Id { get; set; }
        public Guid? LibraryId { get; set; }

        [Required,MaxLength(100)]
        public string ScenarioName { get; set; } = "Unknown";
        [NotMapped] public string? Description { get; set; }
        [NotMapped] public string? DeaInputSproc { get; set; }
        [NotMapped] public string? DeaOutputSproc { get; set; }

        // No [Required] here: the column is nullable, and marking it required makes EF read it with
        // an unguarded reader.GetString() that throws SqlNullValueException on a NULL row.
        [MaxLength(50)]
        public string? CreatedBy { get; set; } = "None";
     
        [Column(TypeName ="datetime")]
        public DateTime CreatedAt { get; set; }
     
        [MaxLength(50)]
        public string? LastRunBy { get; set; } = "None";
     
        [Column(TypeName = "datetime")]
        public DateTime? LastRunAt { get; set; }
     
        [Column(TypeName = "datetime")]
        public DateTime? LastStarted { get; set; }
     
        [Column(TypeName = "datetime")]
        public DateTime? LastFinished { get; set; }
        public bool? Locked { get; set; }
        public string? Notes { get; set; } = "None";
        public string? CriteriaNotes { get; set; }
        public bool Stale { get; set; }
        public bool IsShared { get; set; }

        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
