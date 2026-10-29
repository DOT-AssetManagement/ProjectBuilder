using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Entities
{
    public class DssProjectEntity
    {
        public int? Id { get; set; }
        public int? PlanningPartner { get; set; }
        public int? District { get; set; }
        public int? County { get; set; }
        public string? Description { get; set; }
        public int? EarliestYear { get; set; }
        public int? LatestYear { get; set; }
        public float? Cost { get; set; }
        public float? SafetyScore { get; set; }
        public float? MobilityAndEconomyScore { get; set; }
        public float? EquityAndAccessScore { get; set; }
        public float? ResilienceAndEnvironmentScore { get; set; }
        public float? ConditionAndPerformanceScore { get; set; }
        public float? TotalScore { get; set; }
        public float? RelativeEfficiency { get; set; }
    }
}
