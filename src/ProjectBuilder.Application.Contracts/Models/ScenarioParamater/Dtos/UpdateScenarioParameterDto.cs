using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ScenarioParamater.Dtos
{
    public class UpdateScenarioParameterDto : EntityDto<string>
    {
        public string ParmName { get; set; }
        public string ParmDescription { get; set; }
        public int ScenarioId { get; set; }
        public double? ParameterValue { get; set; }
    }
}
