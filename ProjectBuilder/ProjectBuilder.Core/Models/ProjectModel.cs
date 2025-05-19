using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    [ExcludeFromCodeCoverage]
    public class ProjectModel
    {
        public int ProjectId { get; set; }
        public int ScenarioId { get; set; }
        public string AssetType { get; set; }
        public byte? District { get; set; }
        public string County { get; set; }
        public byte? CountyId { get; set; }
        public int? Route { get; set; }
        public byte? Direction { get; set; }  
        public string Section { get; set; }   
        public int? NumberOfTreatments { get; set; }    
        public double? TotalCost { get; set; }
        public double? Benefit { get; set; }
        public string UserCreated { get; set; } = null!;
        public string CommitmentStatus { get; set; }
        public int? PreferredStartingYear { get; set; }
        public string Selected { get; set; } = null!;
        public int? SelectedFirstYear { get; set; }
        public string UserId { get; set; }
        public string UserNotes { get; set; }
    }
}
