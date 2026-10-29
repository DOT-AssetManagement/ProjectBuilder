using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ProjectCandidate.Dtos
{
    public class ProjectCandidateDto : EntityDto<Guid>
    {
        public Guid LibraryId { get; set; }
        public byte? District { get; set; }
        public byte? Cnty { get; set; }
        public string CountyName { get; set; }
        public string Description { get; set; }
        public string AssetType { get; set; }
        public int? NumberOfWorkCandidates { get; set; }
        public string Treatment { get; set; }
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
        public DateTime? RelativeEfficiencySetAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public double? SafetyScorePercentage { get; set; }
        public double? MobilityAndEconomyScorePercentage { get; set; }
        public double? EquityAndAccessScorePercentage { get; set; }
        public double? ResilienceAndEnvironmentScorePercentage { get; set; }
        public double? ConditionAndPerformanceScorePercentage { get; set; }
        public Guid PoolId => LibraryId;
    }
}
