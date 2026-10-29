using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.CreateBridgeToPavements.Dtos
{
    public class CreateBridgeToPavementDto
    {
        public string Id { get; set; } 
        public int District { get; set; }
        public string County { get; set; }
        public long BridgeId { get; set; }
        public int CountyCode { get; set; }
        public int Route { get; set; }
        public int Segment { get; set; }
        public int OffSet { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
