using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Entities
{
    public class ScenarioProjectObjectiveValueEntity
    {
        public Guid ScenId { get; set; }
        public Guid ProjectId { get; set; }
        public float? RelativeEfficiency { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
