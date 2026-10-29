using System.Collections.Generic;

namespace ProjectBuilder.Web.Pages.Reports
{
    public class SummaryReportFilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SummaryReportChartResponse
    {
        public int? ScenarioId { get; set; }
        public string ScenarioName { get; set; } = string.Empty;
        public string SummaryMeasureKey { get; set; } = string.Empty;
        public string SummaryMeasureLabel { get; set; } = string.Empty;
        public string SummaryValueFormat { get; set; } = "currency";
        public double SummaryTotalValue { get; set; }
        public IEnumerable<SummaryReportMeasureColumn> TableMeasures { get; set; } = System.Array.Empty<SummaryReportMeasureColumn>();
        public IEnumerable<SummaryReportTableRow> TableRows { get; set; } = System.Array.Empty<SummaryReportTableRow>();
        public Dictionary<string, double> TableTotals { get; set; } = new();
        public string HeatmapRowHeader { get; set; } = "Treatment";
        public IEnumerable<string> HeatmapColumns { get; set; } = System.Array.Empty<string>();
        public IEnumerable<SummaryReportHeatmapRow> HeatmapRows { get; set; } = System.Array.Empty<SummaryReportHeatmapRow>();
        public IEnumerable<string> ChartLabels { get; set; } = System.Array.Empty<string>();
        public IEnumerable<SummaryReportChartDataset> ChartDatasets { get; set; } = System.Array.Empty<SummaryReportChartDataset>();
    }

    public class SummaryReportTableRow
    {
        public string Year { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string PlanningPartner { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty;
        public Dictionary<string, double> MeasureValues { get; set; } = new();
    }

    public class SummaryReportMeasureColumn
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ValueFormat { get; set; } = "currency";
    }

    public class SummaryReportHeatmapRow
    {
        public string Label { get; set; } = string.Empty;
        public Dictionary<string, double> Values { get; set; } = new();
    }

    public class SummaryReportAggregateRow
    {
        public int? YearSort { get; set; }
        public string YearLabel { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string ProjectTypeKey { get; set; } = string.Empty;
        public double BudgetAmount { get; set; }
        public double SpentBudgetAmount { get; set; }
        public double DirectCost { get; set; }
        public double IndirectCostDesign { get; set; }
        public double IndirectCostRow { get; set; }
        public double IndirectCostUtilities { get; set; }
        public double IndirectCostOther { get; set; }
        public double TotalCost { get; set; }
        public double Benefit { get; set; }
        public long ProjectCount { get; set; }
        public long TreatmentCount { get; set; }
    }

    public class SummaryReportChartDataset
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ValueFormat { get; set; } = "currency";
        public string Stack { get; set; } = string.Empty;
        public IEnumerable<double> Values { get; set; } = System.Array.Empty<double>();
    }
}
