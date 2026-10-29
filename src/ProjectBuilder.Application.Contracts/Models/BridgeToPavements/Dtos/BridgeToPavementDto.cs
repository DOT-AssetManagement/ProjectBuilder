using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.CreateBridgeToPavements.Dtos
{
    public class BridgeToPavementDto : EntityDto<string>
    {
        public int District { get; set; }
        public string County { get; set; }
        public long BridgeId { get; set; }
        public int CountyCode { get; set; }
        public int Route { get; set; }
        public int Segment { get; set; }
        public int OffSet { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
