using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    [Table("tbl_pb_Parameters")]
    public class ParameterEntity : Entity<string>
    {
        [Column("ParmID")]
        public string Id { get; set; }
        public string Parmfamily { get; set; }
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        public double? DefaultValue { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
