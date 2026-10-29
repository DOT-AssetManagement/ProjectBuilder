using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace ProjectBuilder.Entities
{
    public class AssetTypeParameterDataEntity : Entity<Guid>
    {
        public string AssetType { get; set; }
        public float BenefitScalingFactor { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? ScenId { get; set; }
    }
}
