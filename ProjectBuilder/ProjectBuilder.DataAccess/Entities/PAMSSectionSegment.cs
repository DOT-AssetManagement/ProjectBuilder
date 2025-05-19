using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pams_MaintainableAssetsSegmentation")]
    public class PAMSSectionSegment : IEntity<Guid>
    {
        [Column("AssetId")]
        public Guid EntityId { get; set; }
        public byte District { get; set; }
        [Column("Cnty")]
        public byte CountyId { get; set; }
        public int Route { get; set; }
        public int FromSection { get; set; }
        public int ToSection { get; set; }
        [Column("Direction")]
        public byte Direction { get; set; }
        public bool Interstate { get; set; }
    }
}
