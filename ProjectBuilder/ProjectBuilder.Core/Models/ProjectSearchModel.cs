using System;

namespace ProjectBuilder.Core
{
    public class ProjectSearchModel
    {
        public byte? County { get; set; }
        public byte? District { get; set; }
        public int? Route { get; set; }
        public int? ScenarioId { get; set; }
        public int? ProjectId { get; set; }
        public string Section { get; set; }
        public int? Year { get; set; }
    }
}
