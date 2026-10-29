using System.Collections.Generic;

namespace ProjectBuilder.Models.ScenarioParamater.Dtos
{
    public class SaveParametersInputDto
    {
        public Dictionary<string, double?> ParameterValues { get; set; } = new();
        public List<TreatmentShiftDto> Shifts { get; set; } = new();
    }
}
