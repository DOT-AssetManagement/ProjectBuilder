using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.Project.Dtos
{
    public class ProjectDto : EntityDto<int>
    {
        public int SequenceNo { get; set; }
        public int ScenarioId { get; set; }
        public string AssetType { get; set; }
        public byte? District { get; set; }
        public byte? CountyId { get; set; }
        public string County { get; set; }
        public int? Route { get; set; }
        public byte? Direction { get; set; }
        public int? PreferredStartingYear { get; set; }
        public double? TotalCost { get; set; }
        public double? Benefit { get; set; }
        public int? NumberOfTreatments { get; set; }
        public string Section { get; set; }
        public string CommitmentStatus { get; set; }
        public string Selected { get; set; }
        public int? SelectedFirstYear { get; set; }
        public string UserCreated { get; set; }
    }
}
