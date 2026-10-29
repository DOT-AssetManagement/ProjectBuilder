using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectBuilder.Models.DssMap.Dtos
{
    public class DssMapGenerateRequestDto
    {
        [Required]
        public Guid ScenId { get; set; }
        public double? RelativeEfficiencyThreshold { get; set; }
    }
}
