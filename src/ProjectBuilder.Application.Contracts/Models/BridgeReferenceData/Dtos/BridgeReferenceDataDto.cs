using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.BridgeReferenceData.Dtos
{
    public class BridgeReferenceDataDto : EntityDto<string>
    {
        public int? District { get; set; }
        public int? CountyCode { get; set; }
        public string BridgeId { get; set; }
        public int? Route { get; set; }
        public int? Segment { get; set; }
        public int? OffSet { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
