using System.Collections.Generic;

namespace ProjectBuilder.Models.ProjectTreatment.Dtos
{
    public class ScenarioTreatmentsPageDto
    {
        public List<ProjectTreatmentDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
