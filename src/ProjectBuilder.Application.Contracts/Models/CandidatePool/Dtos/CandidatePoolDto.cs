using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.CandidatePool.Dtos
{
    public class CandidatePoolDto : EntityDto<Guid>
    {
        public Guid Id { get; set; }
        public int CandidatePoolNumber { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        public bool IsShared { get; set; }
        public Guid? SourcePoolId { get; set; }
        public string Owner { get; set; }
        public int TreatmentsCount { get; set; }
        public int BridgeTreatmentsCount { get; set; }
        public int PavementTreatmentsCount { get; set; }
        public int ScenarioCount { get; set; }
        public DateTime? PopulatedAt { get; set; }
        public string Source { get; set; }
        public string SearchString { get; set; }
        public bool IsDeleteable { get; set; }
        public bool IsImporting { get; set; } = false;
        public bool FromScenario { get; set; } = false;
        public string SceName { get; set; }
        public int? ScenarioId { get; set; }
        public bool? ScenarioLocked { get; set; }
        [BindProperty(Name = "assettype"), RegularExpression("^(?i)(Bridge|Pavement)$"), DefaultValue("")]
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
        [DisplayName("Number of Treatments")]
        public int TreatmentsNumber { get; set; }

        // Ensuring proper type consistency
        public IEnumerable<CandidatePoolDto> Libraries { get; set; } = Enumerable.Empty<CandidatePoolDto>();
        //public IEnumerable<CandidatePoolDto> Libraries { get; set; } = new List<CandidatePoolDto>();
        public string FormattedPopulatedAt
        {
            get
            {
                return PopulatedAt.HasValue ? PopulatedAt.Value.ToString("yyyy-MM-dd") : string.Empty;
            }
        }
    }
    public class ImportTreatmentViewModel
    {
        [Required(ErrorMessage = "you need to select an excel file in order to import treatments"), BindProperty(Name = "excelfile")]
        public IFormFile? ExcelFile { get; set; }
        [Required(ErrorMessage = "please select an Import What option"), BindProperty(Name = "importtype")]
        [RegularExpression("^[BPD]$", ErrorMessage = "Import What must be BAMS, PAMS or DecisionSpace")]
        public string ImportType { get; set; }
        [Required(ErrorMessage = "please select a candidate pool in order to import treatments"), BindProperty(Name = "candidatepoolid")]
        public Guid? LibraryId { get; set; }
        [BindProperty(Name = "tabname")]
        public string? TabName { get; set; }
        public bool fromScratch { get; set; }
        public bool keepAll { get; set; }
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
}
