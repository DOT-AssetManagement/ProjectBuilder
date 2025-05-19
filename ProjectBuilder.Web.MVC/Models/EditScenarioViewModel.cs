using ProjectBuilder.Core;

namespace ProjectBuilder.Web.MVC.Models
{
    public class ScenarioParametersViewModel
    {
        public IEnumerable<ScenarioParameterModel> ScenarioParameters { get; set; }
        public string ScenarioName { get; set; }
        public int? ScenarioId { get; set; }
    }
    public class ScenarioBudgetConstraintsViewModel
    {
        public IEnumerable<ScenarioBudgetFlatModel> BudgetConstraints { get; set; }
        public string SelectedScenario { get; set; }
        public int? ScenarioId { get; set; }
    }
}
