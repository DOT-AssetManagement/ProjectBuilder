using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Entities
{
    [Table("tbl_pb_TreatmentCancellationMatrix")]
    public class TreatmentCancellationMatrixEntity : IEntity<string>
    {
        public char AssetTypeA { get; set; }
        [Column("TreatmentA")]
        public string EntityId { get; set; }
        public char AssetTypeB { get; set; }
        public string TreatmentB { get; set; }

    }
}
