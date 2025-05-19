using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Models
{
    public class BridgeToPavementsModel
    {
        public string BrKey { get; set; }
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
