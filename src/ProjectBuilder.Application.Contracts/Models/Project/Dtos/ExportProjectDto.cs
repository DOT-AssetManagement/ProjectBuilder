using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.Project.Dtos
{
    public class ExportProjectDto
    {
        [Required, BindProperty(Name = "scenarioId")]
        public int ScenarioId { get; set; }

        [Required(ErrorMessage = "please select an Export What option"), BindProperty(Name = "exporttype")]
        public string ExportType { get; set; }

        // Comma-separated project IDs currently passing the Projects grid's active filter.
        // Empty/null means no filter is applied, so the export is left unrestricted.
        public string ProjectIdsCsv { get; set; }

        // Mirrors the grid's Funded/UnFunded dropdown, so the export covers the side being viewed.
        public bool Unfunded { get; set; }
    }
}
