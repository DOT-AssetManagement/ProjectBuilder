using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Entities
{
    [Table("tbl_pb_ImportSessions")]
    public class ImportSessions:IEntity<Guid>
    {
        public int NoId { get; set; }
        [Column("Id")]
        public Guid EntityId { get; set; }
        [Required]
        public string ImportSource { get; set; }
        [Required]
        public string DataSourceType { get; set; }
        [Required]
        public string DataSourceName { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedStatus { get; set; }
        public string Notes { get; set; }
    }
}
