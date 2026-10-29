using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_ara_Enumerations")]
    public class AraEnumerations: Entity<string>
    {
        [Column("EnumFamily")]
        public string Id { get; set; }
        public string EnumName { get; set; }
        public int EnumInt { get; set; }

        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
