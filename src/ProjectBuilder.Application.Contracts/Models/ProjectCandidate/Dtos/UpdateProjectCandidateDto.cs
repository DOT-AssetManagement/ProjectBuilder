using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ProjectCandidate.Dtos
{
    public class UpdateProjectCandidateDto : EntityDto<Guid>
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
        public string CreatedBy { get; set; }
    }
}
