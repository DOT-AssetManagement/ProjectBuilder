using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.PAMSSectionSegment.Dtos
{
    public class UpdatePAMSSectionSegmentDto : EntityDto<string>
    {
        public byte District { get; set; }
        public byte CountyId { get; set; }
        public short Route { get; set; }
        public short FromSection { get; set; }
        public short ToSection { get; set; }
        public bool Direction { get; set; }
        public bool Interstate { get; set; }
    }
}
