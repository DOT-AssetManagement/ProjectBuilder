using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
   
    public class Project : IEntity<int>
    {
        [Column("ProjectId")]
        public int EntityId { get; set; }
        [Column("ScenId")]
        public int ScenarioId { get; set; }
        public string AssetType { get; set; } = null!;
        public byte? District { get; set; }
        public byte? CountyId { get; set; }
        public string County { get; set; }
        public int? Route { get; set; }
        public byte? Direction { get; set; }
        public int? PreferredStartingYear { get; set; }
        [Column("TotalCostM")]
        public double? TotalCost { get; set; }
        [Column("BenefitM")]
        public double? Benefit { get; set; }
        [Column("NoTreatments")]
        public int? NumberOfTreatments { get; set; }
        public string Section { get; set; }
        public string CommitmentStatus { get; set; }
        public string Selected { get; set; } = null!;
        public int? SelectedFirstYear { get; set; }
        public string UserCreated { get; set; } = null!;
        public string UserId { get; set; }
        public string UserNotes { get; set; }
    }
}
