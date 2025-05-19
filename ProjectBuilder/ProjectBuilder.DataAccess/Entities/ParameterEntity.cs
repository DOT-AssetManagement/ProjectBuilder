using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Entities
{
    [Table("tbl_pb_Parameters")]
    public class ParameterEntity : IEntity<string>
    {
        [Column("ParmID")]
        public string EntityId { get; set; }
        public string Parmfamily { get; set; }
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        public double? DefaultValue { get; set; }
    }
}
