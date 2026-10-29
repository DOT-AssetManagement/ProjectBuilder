using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.AssetTypeParameter.Dtos
{
    public class AssetTypeParameterDto : EntityDto<Guid>
    {
        public string AssetType { get; set; }
        public float BenefitScalingFactor { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? ScenId { get; set; }
    }
}
