using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ProjectBuilder.DataAccess.Entities
{
    [Table("tbl_pb_BridgeToPavements")]
    public class BridgeToPavements:IEntity<string>
    {
        [Column("BRKEY")]
        public string EntityId { get; set; }
        [Column("DISTRICT")]
        public int District { get; set; }
        [Column("COUNTY")]
        public string  County { get; set; }
        [Column("BRIDGE_ID")]
        public long BridgeId { get; set; }
        [Column("County_Code")]
        public int CountyCode { get; set; }
        public int Route { get; set; }
        public int Segment { get; set; }
        [Column("Offset")]
        public int OffSet { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
