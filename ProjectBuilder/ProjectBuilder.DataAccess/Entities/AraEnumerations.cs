using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Entities
{
    [Table("tbl_ara_Enumerations")]
    public class AraEnumerations: IEntity<string>
    {
        [Column("EnumFamily")]
        public string EntityId { get; set; }
        public string EnumName { get; set; }
        public int EnumInt { get; set; }
    }
}
