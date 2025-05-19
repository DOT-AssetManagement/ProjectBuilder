using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjectBuilder.Web.MVC.Models
{
    public class ProjectsViewModel
    {
        public string ScenarioName { get; set; }
        public IEnumerable<int?> Years { get; set; } = Enumerable.Empty<int?>();
        [Required, BindProperty(Name = "scenarioId")]
        public int? ScenarioId { get; set; }

        public string IsExporting { get; set; }

    }
    public class ProjectTreatmentsViewModel
    {
        public string ScenarioName { get; set; }
        public IEnumerable<int?> Years { get; set; } = Enumerable.Empty<int?>();
        [Required, BindProperty(Name = "scenarioId")]
        public int? ScenarioId { get; set; }
        public int? ProjectId { get; set; }
    }

    public class ExportProjectViewModel
    {
        

        [Required, BindProperty(Name = "scenarioId")]
        public int ScenarioId { get; set; }

        [Required(ErrorMessage = "please select an Export What option"), BindProperty(Name = "exporttype")]
        public string ExportType { get; set; }
    }
}
