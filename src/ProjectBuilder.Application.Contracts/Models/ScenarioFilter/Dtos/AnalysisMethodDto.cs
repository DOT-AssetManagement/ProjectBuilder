using System.Collections.Generic;

namespace ProjectBuilder.Models.ScenarioFilter.Dtos
{
    public class AnalysisMethodDto
    {
        public int ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public string Notes { get; set; }
        public List<FilteringAttributeDto> FilteringAttributes { get; set; } = new();
        public List<ScenarioFilterDto> SavedFilters { get; set; } = new();
    }
}
