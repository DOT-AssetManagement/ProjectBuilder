using ProjectBuilder.Models.ParameterEntity.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.ScenarioParamater.Dtos
{
    public class CreateScenarioParameterDto
    {
        public string Id { get; set; }  
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        public int ScenarioId { get; set; }
        public double? ParameterValue { get; set; }
        public List<ParameterDto> parameters { get; set; }
    }
}
