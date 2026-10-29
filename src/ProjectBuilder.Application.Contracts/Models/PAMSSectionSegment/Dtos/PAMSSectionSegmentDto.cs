using System;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.PAMSSectionSegment.Dtos
{
    public class PAMSSectionSegmentDto : EntityDto<string>
    {
        public string NetworkId { get; set; }
        public byte District { get; set; }
        public byte CountyId { get; set; }
        public short Route { get; set; }
        public string Asset { get; set; }
        public bool Direction { get; set; }
        public short FromSection { get; set; }
        public short ToSection { get; set; }
        public int? Area { get; set; }
        public bool Interstate { get; set; }
        public byte? Lanes { get; set; }
        public byte? Width { get; set; }
        public int? Length { get; set; }
        public string SurfaceName { get; set; }
        public double? Risk { get; set; }
        public string PopulatedBy { get; set; }
        public DateTime? PopulatedAt { get; set; }
        public Guid? ImportSessionId { get; set; }
        public bool? IsMaster { get; set; }
    }
}
