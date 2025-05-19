using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;

namespace ProjectBuilder.Web.MVC.Models
{
    public class AraEnumerationViewModel
    {
        public List<AraEnumerationsModel> ARAEnumerationsList { get; set; } = new List<AraEnumerationsModel>();

        public string EnumFamily { get; set; }
        public string EnumName { get; set; }
        public int?  EnumInt { get; set; }
    }
}
