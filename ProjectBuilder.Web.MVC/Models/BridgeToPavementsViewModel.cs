using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;

namespace ProjectBuilder.Web.MVC.Models
{
    public class BridgeToPavementsViewModel
    {
        public List<BridgeToPavementsModel> BridgeToPavements{ get; set; } = new List<BridgeToPavementsModel>();
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
