using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using ProjectBuilder.Permissions;
using Volo.Abp.Identity;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace ProjectBuilder.Web.Authorization;

public static class ProjectBuilderRazorPagesAuthorizationConfigurer
{
    public static void Configure(RazorPagesOptions options)
    {
        void Require(string page, string permission) =>
            options.Conventions.AuthorizePage(page, permission);

        // Dashboard & top-level app
        Require("/Dashboard/Index", DSSPermissions.TopBarPermissions.Dashboard);

        // Scenarios
        Require("/Scenarios/Index", ScenarioPermissions.Default);
        Require("/Scenarios/CreateScenario", ScenarioPermissions.Create);
        Require("/EditScenario/Parameters", ScenarioPermissions.Edit);
        Require("/EditScenario/BudgetConstraints", ScenarioPermissions.Edit);
        Require("/EditScenario/ScenarioSettings", ScenarioPermissions.Edit);
        Require("/EditScenario/AnalysisMethod", ScenarioPermissions.Edit);
        Require("/ProjectTreatments/Index", ScenarioPermissions.Default);
        Require("/Treatments/Index", TreatmentPoolLibraryPermissions.Treatments.Default);
        Require("/Projects/Index", ScenarioPermissions.Analysis.Projects);

        // Treatment Pool Libraries
        Require("/TreatmentPoolLibraries/Index", TreatmentPoolLibraryPermissions.Default);

        // Supplemental Candidates
        Require("/Pools/Index", SupplementalCandidatesPermissions.PoolsPageActions.Default);
        Require("/Pools/PoolScoreParameters", SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Default);
        Require("/WorkCandidates/Index", SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Default);
        Require("/WorkCandidates/Create", SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Create);

        // Candidate Pools (legacy duplicate of Treatment Pool Libraries — shares its permission)
        Require("/CandidatePools/Index", TreatmentPoolLibraryPermissions.Default);

        // Map
        Require("/Map/Index", ScenarioPermissions.Analysis.Maps);

        // Summary Report
        Require("/Reports/SummaryReport", ScenarioPermissions.Analysis.SummaryReport);

        // Configuration
        Require("/Configuration/Goals/List", ConfigurationPermissions.ConfigurationPageActions.Default);
        Require("/Configuration/Goals/Create", ConfigurationPermissions.ConfigurationPageActions.Create);
        Require("/Configuration/Goals/Edit", ConfigurationPermissions.ConfigurationPageActions.Edit);
        Require("/Configuration/GoalWeights/Index", ConfigurationPermissions.ConfigurationPageActions.Default);
        Require("/Configuration/LoadData", ConfigurationPermissions.ConfigurationPageActions.Import);

        // Outputs
        Require("/Outputs/Index", DSSPermissions.TopBarPermissions.Outputs);
        Require("/Outputs/Maps", DSSPermissions.TopBarPermissions.Outputs);
        Require("/Outputs/Charts", ChartsPermissions.Charts.Default);
        Require("/Outputs/Prioritization", DSSPermissions.TopBarPermissions.Outputs);
        Require("/Outputs/ScaledMeasureValues", DSSPermissions.TopBarPermissions.Outputs);
        Require("/Outputs/Charts/ProjectsAndGoalsFlowDiagram", ChartsPermissions.Charts.ProjectsAndGoalsFlowDiagram);
        Require("/Outputs/Charts/ProjectEfficiencyAndCost", ChartsPermissions.Charts.ProjectEfficiencyAndCost);
        Require("/Outputs/Charts/ProjectsAndGoalsPercentageContribution", ChartsPermissions.Charts.ProjectsAndGoalsPercentageContribution);
        Require("/Outputs/Charts/ProjectsAndGoalsActualContribution", ChartsPermissions.Charts.ProjectsAndGoalsActualContribution);
        Require("/Outputs/Charts/ProjectToScoreArcMapping", ChartsPermissions.Charts.ProjectToScoreArcMapping);
        Require("/Outputs/Charts/BarCharts", ChartsPermissions.Charts.BarCharts);
        Require("/Outputs/Reports/Projects", ReportsPermissions.Reports.Projects);
        Require("/Outputs/Reports/WorkCandidateSummary", ReportsPermissions.Reports.WorkCandidateSummary);
        Require("/Outputs/Reports/WorkCandidateDetail", ReportsPermissions.Reports.WorkCandidateDetail);

        // Administration
        Require("/Administration/Index", AdministrationPermissions.Default);
        Require("/Administration/BackgroundJobs", BackgroundJobsPermissions.Default);

        // Set Up — Lookups
        Require("/SetUp/AraEnumerations/Index", SetUpLookupsPermissions.AraEnumerations);
        Require("/SetUp/BridgeToPavements/Index", SetUpLookupsPermissions.BridgeToPavements);
        Require("/SetUp/MaintainableAssetsSegmentation/Index", SetUpLookupsPermissions.MaintainableAssetsSegmentation);
        Require("/SetUp/BridgeReferenceData/Index", SetUpLookupsPermissions.BridgeReferenceData);
        Require("/SetUp/Counties", SetUpLookupsPermissions.Counties);
        Require("/SetUp/DefaultSlack/Index", SetUpLookupsPermissions.DefaultSlack);
        Require("/SetUp/ImportSessions/Index", SetUpLookupsPermissions.ImportSessions);
        Require("/SetUp/TreatmentCancellationMatrix/Index", SetUpLookupsPermissions.TreatmentCancellationMatrix);
        Require("/SetUp/TreatmentParameters/Index", SetUpLookupsPermissions.TreatmentParameters);
        Require("/SetUp/TreatmentTypes/Index", SetUpLookupsPermissions.TreatmentTypes);
        Require("/SetUp/UserTreatments/Index", SetUpLookupsPermissions.UserTreatments);
        Require("/SetUp/Parameters/List", SetUpLookupsPermissions.Parameters);

        // Set Up — Other Parameter
        Require("/SetUp/OtherParameter/List", DSSPermissions.OtherParameterPermissions.Default);
        Require("/SetUp/OtherParameter/Create", DSSPermissions.OtherParameterPermissions.Create);
        Require("/SetUp/OtherParameter/Edit", DSSPermissions.OtherParameterPermissions.Edit);

        // Set Up — Asset Type Parameter
        Require("/SetUp/AssetTypeParameter/List", DSSPermissions.AssetTypeParameterPermissions.Default);

        // Set Up — Measure
        Require("/SetUp/Measures/List", DSSPermissions.MeasurePermissions.Default);

        // Import
        Require("/Import/MAS/Index", DSSPermissions.ImportPermission.Default);
        Require("/Import/B2PCrosswalk/Index", DSSPermissions.ImportPermission.Default);
        Require("/Import/SuperAssets/Index", DSSPermissions.ImportPermission.Default);
        Require("/Import/BridgeReferenceData/Index", DSSPermissions.ImportPermission.Default);
        Require("/ImportScreen/Index", DSSPermissions.ImportPermission.Default);

        // ABP System
        Require("/Identity/Users/Index", IdentityPermissions.Users.Default);
        Require("/Identity/Users/CreateModal", IdentityPermissions.Users.Create);
        Require("/Identity/Users/EditModal", IdentityPermissions.Users.Update);
        Require("/Identity/Roles/Index", IdentityPermissions.Roles.Default);
        Require("/TenantManagement/Tenants/Index", TenantManagementPermissions.Tenants.Default);
        Require("/SettingManagement/Index", SettingManagementPermissions.Emailing);
    }
}
