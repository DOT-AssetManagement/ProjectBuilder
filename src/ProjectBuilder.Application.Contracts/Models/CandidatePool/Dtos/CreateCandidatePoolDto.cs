using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.CandidatePool.Dtos
{
    public class CreateCandidatePoolDto
    {
        public int CandidatePoolNumber { get; set; }
        public Guid UserId { get; set; }
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public bool? IsActive { get; set; }
        public bool IsShared { get; set; }
        [BindProperty(Name = "assettype"), RegularExpression("^(?i)(Bridge|Pavement)$"), DefaultValue("")]
        public string? SelectedAsset { get; set; }
        [BindProperty(Name = "counties")]
        public short? SelectedCounty { get; set; }
        [BindProperty(Name = "routes")]
        public int? SelectedRoute { get; set; }
        [BindProperty(Name = "minyear")]
        public int? MinYear { get; set; }
        [BindProperty(Name = "maxyear")]
        public int? MaxYear { get; set; }
        [BindProperty(Name = "isemptylibrary")]
        public bool IsEmptyLibrary { get; set; } = false;
        [BindProperty(Name = "districts")]
        public int? SelectedDistrict { get; set; }
        public bool CreatScenpage { get; set; }
    }
}