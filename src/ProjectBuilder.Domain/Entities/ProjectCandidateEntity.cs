using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    public class ProjectCandidateEntity : Entity<Guid>
    {
        public Guid LibraryId { get; set; }
        public byte? District { get; set; }
        public byte? Cnty { get; set; }
        public string? Description { get; set; }
        [NotMapped]
        public String? CountyName { get; set; }
        public int? NumberOfWorkCandidates { get; set; }
        public string? Treatment { get; set; }
        public int? YearEarliest { get; set; }
        public int? YearLatest { get; set; }
        public float? TotalCost { get; set; }
        public float? TotalScaledBenefit { get; set; }
        public float? SafetyScore { get; set; }
        public float? MobilityAndEconomyScore { get; set; }
        public float? EquityAndAccessScore { get; set; }
        public float? ResilienceAndEnvironmentScore { get; set; }
        public float? ConditionAndPerformanceScore { get; set; }
        public float? TotalScore { get; set; }
        public float? RelativeEfficiency { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? RelativeEfficiencySetAt { get; set; }
    }
}
