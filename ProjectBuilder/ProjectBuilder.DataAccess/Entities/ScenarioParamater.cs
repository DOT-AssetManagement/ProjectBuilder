using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class ScenarioParamater : IEntity<string>
    {
        [Column("ParmID")]
        public string EntityId { get; set; }     
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        [Column("ScenId")]
        public int ScenarioId { get; set; }
        [Column("ParmValue")]
        public double? ParameterValue  { get; set; }
    }
}
