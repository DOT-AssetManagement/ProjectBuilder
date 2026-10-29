namespace ProjectBuilder.Permissions;

public static class DSSPermissions
{
    public const string GroupName = "DSS";

    public static class SetUp
    {
        public const string Default = GroupName + ".SetUp";
    }

    public static class AssetTypeParameterPermissions
    {
        public const string Default = "dss.AssetTypeParameter";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class OtherParameterPermissions
    {
        public const string Default = "dss.OtherParameter";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class MeasurePermissions
    {
        public const string Default = "dss.Measure";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class ImportPermission
    {
        public const string Default = "dss.Import";
        public const string Import = Default + ".Import";
    }

    public static class TopBarPermissions
    {
        public const string Default = "dss.Topbar";
        public const string Outputs = Default + ".Outputs";
        public const string Dashboard = Default + ".Dashboard";
    }
}

public static class SupplementalCandidatesPermissions
{
    public const string GroupName = "SupplementalCandidates";

    public static class PoolsPageActions
    {
        public const string Default = "dss.SupplementalCandidates";
        public const string Create = Default + ".Create";
        public const string Clear = Default + ".Clear";
        public const string ImportWorkCandidates = Default + ".ImportWorkCandidates";
        public const string ImportConfiguration = Default + ".ImportConfiguration";
        public const string Run = Default + ".Run";
        public const string OutPut = Default + ".Outputs";
        public const string Projects = Default + ".Projects";
        public const string Map = Default + ".Map";
        public const string ExportProjectsOrCandidate = Default + ".ExportProjectsOrCandidates";
        public const string Edit = Default + ".Edit";

        public static class WorkCandidates
        {
            public const string Default = PoolsPageActions.Default + ".WorkCandidates";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        public static class Configuration
        {
            public const string Default = PoolsPageActions.Default + ".Configuration";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
    }
}

public static class ConfigurationPermissions
{
    public static class ConfigurationPageActions
    {
        public const string Default = "dss.Configuration";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Import = Default + ".Import";
    }
}

public static class ChartsPermissions
{
    public static class Charts
    {
        public const string Default = "dss.Charts";
        public const string ProjectsAndGoalsFlowDiagram = Default + ".ProjectsAndGoalsFlowDiagram";
        public const string ProjectEfficiencyAndCost = Default + ".ProjectEfficiencyAndCost";
        public const string ProjectsAndGoalsPercentageContribution = Default + ".ProjectsAndGoalsPercentageContribution";
        public const string ProjectsAndGoalsActualContribution = Default + ".ProjectsAndGoalsActualContribution";
        public const string ProjectToScoreArcMapping = Default + ".ProjectToScoreArcMapping";
        public const string BarCharts = Default + ".BarCharts";
    }
}

public static class ReportsPermissions
{
    public static class Reports
    {
        public const string Default = "dss.Reports";
        public const string Projects = Default + ".Projects";
        public const string ProjectsEdit = Projects + ".Edit";
        public const string ProjectsExport = Projects + ".Export";
        public const string WorkCandidateSummary = Default + ".WorkCandidateSummary";
        public const string WorkCandidateSummaryExport = WorkCandidateSummary + ".Export";
        public const string WorkCandidateDetail = Default + ".WorkCandidateDetail";
        public const string WorkCandidateDetailExport = WorkCandidateDetail + ".Export";
    }
}

public static class ScenarioPermissions
{
    public const string GroupName = "Scenarios";
    public const string Default = "dss.Scenarios";
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string Run = Default + ".Run";

    public static class Analysis
    {
        public const string Default = ScenarioPermissions.Default + ".Analysis";
        public const string Projects = Default + ".Projects";
        public const string SummaryReport = Default + ".SummaryReport";
        public const string Maps = Default + ".Maps";

        public static class ProjectTreatments
        {
            public const string Default = Analysis.Default + ".ProjectTreatments";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
    }
}

public static class TreatmentPoolLibraryPermissions
{
    public const string GroupName = "TreatmentPoolLibraries";
    public const string Default = "dss.TreatmentPoolLibraries";
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string Import = Default + ".Import";
    public const string Copy = Default + ".Copy";

    public static class Treatments
    {
        public const string Default = TreatmentPoolLibraryPermissions.Default + ".Treatments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}

public static class AdministrationPermissions
{
    public const string Default = "dss.Administration";
}

public static class BackgroundJobsPermissions
{
    public const string Default = "dss.BackgroundJobs";
}

public static class SetUpLookupsPermissions
{
    public const string Default = "dss.SetUp.Lookups";
    public const string AraEnumerations = Default + ".AraEnumerations";
    public const string BridgeToPavements = Default + ".BridgeToPavements";
    public const string MaintainableAssetsSegmentation = Default + ".MaintainableAssetsSegmentation";
    public const string BridgeReferenceData = Default + ".BridgeReferenceData";
    public const string Counties = Default + ".Counties";
    public const string DefaultSlack = Default + ".DefaultSlack";
    public const string ImportSessions = Default + ".ImportSessions";
    public const string TreatmentCancellationMatrix = Default + ".TreatmentCancellationMatrix";
    public const string TreatmentParameters = Default + ".TreatmentParameters";
    public const string TreatmentTypes = Default + ".TreatmentTypes";
    public const string UserTreatments = Default + ".UserTreatments";
    public const string Parameters = Default + ".Parameters";
}

public static class ProjectBuilderPermissions
{
    public const string GroupName = "ProjectBuilder";

    public static class Books
    {
        public const string Default = GroupName + ".Books";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}
