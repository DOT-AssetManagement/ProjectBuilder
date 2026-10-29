using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ScenarioParamater.Dtos
{
    public class ScenarioParameterDto : EntityDto<string>
    {
        public string ParameterName { get; set; }
        public string ParameterDescription { get; set; }
        public int ScenarioId { get; set; }
        public double? ParameterValue { get; set; }
        public string ParameterId { get; set; }
    }
}
