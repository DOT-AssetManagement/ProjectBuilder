using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.OutPut.Dtos
{
    public class MapsDto
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
