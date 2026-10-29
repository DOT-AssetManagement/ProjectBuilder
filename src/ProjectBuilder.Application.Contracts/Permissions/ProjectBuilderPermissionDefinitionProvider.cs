using ProjectBuilder.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace ProjectBuilder.Permissions;

public class ProjectBuilderPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var dssGroup = context.AddGroup(DSSPermissions.GroupName, L("Admin"));

        // SetUp / Administration
        var setUp = dssGroup.AddPermission(DSSPermissions.SetUp.Default, L("Set Up"));

        var assetTypeParam = setUp.AddChild(DSSPermissions.AssetTypeParameterPermissions.Default, L("Asset Type Parameter"));
        assetTypeParam.AddChild(DSSPermissions.AssetTypeParameterPermissions.Create, L("Create"));
        assetTypeParam.AddChild(DSSPermissions.AssetTypeParameterPermissions.Edit, L("Edit"));
        assetTypeParam.AddChild(DSSPermissions.AssetTypeParameterPermissions.Delete, L("Delete"));

        var otherParam = setUp.AddChild(DSSPermissions.OtherParameterPermissions.Default, L("Other Parameter"));
        otherParam.AddChild(DSSPermissions.OtherParameterPermissions.Create, L("Create"));
        otherParam.AddChild(DSSPermissions.OtherParameterPermissions.Edit, L("Edit"));
        otherParam.AddChild(DSSPermissions.OtherParameterPermissions.Delete, L("Delete"));

        var measure = setUp.AddChild(DSSPermissions.MeasurePermissions.Default, L("Measure"));
        measure.AddChild(DSSPermissions.MeasurePermissions.Create, L("Create"));
        measure.AddChild(DSSPermissions.MeasurePermissions.Edit, L("Edit"));
        measure.AddChild(DSSPermissions.MeasurePermissions.Delete, L("Delete"));

        var lookups = setUp.AddChild(SetUpLookupsPermissions.Default, L("Look Up Data"));
        lookups.AddChild(SetUpLookupsPermissions.AraEnumerations, L("Enumerations"));
        lookups.AddChild(SetUpLookupsPermissions.BridgeToPavements, L("Bridge To Pavements"));
        lookups.AddChild(SetUpLookupsPermissions.MaintainableAssetsSegmentation, L("Maintainable Assets Segmentation"));
        lookups.AddChild(SetUpLookupsPermissions.BridgeReferenceData, L("Bridge Reference Data"));
        lookups.AddChild(SetUpLookupsPermissions.Counties, L("County"));
        lookups.AddChild(SetUpLookupsPermissions.DefaultSlack, L("Slack Periods"));
        lookups.AddChild(SetUpLookupsPermissions.ImportSessions, L("Import Sessions"));
        lookups.AddChild(SetUpLookupsPermissions.TreatmentCancellationMatrix, L("Treatment Cancellation Matrix"));
        lookups.AddChild(SetUpLookupsPermissions.TreatmentParameters, L("Treatment Parameters"));
        lookups.AddChild(SetUpLookupsPermissions.TreatmentTypes, L("Treatment Types"));
        lookups.AddChild(SetUpLookupsPermissions.UserTreatments, L("User Treatments"));
        lookups.AddChild(SetUpLookupsPermissions.Parameters, L("Parameters"));

        // Import
        dssGroup.AddPermission(DSSPermissions.ImportPermission.Default, L("Import"));

        // Supplemental Candidates (own group/tab)
        var supplementalCandidatesGroup = context.AddGroup(SupplementalCandidatesPermissions.GroupName, L("Candidates +"));
        var pools = supplementalCandidatesGroup.AddPermission(SupplementalCandidatesPermissions.PoolsPageActions.Default, L("Candidates +"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Create, L("Create"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.ImportWorkCandidates, L("Import Work Candidates"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.ImportConfiguration, L("Import Configuration"));

        var poolsWorkCandidates = pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Default, L("Work Candidates"));
        poolsWorkCandidates.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Create, L("Create"));
        poolsWorkCandidates.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Edit, L("Edit"));
        poolsWorkCandidates.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Delete, L("Delete"));

        var poolsConfiguration = pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Default, L("Configuration"));
        poolsConfiguration.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Create, L("Create"));
        poolsConfiguration.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Edit, L("Edit"));
        poolsConfiguration.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Delete, L("Delete"));

        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Run, L("Run"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.OutPut, L("Outputs"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Projects, L("Projects"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Map, L("Map"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.ExportProjectsOrCandidate, L("Export Projects & Candidates"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Edit, L("Edit"));
        pools.AddChild(SupplementalCandidatesPermissions.PoolsPageActions.Clear, L("Clear"));

        // Configuration (moved to Supplemental Candidates)
        var config = supplementalCandidatesGroup.AddPermission(ConfigurationPermissions.ConfigurationPageActions.Default, L("Configuration"));
        config.AddChild(ConfigurationPermissions.ConfigurationPageActions.Create, L("Create"));
        config.AddChild(ConfigurationPermissions.ConfigurationPageActions.Edit, L("Edit"));
        config.AddChild(ConfigurationPermissions.ConfigurationPageActions.Import, L("Import"));

        // Top Bar (moved to Supplemental Candidates)
        supplementalCandidatesGroup.AddPermission(DSSPermissions.TopBarPermissions.Dashboard, L("Dashboard"));
        supplementalCandidatesGroup.AddPermission(DSSPermissions.TopBarPermissions.Outputs, L("Outputs"));

        // Charts (moved to Supplemental Candidates)
        var charts = supplementalCandidatesGroup.AddPermission(ChartsPermissions.Charts.Default, L("Charts"));
        charts.AddChild(ChartsPermissions.Charts.ProjectsAndGoalsFlowDiagram, L("Projects And Goals Flow Diagram"));
        charts.AddChild(ChartsPermissions.Charts.ProjectEfficiencyAndCost, L("Project Efficiency and Cost"));
        charts.AddChild(ChartsPermissions.Charts.ProjectsAndGoalsPercentageContribution, L("Projects and Goals Percentage Contribution"));
        charts.AddChild(ChartsPermissions.Charts.ProjectsAndGoalsActualContribution, L("Projects & Goals Actual Contribution to Total Score"));
        charts.AddChild(ChartsPermissions.Charts.ProjectToScoreArcMapping, L("Project to Score Arc Mapping"));
        charts.AddChild(ChartsPermissions.Charts.BarCharts, L("Bar Charts"));

        // Reports (moved to Supplemental Candidates)
        var reports = supplementalCandidatesGroup.AddPermission(ReportsPermissions.Reports.Default, L("Reports"));
        var reportsProjects = reports.AddChild(ReportsPermissions.Reports.Projects, L("Projects"));
        reportsProjects.AddChild(ReportsPermissions.Reports.ProjectsEdit, L("Edit"));
        reportsProjects.AddChild(ReportsPermissions.Reports.ProjectsExport, L("Export to Excel"));
        var reportsWorkCandidateSummary = reports.AddChild(ReportsPermissions.Reports.WorkCandidateSummary, L("Work Candidate Summary"));
        reportsWorkCandidateSummary.AddChild(ReportsPermissions.Reports.WorkCandidateSummaryExport, L("Export to Excel"));
        var reportsWorkCandidateDetail = reports.AddChild(ReportsPermissions.Reports.WorkCandidateDetail, L("Work Candidate Detail"));
        reportsWorkCandidateDetail.AddChild(ReportsPermissions.Reports.WorkCandidateDetailExport, L("Export to Excel"));

        // Scenarios (own group/tab)
        var scenariosGroup = context.AddGroup(ScenarioPermissions.GroupName, L("Scenarios"));
        var scenarios = scenariosGroup.AddPermission(ScenarioPermissions.Default, L("Scenarios"));
        scenarios.AddChild(ScenarioPermissions.Create, L("Create"));
        scenarios.AddChild(ScenarioPermissions.Run, L("Run"));
        scenarios.AddChild(ScenarioPermissions.Delete, L("Delete"));
        scenarios.AddChild(ScenarioPermissions.Edit, L("Edit"));

        var analysis = scenarios.AddChild(ScenarioPermissions.Analysis.Default, L("Analysis"));
        analysis.AddChild(ScenarioPermissions.Analysis.Projects, L("Projects"));
        var projectTreatments = analysis.AddChild(ScenarioPermissions.Analysis.ProjectTreatments.Default, L("Project Treatments"));
        projectTreatments.AddChild(ScenarioPermissions.Analysis.ProjectTreatments.Edit, L("Edit"));
        projectTreatments.AddChild(ScenarioPermissions.Analysis.ProjectTreatments.Delete, L("Delete"));
        analysis.AddChild(ScenarioPermissions.Analysis.SummaryReport, L("Summary Report"));
        analysis.AddChild(ScenarioPermissions.Analysis.Maps, L("Maps"));

        // Treatment Pool Libraries (own group/tab)
        var treatmentPoolLibrariesGroup = context.AddGroup(TreatmentPoolLibraryPermissions.GroupName, L("Treatment Pool Libraries"));
        var tpl = treatmentPoolLibrariesGroup.AddPermission(TreatmentPoolLibraryPermissions.Default, L("Treatment Pool Libraries"));
        tpl.AddChild(TreatmentPoolLibraryPermissions.Create, L("Create"));
        tpl.AddChild(TreatmentPoolLibraryPermissions.Edit, L("Edit"));
        tpl.AddChild(TreatmentPoolLibraryPermissions.Import, L("Import"));
        tpl.AddChild(TreatmentPoolLibraryPermissions.Copy, L("Copy"));
        var treatments = tpl.AddChild(TreatmentPoolLibraryPermissions.Treatments.Default, L("Treatments"));
        treatments.AddChild(TreatmentPoolLibraryPermissions.Treatments.Create, L("Create"));
        treatments.AddChild(TreatmentPoolLibraryPermissions.Treatments.Edit, L("Edit"));
        treatments.AddChild(TreatmentPoolLibraryPermissions.Treatments.Delete, L("Delete"));
        tpl.AddChild(TreatmentPoolLibraryPermissions.Delete, L("Delete"));

        // Administration & Background Jobs
        dssGroup.AddPermission(AdministrationPermissions.Default, L("Administration"));
        dssGroup.AddPermission(BackgroundJobsPermissions.Default, L("Background Jobs"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProjectBuilderResource>(name);
    }
}
