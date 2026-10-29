using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_TreatmentCancellationMatrix")]
    public class TreatmentCancellationMatrixEntity : Entity<string>
    {
        public char AssetTypeA { get; set; }
        [Column("TreatmentA")]
        public string Id { get; set; }
        public char AssetTypeB { get; set; }
        public string TreatmentB { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }

    }
}
