using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GisJsonHandler
{
    public class Scenario
    {
        public int ScenId;
        public string Name;
        public string LibraryName;
        public Guid LibraryId;
        public string LastRunBy;
        public DateTime? LastRunTime;
        public string Notes;
    };

    public class Project
    {
        [JsonProperty (PropertyName = "ProjId")]
        public int ProjectId;

        public string UserId;
        public string UserNotes;

        public long SchemaId;

        [JsonProperty (PropertyName = "ProjType")]
        public short ProjectType;

        public int? Year;

        [JsonProperty (PropertyName = "NBridges")]
        public int NumBridges;

        [JsonProperty (PropertyName = "NPave")]
        public int NumPaveSections;

        [JsonProperty (PropertyName = "Cost")]
        public double TotalCost;
    };

    public class Treatment
    {
        [JsonProperty(PropertyName = "ProjId")]
        public int ProjectId;

        [JsonProperty(PropertyName = "ProjType")]
        public short ProjectType;

        [JsonProperty(PropertyName = "TreatmentId")]
        public Guid ImportTimeGeneratedId;

        [JsonProperty(PropertyName = "TreatId")]
        public long TreatmentId;

        [JsonProperty(PropertyName = "Treatment")]
        public string AppliedTreatment;

        [JsonProperty(PropertyName = "TreatType")]
        public string TreatmentType;

        [JsonProperty(PropertyName = "Dist")]
        public short District;

        public short Cnty;
        
        [JsonProperty(PropertyName = "Rte")]
        public int Route;

        [JsonProperty(PropertyName = "Dir")]
        public short Direction;

        public int FromSection;
        public int ToSection;
        public string BRKEY;
        public long? BRIDGE_ID;

        [JsonProperty(PropertyName = "Owner")]
        public string OwnerCode;

        [JsonProperty(PropertyName = "COUNTY")]
        public string County;

        [JsonProperty(PropertyName = "MPO/RPO")]
        public string MPO;

        public int? Year;
        public double? Cost;
        public double? Benefit;

        public int? PreferredYear;
        public int? MinYear;
        public int? MaxYear;
        public int? PriorityOrder;
        public bool? IsCommitted;
        public double? Risk;
        public double? IndirectCostDesign;
        public double? IndirectCostOther;
        public double? IndirectCostROW;
        public double? IndirectCostUtilities;

        [JsonProperty(PropertyName = "B/C")]
        public double? BenefitCostRatio;

        public string MPMSID;
    };

    public class GisOutput
    {
        [JsonProperty (PropertyName = "Scenario")]
        public Scenario ScenHeader = new Scenario();
        public List<Project> Projects = new List<Project>();
        public List<Treatment> Treatments = new List<Treatment>();
    }
}
