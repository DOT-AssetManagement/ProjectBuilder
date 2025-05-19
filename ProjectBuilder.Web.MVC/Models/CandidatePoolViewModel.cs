using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Runtime.InteropServices;

namespace ProjectBuilder.Web.MVC.Models
{
    public class CandidatePoolViewModel
    {
        public IEnumerable<CandidatePoolModel> Libraries { get; set; } = Enumerable.Empty<CandidatePoolModel>();

        [Required,BindProperty(Name= "libraryname")]
        public string Name { get; set; }
        [Required,BindProperty(Name= "librarydescription")]
        public string Description { get; set; }
        [BindProperty(Name="libraryId")]
        public Guid? Id { get; set; }

        [DisplayName("Number of Treatments")]
        public int TreatmentsNumber { get; set; }
        [BindProperty(Name= "isshared")]
        public bool IsShared { get; set; }
        [BindProperty(Name="isemptylibrary")]
        public bool IsEmptyLibrary { get; set; } = false;
        [BindProperty(Name = "assettype"),RegularExpression("^(?i)(Bridge|Pavement)$"),DefaultValue("")]
        public string? SelectedAsset { get; set; }
        [BindProperty(Name = "districts")]
        public int? SelectedDistrict { get; set; }
        [BindProperty(Name = "counties")]
        public short? SelectedCounty { get; set; }
        [BindProperty(Name = "routes")]
        public int? SelectedRoute { get; set; }
        [BindProperty(Name = "minyear")]
        public int? MinYear { get; set; }
        [BindProperty(Name = "maxyear")]
        public int? MaxYear { get; set; }
        public bool IsImporting { get; set; } = false;
        public bool FromScenario { get; set; } = false;
        public bool CreatScenpage { get; set; }
        public string SearchString { get; set; }

    }

    public class ImportMASAndB2PViewModel
    {
        [Required, BindProperty(Name = "excelfile")]
        public IFormFile ExcelFile { get; set; }
        [BindProperty(Name = "ismas")]
        public bool IsMAS { get; set; }
     
        [BindProperty(Name = "tabname")]
        public string? TabName { get; set; }
    }
    public class ImportTreatmentViewModel
    {
        [Required(ErrorMessage ="you need to select an excel file in order to import treatments"),BindProperty(Name = "excelfile")]
        public IFormFile? ExcelFile { get; set; }
        [Required(ErrorMessage ="please select an Import What option"),BindProperty(Name = "importtype")]
        public string ImportType { get; set; }
        [Required(ErrorMessage ="please select a candidate pool in order to import treatments"),BindProperty(Name = "candidatepoolid")]
        public Guid? LibraryId { get; set; }
        [BindProperty(Name = "tabname")]
        public string? TabName { get; set; }
        public bool fromScratch { get; set; } 
        public bool keepAll { get; set; }
    }
    public class DashboardViewModel
    {
        public long ScenariosCount { get; set; }
        public long BridgesCount { get; set; }
        public long PavementsCount { get; set; }
        public long ProjectsCount { get; set; }
    }
}
