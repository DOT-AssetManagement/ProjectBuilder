using ProjectBuilder.Core;

namespace ProjectBuilder.Web.MVC.Models
{
    public class PotentialBenefitsChartViewModel
    {
        public IEnumerable<ScenarioModel> Scenarios { get; set; } = Enumerable.Empty<ScenarioModel>();
        public int? ScenarioId { get; set; }
        public IEnumerable<int?> Districts { get; set; } = Enumerable.Empty<int?>();
        public int? SelectedDistrict { get; set; }
        public int SelectedNeedIndex { get; set; }
        public string Json { get; set; }
        public Dictionary<string, Dictionary<string, object>> SeriesPoint { get; set; } = new Dictionary<string, Dictionary<string, object>>();
        public List<int?> Labels { get; set; }
    }
}
