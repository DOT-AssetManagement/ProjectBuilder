using System;
using log4net;
using log4net.Appender;
using log4net.Layout;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pb_Scenarios")]
    public class Scenario : IEntity<int>
    {
        [Column("ScenId")]
        public int EntityId { get; set; }
        public Guid? LibraryId { get; set; }

        [Required,MaxLength(100)]
        public string ScenarioName { get; set; } = "Unknown";
      
        [Required, MaxLength(50)]
        public string CreatedBy { get; set; } = "None";
     
        [Column(TypeName ="datetime")]
        public DateTime CreatedAt { get; set; }
     
        [MaxLength(50)]
        public string LastRunBy { get; set; } = "None";
     
        [Column(TypeName = "datetime")]
        public DateTime? LastRunAt { get; set; }
     
        [Column(TypeName = "datetime")]
        public DateTime? LastStarted { get; set; }
     
        [Column(TypeName = "datetime")]
        public DateTime? LastFinished { get; set; }
        public bool? Locked { get; set; }     
        public string Notes { get; set; } = "None";
        public bool Stale { get; set; }
    }
}
