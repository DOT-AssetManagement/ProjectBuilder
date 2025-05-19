using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public class CandidatePoolModel
    {
        public bool HasScenarioAttached;

        public Guid CandidatePoolId { get; set; }
        public int CandidatePoolNumber { get; set; }
        public long UserId { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        public bool IsShared { get; set; }
        public DateTime? PopulatedAt { get; set; }
        public string Source { get; set; }
        public int TreatmentsCount { get; set; }
        public int BridgeTreatmentsCount { get; set; }
        public int PavementTreatmentsCount { get; set; }
        public int ScenarioCount { get; set; }
        public string SceName { get; set; }
        public bool IsDeleteable { get; set; }
        public string FormattedPopulatedAt
        {
            get
            {
                return PopulatedAt.HasValue ? PopulatedAt.Value.ToString("yyyy-MM-dd") : string.Empty;
            }
        }
    }
    
}
