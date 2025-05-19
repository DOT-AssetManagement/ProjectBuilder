using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    [Table("tbl_pb_CargoAttributes")]
    public class CargoAttributes : IEntity<int>
    {
        [Column("AttributeNo")]
        public int EntityId { get; set; }

        public char AssetType { get; set; }

        public string AttributeName { get; set; }

        public string AttributeType { get; set; }

        public string Notes { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }        
    }

}
