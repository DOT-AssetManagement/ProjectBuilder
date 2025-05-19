using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public enum CurrentView
    {
        DashboardView,
        TreatmentsView,
        RunView,
        ScenariosView,
        ReportsView,
        SettingsView,
        ImportTreatmentView,
        EditTreatmentView,
        ConfirmationMessageView,
        NewTreatmentView,
        TreatmentGroupsView,
        TreatmentTypesView,
        ProjectsView,
        ProjectDetailsView,
        RunScenarioView,
        HelpView,
        ImportPAMSBAMSView,
        ChartsView,
        NeedsChartView,
        BudgetSpentView,
        BudgetChartView,
        PotentialBenefitsChartView,
        ProjectSummaryView,
        TreatmentSummaryView,
        CombinedProjectView,
        None
    }
    public enum CurrentTheme : int
    {
        System = 0,
        Dark = 1,
        Light = 2
    }
    public enum SettingName
    {
        CurrentTheme,
        CurrentPalette,
        CustomPalette,
        ShowDeleteConfirmationMessage        
    }
    public enum Roles
    {
        Guest,
        Operator,
        Admin
    }
}
