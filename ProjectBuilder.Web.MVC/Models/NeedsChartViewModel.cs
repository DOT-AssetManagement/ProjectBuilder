using ProjectBuilder.Core;
using System.ComponentModel;

namespace ProjectBuilder.Web.MVC.Models
{
    public class NeedsChartViewModel
    {
        public IEnumerable<int?> Districts { get; set; } = Enumerable.Empty<int?>();
        public int? ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public int? SelectedDistrict { get; set; }
        public int SelectedNeedIndex { get; set; }
        public string Json { get; set; }
        public Dictionary<string, Dictionary<string, object>> SeriesPoint { get; set; } = new Dictionary<string, Dictionary<string, object>>();
        public List<int?> Labels { get; set; }
    }
    public class ChartsViewModel
    {
        public int? ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public IEnumerable<int?> Districts { get; set; } = Enumerable.Empty<int?>();
    }
    public class ReportsViewModel<T> 
    {
        public int? ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public int? SelectedDistrict { get; set; }
        public int? SelectedProjectType { get; set; }
        public IEnumerable<int?> Districts { get; set; } = Enumerable.Empty<int?>();
        public IEnumerable<T> ReportsData { get; set; } = Enumerable.Empty<T>();
    }
}
