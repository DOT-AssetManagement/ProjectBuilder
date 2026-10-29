using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    public class ScenarioParamater : Entity<string>
    {
        [Column("ParmID")]
        public string Id { get; set; }     
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        [Column("ScenId")]
        public int ScenarioId { get; set; }
        [Column("ParmValue")]
        public double? ParameterValue  { get; set; }
        public object?[] GetKeys()
        {
            return new object[] { Id };
        }
    }
}
