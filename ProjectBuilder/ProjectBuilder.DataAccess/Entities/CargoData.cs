using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pb_CargoData")]
    public class CargoData : IEntity<Guid>
    {
        [Column("ImportTimeGeneratedGuid")]
        public Guid EntityId { get; set; }

        public Guid ImportSessionId { get; set; }

        public int AttributeNo { get; set; }

        public string? TextValue { get; set; }

        public double? NumericValue { get; set; }
    }
}
