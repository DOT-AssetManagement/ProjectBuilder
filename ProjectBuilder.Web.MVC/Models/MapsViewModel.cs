using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ProjectBuilder.Web.MVC.Models
{
    public class MapsViewModel
    {

        [Required, BindProperty(Name = "scenario")]
        public int? SelectedScenario { get; set; }
        [Required, BindProperty(Name = "district")]
        public byte? SelectedDistrict { get; set; }
        [Required, BindProperty(Name = "county")]
        public byte? SeletedCounty { get; set; }
        [Required, BindProperty(Name = "route")]
        public int? SelectedRoute { get; set; }
        [Required, BindProperty(Name = "treatmenttype")]
        public string TreatementType { get; set; }
    }
}
