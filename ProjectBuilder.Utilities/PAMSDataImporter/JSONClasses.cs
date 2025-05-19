
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PAMSDataImporter
{
    public class Facility
    {
        [JsonProperty(PropertyName ="FacilityName")]
        public string Name;

        [JsonProperty(PropertyName = "SectionName")]
        public string Section;

        [JsonProperty(PropertyName = "ValuePerNumericAttribute")]
        public Dictionary<string, double> NumericAttributes = new Dictionary<string, double>();

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
        public int Status;
    }

    public class TreatmentRejection
    {
        [JsonProperty(PropertyName = "TreatmentName")]
        public string TreatmentName;

        [JsonProperty(PropertyName = "TreatmentRejectionReason")]
        public int Reason;
    }

    public class CashFlowConsideration
    {
        [JsonProperty (PropertyName = "ReasonAgainstCashFlow")]
        public ReasonAgainstCashFlow Reason;

        [JsonProperty(PropertyName = "CashFlowRuleName")]
        public string CashFlowRuleName;
    }

    public class TreatmentConsideration
    {
        [JsonProperty (PropertyName = "BudgetPriorityLevel")]
        public int BudgetPriorityLevel;

        [JsonProperty(PropertyName = "BudgetUsages")]
        public List<BudgetUsage> BudgetUsages = new List<BudgetUsage>();

        [JsonProperty(PropertyName = "CashFlowConsiderations")]
        public List<CashFlowConsideration> CashFlowConsiderations;

        [JsonProperty(PropertyName = "TreatmentName")]
        public string TreatmentName;
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

    public class TreatmentSchedulingCollision
    {
        [JsonProperty (PropertyName = "Year")]
        public int Year;

        [JsonProperty(PropertyName = "NameOfUnscheduledTreatment")]
        public string NameOfUnscheduledTreatment;
    }

    public class SectionTreatment
    {
        [JsonProperty(PropertyName = "AppliedTreatment")]
        public string AppliedTreatment;

        [JsonProperty(PropertyName = "TreatmentCause")]
        public int TreatmentCause;

        [JsonProperty(PropertyName = "TreatmentConsiderations")]
        [JsonIgnore]
        public List<TreatmentConsideration> TreatmentConsiderations = new List<TreatmentConsideration>();

        [JsonProperty(PropertyName = "TreatmentFundingIgnoresSpendingLimit")]
        public bool TreatmentFundingIgnoresSpendingLimit;

        [JsonProperty(PropertyName = "TreatmentOptions")]
        public List<TreatmentOption> TreatmentOptions = new List<TreatmentOption>();

        [JsonProperty(PropertyName = "TreatmentRejections")]
        [JsonIgnore]
        public List<TreatmentRejection> TreatmentRejections = new List<TreatmentRejection>();

        [JsonProperty(PropertyName = "TreatmentSchedulingCollisions")]
        [JsonIgnore]
        public List<TreatmentSchedulingCollision> TreatmentSchedulingCollisions = new List<TreatmentSchedulingCollision>();

        [JsonProperty(PropertyName = "TreatmentsStatus")]
        public int TreatmentStatus;

        [JsonProperty(PropertyName = "FacilityName")]
        public string Name;

        [JsonProperty(PropertyName = "SectionName")]
        public string Section;

        [JsonProperty(PropertyName = "ValuePerNumericAttribute")]
        [JsonIgnore]
        public Dictionary<string, double> NumericAttributes = new Dictionary<string, double>();

        [JsonProperty(PropertyName = "ValuePerTextAttribute")]
        [JsonIgnore]
        public Dictionary<string, string> TextAttributes = new Dictionary<string, string>();
    }


    public abstract class ConditionGoalDetail
    {
        [JsonProperty (PropertyName = "AttributeName")]
        public string AttributeName { get; set; }

        [JsonProperty(PropertyName = "GoalIsMet")]
        public bool GoalIsMet { get; set; }

        [JsonProperty(PropertyName = "GoalName")]
        public string GoalName { get; set; }
    }

    public class DeficientConditionGoal: ConditionGoalDetail
    {
        [JsonProperty (PropertyName = "ActualDeficientPercentage")]
        public double ActualDeficientPercentage;
        [JsonProperty(PropertyName = "AllowedDeficientPercentage")]
        public double AllowedDeficientPercentage;
        [JsonProperty(PropertyName = "DeficientLimit")]
        public double DeficientLimit;
    }

    public class TargetConditionGoal: ConditionGoalDetail
    {
        [JsonProperty (PropertyName = "ActualValue")]
        public double ActualValue { get; set; }

        [JsonProperty(PropertyName = "TargetValue")]
        public double TargetValue { get; set; }
    }

    public class YearOfPlanningHorizon
    {
        [JsonProperty(PropertyName = "Budgets")]
        public List<Budget> Budgets = new List<Budget>();

        [JsonProperty(PropertyName = "ConditionOfNetwork")]
        public double ConditionOfNetwork;

        [JsonProperty(PropertyName = "DeficientConditionGoals")]
        [JsonIgnore]
        public List<DeficientConditionGoal> DeficientConditionGoals = new List<DeficientConditionGoal>();

        [JsonProperty(PropertyName = "Sections")]
        public List<SectionTreatment> Sections = new List<SectionTreatment>();

        [JsonProperty(PropertyName = "TargetConditionGoals")]
        [JsonIgnore]
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
}
