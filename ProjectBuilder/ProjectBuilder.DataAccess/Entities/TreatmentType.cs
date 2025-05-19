using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    [Table("TreatmentTypes")]
    public class TreatmentType : IEntity<string>
    {
        [Column("Treatment")]
        public string EntityId { get; set; }
        [Column("MoveEarlierBufferOverride")]
        public int? MoveEarlier { get; set; }
        [Column("MoveLaterBufferOverride")]
        public int? MoveLater { get; set; }
        public string AssetType { get; set; }
    }
}
