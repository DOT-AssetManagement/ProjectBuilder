using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSS.Utilities
{
    public class DssScenario
    {
        [JsonProperty(Order = 10)]
        public int? ScenId;

        [JsonProperty(Order = 20)]
        public string? Name;

        /*
        [JsonProperty(Order = 30)]
        public string? Description;
        */

        [JsonProperty(Order = 40, PropertyName="LibraryName")]
        public string? PoolName;

        [JsonProperty(Order = 50, PropertyName ="LastRunBy")]
        public string? User;

        [JsonProperty(Order = 60, PropertyName ="LastRunTime")]
        public DateTime? LastRun;

        [JsonProperty(Order = 70)]
        public double? EfficiencyThreshold;

        [JsonProperty(Order = 80)]
        public string Notes ="Completed";
    }

    public class DssProject
    {
        [JsonProperty(Order =10, PropertyName ="ProjId")]
        public int? Id;

        [JsonProperty(Order =20)]
        public long? SchemaId = null;

        [JsonProperty(Order =30, PropertyName = "ProjType")]
        public short ProjectType;

        [JsonProperty(Order = 40)]
        public int? Year;

        [JsonProperty(Order = 50, PropertyName = "NBridges")]
        public int? NumBridge;

        [JsonProperty(Order =60, PropertyName ="NPave")]
        public int? NumPave;

        [JsonProperty(Order =70, PropertyName ="Cost")]
        public double? Cost;

        /*
        public string? Partner;
        public string? Description;
        public int? District;
        public int? Cnty;
        public string? County;
     
        public string? Treatment;
        public int? MinYear;
        public int? MaxYear;
     
        public double? Benefit;
        public double? Efficiency;
        */
    }

    public class DssWorkCandidate
    {
        [JsonProperty(PropertyName = "ProjId")]
        public int? ProjectId;

        [JsonProperty(PropertyName = "ProjType")]
        public short ProjectType=0;

        [JsonProperty(PropertyName = "TreatId")]
        public int? TreatmentId;

        [JsonProperty(PropertyName = "AssetType")]
        public string? AssetType;

        [JsonProperty(PropertyName = "Treatment")]
        public string? TreatmentName;

        [JsonProperty(PropertyName = "TreatType")]
        public string? TreatmentType;

        [JsonProperty(PropertyName = "Dist")]
        public int? District;

        public int? Cnty;

        [JsonProperty(PropertyName = "Rte")]
        public int? Route;

        [JsonProperty(PropertyName = "Dir")]
        public int? Direction;

        public int? FromSection = null;
        public int? ToSection = null;
        public string? BRKEY = null;
        public long? BRIDGE_ID = null;

        [JsonProperty(PropertyName = "Owner")]
        public string? OwnerCode=null;
        
        //public string? CRS=null;
        
        [JsonProperty(PropertyName = "COUNTY")]
        public string? County = null;

        [JsonProperty(PropertyName = "MPO/RPO")]
        public string? Partner;

        public int? Year;
        public double? Cost;
        public double? Benefit;

        [JsonProperty(PropertyName = "B/C")]
        public double? BCRatio;
    };

    public class DssGisOutput
    {
        [JsonProperty(PropertyName = "Scenario")]
        public DssScenario Scenario = new DssScenario();
        public List<DssProject> Projects = new List<DssProject>();
        [JsonProperty(PropertyName ="Treatments")]
        public List<DssWorkCandidate> WorkCandidates = new List<DssWorkCandidate>();
    }

    
}
