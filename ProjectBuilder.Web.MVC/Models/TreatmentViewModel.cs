using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ProjectBuilder.Web.MVC.Models
{
    public class TreatmentViewModel
    {
        [Required, BindProperty(Name = "assettype")]
        public string AssetType { get; set; }

        [Required,BindProperty(Name = "treatment")]
        public string Treatment { get; set; }
        [Required, BindProperty(Name = "iscommitted")]
        public bool IsCommitted { get; set; }
        [Required, BindProperty(Name = "district")]
        public byte? District { get; set; }
        [Required, BindProperty(Name = "preferredyear")]
        public int? PreferredYear { get; set; }
        [Required, BindProperty(Name = "pariorityorder")]
        public byte? PriorityOrder { get; set; }
        [Required, BindProperty(Name = "county")]
        public byte? County { get; set; }
        [Required, BindProperty(Name = "minyear")]
        public int? MinYear { get; set; }
        [Required, BindProperty(Name = "maxyear")]
        public int? MaxYear { get; set; }

        [Required, BindProperty(Name = "cost")]
        public double? Cost { get; set; }
        [Required, BindProperty(Name = "route")]
        public int? Route { get; set; }
        [Required, BindProperty(Name = "benefit")]
        public double? Benefit { get; set; }
        [Required, BindProperty(Name = "section")]
        public string Section { get; set; }
        [Required, BindProperty(Name = "treatmenttype")]
        public int? TreatmentType { get; set; }
        [Required, BindProperty(Name = "risk")]
        public double? Risk { get; set; }
        [Required, BindProperty(Name = "interstate")]
        public bool Interstate { get; set; }
        [Required, BindProperty(Name = "direction")]
        public bool Direction { get; set; }
        [Required, BindProperty(Name = "indirectcostdesign")]
        public double? IndirectCostDesign { get; set; }
        [Required, BindProperty(Name = "indirectcostrow")]
        public double? IndirectCostRow { get; set; }
        [Required, BindProperty(Name = "indirectcostutilities")]
        public double? IndirectCostUtilities { get; set; }
        [Required, BindProperty(Name = "indirectcostothers")]
        public double? IndirectCostOthers { get; set; }
        [Required, BindProperty(Name = "treatmenttype")]
        public int? UserTreatmentTypeNo { get; set; }
        [Required, BindProperty(Name = "bridgeid")]
        public long? BridgeId { get; set; }
        [Required, BindProperty(Name = "brkey")]
        public string Brkey { get; set; }
        public int? Year { get; set; }
        [BindProperty(Name = "treatmentid")]
        public Guid TreatmentId { get; set; }
        public string ProjectId { get; set; }
        [BindProperty(Name = "filterdirection")]
        public bool? FilterDirection { get; set; }
        [BindProperty(Name = "projecttreatmentid")]
        public long? ProjectTreatmentId { get; set; }
        [BindProperty(Name = "scenarioid")]
        public int? ScenarioId { get; set; }
    }
}
