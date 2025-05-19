using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProjectBuilder.Core;

namespace ProjectBuilder.Web.MVC.Models
{
    public class RunScenarioViewModel
    {
        [BindProperty(Name ="SelectedScenario")]
        public int? SelectedScenario { get; set; }
        [BindProperty(Name = "paramid")]
        public string ParamId { get; set; }
        public double? ParamaterValue { get; set; }
        public IEnumerable<ScenarioModel> Scenarios { get; set; } 
        public IEnumerable<ScenarioParameterModel> ScenarioParameters { get; set; } = Enumerable.Empty<ScenarioParameterModel>();
        public IEnumerable<ScenarioBudgetFlatModel> ScenarioBudgets { get; set; } = Enumerable.Empty<ScenarioBudgetFlatModel>();
    }
}
