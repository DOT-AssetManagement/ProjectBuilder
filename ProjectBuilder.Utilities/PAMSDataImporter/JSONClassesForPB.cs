using System.Collections.Generic;
using Newtonsoft.Json;

/* Dmitry Gurenich, 07/15/2022
 * This file contains classes as we at SPP would like to see in JSON files produced by ARA
 * from PAMS for the Project Builder.
 * It inherits from the classes currently used for this purpose with some additions and exclusions
 * that are commented.
 */
 
namespace PAMSDataImporter
{
   
    /* 
     * This class does not currently exist, but it will be needed to get the bridge-to-pavement section
     * matching info that we currenly take from  the BridgeToPavement table of the iAMBridgeCare database
     */
    public class BridgeToPavement
    {
        [JsonProperty(PropertyName = "BrKey")]
        public string BrKey;

        [JsonProperty(PropertyName = "District")]
        public string District;

        [JsonIgnore]  // We do not need it for PB, but if it might be of use for other purposes
        [JsonProperty(PropertyName = "County")]
        public string County;

        [JsonProperty(PropertyName = "BridgeId")]
        [JsonIgnore]  // We do not need it for PB, but if it might be of use for other purposes
        public string BridgeId;

        [JsonProperty(PropertyName = "County Code")]
        public string Cnty;

        [JsonProperty(PropertyName = "Route")]
        public string Route;

        [JsonProperty(PropertyName = "Segment")]
        public string Segment;

        [JsonProperty(PropertyName = "Offset")]
        public string Offset;
    };

    /*
     * This class does not exist, but we'd like to be included with Applied Treatments even this treatment is "No Treatment"
     * Ideally these objects should carry the data that PAMS presents to the end-user in reports and.or views.
     */
    public class TreatmentConsequence
    {
        [JsonProperty(PropertyName = "ConsequenceName")]
        public string Name;

        [JsonProperty(PropertyName = "ConsequenceNumericValue")]
        public double NumValue;

        [JsonProperty(PropertyName = "ConsequenceTextValue")]
        public double TextValue;
    }

    /*
     * Below is the region with classes currently existing but not needed for PB
     * Class and variable names used here may be different from those in the ARA code, 
     * but JsonProperty names are the same.
     */
    #region These classes exist currently but are not expected to be used by PB

    public class TreatmentRejection
    {
        [JsonProperty(PropertyName = "TreatmentName")]
        public string TreatmentName;

        [JsonProperty(PropertyName = "TreatmentRejectionReason")]
        public TreatmentRejectionReason Reason;
    }

    public class CashFlowConsideration
    {
        [JsonProperty(PropertyName = "ReasonAgainstCashFlow")]
        public ReasonAgainstCashFlow Reason;

        [JsonProperty(PropertyName = "CashFlowRuleName")]
        public string CashFlowRuleName;
    }

    public class TreatmentConsideration
    {
        [JsonProperty(PropertyName = "BudgetPriorityLevel")]
        public int BudgetPriorityLevel;

        [JsonProperty(PropertyName = "BudgetUsages")]
        public List<BudgetUsage> BudgetUsages = new List<BudgetUsage>();

        [JsonProperty(PropertyName = "CashFlowConsiderations")]
        public List<CashFlowConsideration> CashFlowConsiderations;

        [JsonProperty(PropertyName = "TreatmentName")]
        public string TreatmentName;
    }

    public abstract class ConditionGoalDetail
    {
        [JsonProperty(PropertyName = "AttributeName")]
        public string AttributeName { get; set; }

        [JsonProperty(PropertyName = "GoalIsMet")]
        public bool GoalIsMet { get; set; }

        [JsonProperty(PropertyName = "GoalName")]
        public string GoalName { get; set; }
    }

    public class DeficientConditionGoal : ConditionGoalDetail
    {
        [JsonProperty(PropertyName = "ActualDeficientPercentage")]
        public double ActualDeficientPercentage;
        [JsonProperty(PropertyName = "AllowedDeficientPercentage")]
        public double AllowedDeficientPercentage;
        [JsonProperty(PropertyName = "DeficientLimit")]
        public double DeficientLimit;
    }

    public class TargetConditionGoal : ConditionGoalDetail
    {
        [JsonProperty(PropertyName = "ActualValue")]
        public double ActualValue { get; set; }

        [JsonProperty(PropertyName = "TargetValue")]
        public double TargetValue { get; set; }
    }

    public class TreatmentSchedulingCollision
    {
        [JsonProperty(PropertyName = "Year")]
        public int Year;

        [JsonProperty(PropertyName = "NameOfUnscheduledTreatment")]
        public string NameOfUnscheduledTreatment;
    }
    #endregion

    /*
     *  Below are the classes that exist currently. With some additions and (mostly) exclusions.
     *  Class and variable names used here may be different from those in the ARA code, 
     *  but JsonProperty names are the same.
     */

    public class Facility
    {
        [JsonProperty(PropertyName ="FacilityName")]
        public string Name;

        [JsonProperty(PropertyName = "SectionName")]
        public string Section;

        // At this point we would like to see in this section only the attributes with names limited to:
        // "WIDTH", "LANES", "SEGMENT_LENGTH" with possible additions in future
        [JsonProperty(PropertyName = "ValuePerNumericAttribute")]
        public Dictionary<string, double> NumericAttributes = new Dictionary<string, double>();

