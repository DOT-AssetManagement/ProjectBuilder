using System;

namespace ProjectBuilder.Models.AssetTypeParameter.Dtos
{
    public class CreateAssetTypeParameterDto
    {
        public string AssetType { get; set; }
        public float BenefitScalingFactor { get; set; }
        public string CreatedBy { get; set; }
        public Guid? ScenId { get; set; }
    }
}
