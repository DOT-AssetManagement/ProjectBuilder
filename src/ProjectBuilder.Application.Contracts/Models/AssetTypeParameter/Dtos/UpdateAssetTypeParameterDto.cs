using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.AssetTypeParameter.Dtos
{
    public class UpdateAssetTypeParameterDto : EntityDto<Guid>
    {
        public string AssetType { get; set; }
        public float BenefitScalingFactor { get; set; }
        public Guid? ScenId { get; set; }
    }
}
