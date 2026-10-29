using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Treatment.Dtos
{
    public class TreatmentDto : EntityDto<byte>
    {
        public Guid ImportTimeGeneratedId { get; set; }
        public Guid SimulationId { get; set; }
        public Guid NetworkId { get; set; }
        public Guid AssetId { get; set; }
        public string AssetType { get; set; }
        public string Asset { get; set; }
        public byte? District { get; set; }
        public byte? CountyId { get; set; }
        public int? Route { get; set; }
        public byte? Direction { get; set; }
        public int? FromSection { get; set; }
        public int? ToSection { get; set; }
        public int? Offset { get; set; }
        public bool? Interstate { get; set; }
        public string TreatmentName { get; set; }
        public double? Benefit { get; set; }
        public double? Cost { get; set; }
        public double? Risk { get; set; }
        public int? PriorityOrder { get; set; }
        public int? PreferredYear { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public string Brkey { get; set; }
        public long? BridgeId { get; set; }
        public double? RemainingLife { get; set; }
        public bool? TreatmentFundingIgnoresSpendingLimit { get; set; }
        public byte? TreatmentStatus { get; set; }
        public byte? TreatmentCause { get; set; }
        public bool IsCommitted { get; set; }
        public bool? IsIsolatedBridge { get; set; }
        public string PopulatedBy { get; set; }
        public DateTime? PopulatedAt { get; set; }
        [BindProperty(Name = "filterdirection")]
        public bool? FilterDirection { get; set; }
        public int? Year { get; set; }
        [BindProperty(Name = "section")]
        public string Section { get; set; }
        [BindProperty(Name = "scenarioid")]
        public int? ScenarioId { get; set; }
     
    }
}