        // At this point we would like to see in this section only the attributes with names limited to:
        // "SR", "DISTRICT", "CNTY" with possible additions in future
        [JsonProperty(PropertyName = "ValuePerTextAttribute")]
        public Dictionary<string, string> TextAttributes = new Dictionary<string, string>();
    }

    public class Budget
    {
        [JsonProperty(PropertyName = "AvailableFunding")]
        public double Funding;

        [JsonProperty(PropertyName = "BudgetName")]
        public string Name;
    }

    public class BudgetUsage
    {
        [JsonProperty(PropertyName = "BudgetName")]
        public string Name;

        [JsonProperty(PropertyName = "CoveredCost")]
        public double Cost;

        [JsonProperty(PropertyName = "Status")]
        public BudgetUsageStatus Status;
    }

    public class TreatmentOption
    {
        [JsonProperty(PropertyName = "Benefit")]
        public double Benefit;

        [JsonProperty(PropertyName = "Cost")]
        public double Cost;

        [JsonProperty(PropertyName = "RemainingLife")]
        public double RemainingLife;

        [JsonProperty(PropertyName = "TreatmentName")]
        public string TreatmentName;
    }

 

    public class SectionTreatment
    {
        [JsonProperty(PropertyName = "AppliedTreatment")]
        public string AppliedTreatment;

        [JsonProperty(PropertyName = "TreatmentCause")]
        public TreatmentCause TreatmentCause;

        [JsonProperty(PropertyName = "TreatmentConsiderations")]
        //[JsonIgnore] // Not needed for PB
        public List<TreatmentConsideration> TreatmentConsiderations = new List<TreatmentConsideration>();

        [JsonProperty(PropertyName = "TreatmentFundingIgnoresSpendingLimit")]
        public bool TreatmentFundingIgnoresSpendingLimit;

        [JsonProperty(PropertyName = "TreatmentOptions")]
        public List<TreatmentOption> TreatmentOptions = new List<TreatmentOption>();

        [JsonProperty(PropertyName = "TreatmentRejections")]
        //[JsonIgnore] // Not needed for PB
        public List<TreatmentRejection> TreatmentRejections = new List<TreatmentRejection>();

        [JsonProperty(PropertyName = "TreatmentSchedulingCollisions")]
        //[JsonIgnore] // Not needed for PB
        public List<TreatmentSchedulingCollision> TreatmentSchedulingCollisions = new List<TreatmentSchedulingCollision>();

        [JsonProperty(PropertyName = "TreatmentsStatus")]
        public TreatmentStatus TreatmentStatus;

        [JsonProperty(PropertyName = "FacilityName")]
        public string Name;

        [JsonProperty(PropertyName = "SectionName")]
        public string Section;

        [JsonProperty(PropertyName = "ValuePerNumericAttribute")]
        //[JsonIgnore] // Not needed for PB
        public Dictionary<string, double> NumericAttributes = new Dictionary<string, double>();

        [JsonProperty(PropertyName = "ValuePerTextAttribute")]
        //[JsonIgnore] // Not needed for PB
        public Dictionary<string, string> TextAttributes = new Dictionary<string, string>();

        // This member does not exist we would like to be added for PB
        [JsonProperty("Consequences")]
        public List<TreatmentConsequence> Consequences = new List<TreatmentConsequence>();
    }
  
    public class YearOfPlanningHorizon
    {
        [JsonProperty(PropertyName = "Budgets")]
        public List<Budget> Budgets = new List<Budget>();

        [JsonProperty(PropertyName = "ConditionOfNetwork")]
        public double ConditionOfNetwork;

        [JsonProperty(PropertyName = "DeficientConditionGoals")]
        [JsonIgnore] // Not needed for PB
        public List<DeficientConditionGoal> DeficientConditionGoals = new List<DeficientConditionGoal>();

        [JsonProperty(PropertyName = "Sections")]
        public List<SectionTreatment> Sections = new List<SectionTreatment>();

        [JsonProperty(PropertyName = "TargetConditionGoals")]
        [JsonIgnore] // Not needed for PB
        public List<TargetConditionGoal> TargetConditionGoals = new List<TargetConditionGoal>();

        [JsonProperty(PropertyName = "Year")]
        public int Year;
    }

    public class PAMSJSONOutput
    {
        [JsonProperty(PropertyName = "InitialConditionOfNetwork")]
        public double InitialConditionOfNetwork;

        [JsonProperty(PropertyName = "InitialSectionSummaries")]
        public List<Facility> InitialSectionSummaries = new List<Facility>();

        [JsonProperty(PropertyName = "Years")]
        public List<YearOfPlanningHorizon> Years = new List<YearOfPlanningHorizon>();
    }

    public class JsonConfig
    {
        [JsonProperty(PropertyName = "SourceConnectionString")]
        public string SourceConnectionString;

        [JsonProperty(PropertyName = "TargetConnectionString")]
        public string TargetConnectionString;

        [JsonProperty(PropertyName = "SimulationId")]
        public string SimulationId;

        [JsonProperty(PropertyName = "AdditionalTableList")]
        public List<string> AdditionalTableList = new List<string>();

        [JsonProperty(PropertyName = "DoFullJsonImport")]
        public bool DoFullJsonImport = false;

        [JsonProperty(PropertyName = "DoYearByYear")]
        public bool DoYearByYear = false;
    }
}
