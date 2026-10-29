using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Pages.Projects;
using ProjectBuilder.Web.Services;
using Volo.Abp.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.Reports
{
    public class SummaryReportModel : AnalysisPageModelBase
    {
        private const int CommandTimeoutSeconds = 300;

        private readonly ProjectBuilderDbContext _context;
        private readonly IScenarioAppService _scenarioAppService;

        public override string ActiveMenu => "SummaryReport";

        public bool HasScenarioBudgetConstraints { get; set; }
        public string? ScenarioNotes { get; set; }
        public List<int?> Districts { get; set; } = new();
        public List<SummaryReportFilterOption> CategoryOptions { get; set; } = new();
        public List<SummaryReportFilterOption> TreatmentOptions { get; set; } = new();
        public List<SummaryReportFilterOption> ProjectTypeOptions { get; set; } = new();
        public List<SummaryReportFilterOption> CountyOptions { get; set; } = new();
        public List<SummaryReportFilterOption> PlanningPartnerOptions { get; set; } = new();
        public List<SummaryReportFilterOption> BpnOptions { get; set; } = new();
        public List<SummaryReportFilterOption> FuncClassOptions { get; set; } = new();

        public SummaryReportModel(
            ProjectBuilderDbContext context,
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService,
            IProjectRepository projectRepository)
            : base(currentUser, tourGuideSeenService, projectRepository)
        {
            _context = context;
            _scenarioAppService = scenarioAppService;
        }

        public async Task<IActionResult> OnGetAsync(int scenid)
        {
            await LoadScenarioAsync(_scenarioAppService, scenid);

            var scenario = await _context.Scenarios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == scenid);
            if (scenario == null) return Page();

            ScenarioNotes = scenario.Notes;
            HasScenarioBudgetConstraints = await _context.BudgetConstraints.AsNoTracking().AnyAsync(c => c.ScenId == scenid);

            var allTreatments = _context.ProjectTreatments.AsNoTracking().Where(pt => pt.ScenarioId == scenid);

            Districts = await allTreatments
                .Where(pt => pt.District.HasValue)
                .Select(pt => (int?)pt.District!.Value)
                .Distinct().OrderBy(d => d).ToListAsync();

            CountyOptions = await allTreatments
                .Where(pt => pt.CountyId.HasValue || !string.IsNullOrWhiteSpace(pt.County))
                .Select(pt => new { Value = pt.CountyId.HasValue ? pt.CountyId.Value.ToString() : string.Empty, Label = pt.County ?? string.Empty })
                .Distinct().OrderBy(o => o.Label)
                .Select(o => new SummaryReportFilterOption { Value = o.Value, Label = o.Label })
                .ToListAsync();

            if (scenario.LibraryId != null)
            {
                CategoryOptions = (await GetCargoAttributeValuesAsync(scenario.LibraryId.Value, "CATEGORY"))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => new SummaryReportFilterOption { Value = c, Label = c })
                    .ToList();
            }

            ProjectTypeOptions = await LoadProjectTypeOptionsAsync(scenid, null);

            TreatmentOptions = await allTreatments
                .Where(pt => !string.IsNullOrWhiteSpace(pt.TreatmentType))
                .Select(pt => pt.TreatmentType!.Trim())
                .Distinct().OrderBy(t => t)
                .Select(t => new SummaryReportFilterOption { Value = t, Label = t })
                .ToListAsync();

            PlanningPartnerOptions = await LoadPlanningPartnerOptionsAsync(null, null, null);
            BpnOptions = await LoadClassificationOptionsAsync("BusPlanNetwork");
            FuncClassOptions = await LoadClassificationOptionsAsync("FuncClass");

            return Page();
        }

        public async Task<IActionResult> OnGetLoadChartAsync(
            int? scenid, int? district, string? county, string? countyLabel,
            string? category, string? treatment, string? measure, string? measures,
            string? projectTypes, string? year, string? planningPartner,
            string? bpn, string? funcClasses, string? nhs, string? groupBy)
        {
            var response = await BuildSummaryReportResponseAsync(
                scenid, district, county, countyLabel, category, treatment,
                measure, measures, projectTypes, year, planningPartner, bpn, funcClasses, nhs, groupBy);

            return new JsonResult(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task<IActionResult> OnGetPlanningPartnersAsync(int? district, string? county, string? countyLabel)
        {
            var options = await LoadPlanningPartnerOptionsAsync(district, county, countyLabel);
            return new JsonResult(options, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task<IActionResult> OnGetDownloadCsvAsync(
            int? scenid, int? district, string? county, string? countyLabel,
            string? category, string? treatment, string? measure, string? measures,
            string? projectTypes, string? year, string? planningPartner,
            string? bpn, string? funcClasses, string? nhs, string? groupBy)
        {
            var response = await BuildSummaryReportResponseAsync(
                scenid, district, county, countyLabel, category, treatment,
                measure, measures, projectTypes, year, planningPartner, bpn, funcClasses, nhs, groupBy);

            var tableRows = response.TableRows?.ToList() ?? new List<SummaryReportTableRow>();
            var tableMeasures = response.TableMeasures?.ToList() ?? new List<SummaryReportMeasureColumn>();

            if (tableRows.Count == 0) return NotFound("No report data found for export.");

            var csvBuilder = new StringBuilder();
            var headerCells = GetDimensionHeaders().Concat(tableMeasures.Select(m => m.Label));
            csvBuilder.AppendLine(string.Join(",", headerCells.Select(EscapeCsvValue)));

            foreach (var row in tableRows)
            {
                var rowValues = GetDimensionValues(row)
                    .Concat(tableMeasures.Select(m => FormatMeasureForCsv(GetMeasureValue(row, m.Key), m.ValueFormat)));
                csvBuilder.AppendLine(string.Join(",", rowValues.Select(EscapeCsvValue)));
            }

            var totalValues = new[] { "Total", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }
                .Concat(tableMeasures.Select(m => FormatMeasureForCsv(GetTotalValue(response, m.Key), m.ValueFormat)));
            csvBuilder.AppendLine(string.Join(",", totalValues.Select(EscapeCsvValue)));

            return File(
                Encoding.UTF8.GetBytes(csvBuilder.ToString()),
                "text/csv; charset=utf-8",
                BuildExportFileName(response.ScenarioName, tableMeasures.Select(m => m.Label), "SummaryReport", "csv"));
        }

        public async Task<IActionResult> OnGetDownloadXlsxAsync(
            int? scenid, int? district, string? county, string? countyLabel,
            string? category, string? treatment, string? measure, string? measures,
            string? projectTypes, string? year, string? planningPartner,
            string? bpn, string? funcClasses, string? nhs, string? groupBy)
        {
            var response = await BuildSummaryReportResponseAsync(
                scenid, district, county, countyLabel, category, treatment,
                measure, measures, projectTypes, year, planningPartner, bpn, funcClasses, nhs, groupBy);

            var tableRows = response.TableRows?.ToList() ?? new List<SummaryReportTableRow>();
            var tableMeasures = response.TableMeasures?.ToList() ?? new List<SummaryReportMeasureColumn>();

            if (tableRows.Count == 0) return NotFound("No report data found for export.");

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Summary Report");
            var dimHeaders = GetDimensionHeaders().ToList();

            for (int i = 0; i < dimHeaders.Count; i++)
                ws.Cell(1, i + 1).Value = dimHeaders[i];
            for (int i = 0; i < tableMeasures.Count; i++)
                ws.Cell(1, dimHeaders.Count + i + 1).Value = tableMeasures[i].Label;

            int rowIdx = 2;
            foreach (var row in tableRows)
            {
                var dimVals = GetDimensionValues(row).ToList();
                for (int i = 0; i < dimVals.Count; i++)
                    ws.Cell(rowIdx, i + 1).Value = dimVals[i];
                for (int i = 0; i < tableMeasures.Count; i++)
                {
                    var cell = ws.Cell(rowIdx, dimHeaders.Count + i + 1);
                    cell.Value = GetMeasureValue(row, tableMeasures[i].Key);
                    cell.Style.NumberFormat.Format = tableMeasures[i].ValueFormat == "count" ? "#,##0" : "$#,##0.00";
                }
                rowIdx++;
            }

            ws.Cell(rowIdx, 1).Value = "Total";
            for (int i = 0; i < tableMeasures.Count; i++)
            {
                var cell = ws.Cell(rowIdx, dimHeaders.Count + i + 1);
                cell.Value = GetTotalValue(response, tableMeasures[i].Key);
                cell.Style.NumberFormat.Format = tableMeasures[i].ValueFormat == "count" ? "#,##0" : "$#,##0.00";
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                BuildExportFileName(response.ScenarioName, tableMeasures.Select(m => m.Label), "SummaryReport", "xlsx"));
        }

        // ── Data building ───────────────────────────────────────────────────────

        private async Task<SummaryReportChartResponse> BuildSummaryReportResponseAsync(
            int? scenid, int? district, string? county, string? countyLabel,
            string? category, string? treatment, string? measure, string? measures,
            string? projectTypes, string? year, string? planningPartner,
            string? bpn, string? funcClasses, string? nhs, string? groupBy)
        {
            if (!scenid.HasValue) return new SummaryReportChartResponse();

            var scenario = await _context.Scenarios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == scenid.Value);
            if (scenario?.LibraryId == null)
            {
                return new SummaryReportChartResponse
                {
                    ScenarioId = scenid,
                    ScenarioName = scenario?.ScenarioName ?? string.Empty
                };
            }

            var normalizedCounty = await ResolveCountyFilterNameAsync(district, county, countyLabel);
            var normalizedCategory = Normalize(category);
            var normalizedTreatment = Normalize(treatment);
            var normalizedPlanningPartner = Normalize(planningPartner);
            var normalizedBpn = Normalize(bpn);
            var normalizedFuncClasses = NormalizeDelimited(funcClasses);
            var normalizedNhs = NormalizeNhs(nhs);
            var hasBudgetConstraints = await _context.BudgetConstraints.AsNoTracking().AnyAsync(c => c.ScenId == scenid.Value);
            var selectedMeasures = FilterUnavailableMeasures(ParseMeasures(measures, measure), hasBudgetConstraints);
            var budgetMeasureMode = selectedMeasures.All(m => m is "budget" or "budgetspent");
            var selectedProjectTypes = ParseProjectTypes(projectTypes);
            if (budgetMeasureMode && selectedProjectTypes != null)
                selectedProjectTypes = selectedProjectTypes.Where(pt => pt is "B" or "P").ToHashSet(StringComparer.OrdinalIgnoreCase);

            var selectedGroupBy = ParseGroupBy(groupBy);
            if (budgetMeasureMode && selectedGroupBy == "projecttype") selectedGroupBy = "category";

            var effectiveDistrict = budgetMeasureMode ? null : district;
            var effectiveCounty = budgetMeasureMode ? null : normalizedCounty;
            var effectivePlanningPartner = budgetMeasureMode ? null : normalizedPlanningPartner;
            var effectiveBpn = budgetMeasureMode ? null : normalizedBpn;
            var effectiveFuncClasses = budgetMeasureMode ? null : normalizedFuncClasses;
            var effectiveNhs = budgetMeasureMode ? null : normalizedNhs;

            if (selectedProjectTypes != null && selectedProjectTypes.Count == 0)
            {
                return BuildResponseFromRows(scenid, scenario, effectiveDistrict, effectiveCounty,
                    normalizedCategory, normalizedTreatment, effectivePlanningPartner,
                    selectedMeasures, selectedProjectTypes, selectedGroupBy,
                    Array.Empty<SummaryReportAggregateRow>());
            }

            var aggregateRows = await TryLoadAggregateRowsAsync(
                scenid.Value, scenario.LibraryId.Value,
                effectiveDistrict, effectiveCounty, normalizedCategory, normalizedTreatment,
                effectivePlanningPartner, selectedProjectTypes, effectiveBpn, effectiveFuncClasses,
                effectiveNhs, selectedGroupBy, budgetMeasureMode);

            if (aggregateRows == null) return new SummaryReportChartResponse { ScenarioId = scenid, ScenarioName = scenario.ScenarioName };

            return BuildResponseFromRows(scenid, scenario, effectiveDistrict, effectiveCounty,
                normalizedCategory, normalizedTreatment, effectivePlanningPartner,
                selectedMeasures, selectedProjectTypes, selectedGroupBy, aggregateRows);
        }

        private SummaryReportChartResponse BuildResponseFromRows(
            int? scenid, ProjectBuilder.Entities.Scenario scenario,
            int? district, string? normalizedCounty, string? normalizedCategory,
            string? normalizedTreatment, string? normalizedPlanningPartner,
            IReadOnlyCollection<string> selectedMeasures, HashSet<string>? selectedProjectTypes,
            string selectedGroupBy, IReadOnlyCollection<SummaryReportAggregateRow> aggregateRows)
        {
            var tableMeasures = BuildTableMeasures(selectedMeasures);
            var chartMeasure = BuildChartMeasures(selectedMeasures).FirstOrDefault() ?? ResolveSummaryMeasure(selectedMeasures);
            var districtLabel = district.HasValue ? $"District {district.Value}" : "All Districts";
            var countyLabel = string.IsNullOrWhiteSpace(normalizedCounty) ? "All Counties" : normalizedCounty;
            var ppLabel = string.IsNullOrWhiteSpace(normalizedPlanningPartner) ? "All Planning Partners" : normalizedPlanningPartner;
            var catLabel = string.IsNullOrWhiteSpace(normalizedCategory) ? "All Categories" : normalizedCategory;
            var treatLabel = string.IsNullOrWhiteSpace(normalizedTreatment) ? "All Treatments" : normalizedTreatment;
            var ptLabel = ResolveProjectTypeSelectionLabel(selectedProjectTypes);

            var tableRows = aggregateRows
                .GroupBy(r => new
                {
                    YearSort = r.YearSort ?? int.MaxValue,
                    r.YearLabel,
                    District = districtLabel,
                    County = countyLabel,
                    PlanningPartner = ppLabel,
                    Category = selectedGroupBy == "category" ? ResolveWildcard(r.Category, "All Categories") : catLabel,
                    Treatment = selectedGroupBy == "treatment" ? ResolveWildcard(r.Treatment, "All Treatments") : treatLabel,
                    ProjectType = selectedGroupBy == "projecttype" ? ResolveProjectTypeLabel(r.ProjectTypeKey) : ptLabel
                })
                .OrderBy(g => g.Key.YearSort).ThenBy(g => g.Key.Category).ThenBy(g => g.Key.Treatment).ThenBy(g => g.Key.ProjectType)
                .Select(g => new SummaryReportTableRow
                {
                    Year = g.Key.YearLabel,
                    District = g.Key.District,
                    County = g.Key.County,
                    PlanningPartner = g.Key.PlanningPartner,
                    Category = g.Key.Category,
                    Treatment = g.Key.Treatment,
                    ProjectType = g.Key.ProjectType,
                    MeasureValues = tableMeasures.ToDictionary(m => m.Key, m => CalculateMeasureValue(g, m.Key))
                })
                .ToList();

            tableRows = tableRows.Where(r => tableMeasures.Any(m => Math.Abs(GetMeasureValue(r, m.Key)) > 0d)).ToList();

            var summaryMeasure = ResolveSummaryMeasure(selectedMeasures);
            var tableTotals = tableMeasures.ToDictionary(m => m.Key, m => Math.Round(tableRows.Sum(r => GetMeasureValue(r, m.Key)), 2));

            var heatmapCells = aggregateRows
                .GroupBy(r => new
                {
                    YearSort = r.YearSort ?? int.MaxValue,
                    r.YearLabel,
                    Label = ResolveChartGroupLabel(r.Category, r.Treatment, r.ProjectTypeKey, selectedGroupBy),
                    SortOrder = ResolveChartGroupLabel(r.Category, r.Treatment, r.ProjectTypeKey, selectedGroupBy)
                })
                .Select(g => new { g.Key.YearSort, g.Key.YearLabel, g.Key.Label, g.Key.SortOrder, Value = CalculateMeasureValue(g, summaryMeasure.Key) })
                .ToList();

            var heatmapColumns = heatmapCells.OrderBy(c => c.YearSort).Select(c => c.YearLabel).Distinct().ToList();
            var heatmapRows = heatmapCells
                .GroupBy(c => new { c.Label, c.SortOrder })
                .OrderBy(g => g.Key.SortOrder).ThenBy(g => g.Key.Label)
                .Select(g => new SummaryReportHeatmapRow
                {
                    Label = g.Key.Label,
                    Values = heatmapColumns.ToDictionary(col => col, col => g.Where(c => c.YearLabel == col).Select(c => c.Value).FirstOrDefault())
                })
                .ToList();

            var chartData = BuildChartData(aggregateRows, chartMeasure, selectedGroupBy);

            return new SummaryReportChartResponse
            {
                ScenarioId = scenid,
                ScenarioName = scenario.ScenarioName,
                SummaryMeasureKey = summaryMeasure.Key,
                SummaryMeasureLabel = summaryMeasure.Label,
                SummaryValueFormat = summaryMeasure.ValueFormat,
                SummaryTotalValue = tableTotals.TryGetValue(summaryMeasure.Key, out var tot) ? tot : 0d,
                TableMeasures = tableMeasures,
                TableRows = tableRows,
                TableTotals = tableTotals,
                HeatmapRowHeader = ResolveGroupByAxisLabel(selectedGroupBy),
                HeatmapColumns = heatmapColumns,
                HeatmapRows = heatmapRows,
                ChartLabels = chartData.Labels,
                ChartDatasets = chartData.Datasets
            };
        }

        private async Task<List<SummaryReportAggregateRow>?> TryLoadAggregateRowsAsync(
            int scenarioId, Guid libraryId, int? district, string? countyName,
            string? category, string? treatment, string? planningPartner,
            HashSet<string>? selectedProjectTypes, string? bpn, string? funcClasses,
            string? nhs, string selectedGroupBy, bool budgetMeasureMode)
        {
            var rows = new List<SummaryReportAggregateRow>();
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            try
            {
                if (shouldClose) await connection.OpenAsync();

                await using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.sp_pb_ui_SummaryReportAggregates";
                cmd.CommandTimeout = CommandTimeoutSeconds;

                AddParam(cmd, "@ScenarioId", scenarioId);
                AddParam(cmd, "@LibraryId", libraryId);
                AddParam(cmd, "@District", district);
                AddParam(cmd, "@CountyName", countyName);
                AddParam(cmd, "@Category", category);
                AddParam(cmd, "@Treatment", treatment);
                AddParam(cmd, "@PlanningPartner", planningPartner);
                AddParam(cmd, "@ProjectTypes", selectedProjectTypes == null ? null : string.Join(",", selectedProjectTypes.OrderBy(p => p)));
                AddParam(cmd, "@Bpn", bpn);
                AddParam(cmd, "@FuncClasses", funcClasses);
                AddParam(cmd, "@Nhs", nhs);
                AddParam(cmd, "@GroupBy", selectedGroupBy);
                AddParam(cmd, "@UseBudgetMeasureMode", budgetMeasureMode);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new SummaryReportAggregateRow
                    {
                        YearSort = reader["YearSort"] == DBNull.Value ? null : Convert.ToInt32(reader["YearSort"], CultureInfo.InvariantCulture),
                        YearLabel = NormalizeLabel(reader["YearLabel"]?.ToString(), "Unscheduled"),
                        Category = NormalizeLabel(reader["Category"]?.ToString(), "Uncategorized"),
                        Treatment = NormalizeLabel(reader["Treatment"]?.ToString(), "Unspecified Treatment"),
                        ProjectTypeKey = NormalizeLabel(reader["ProjectType"]?.ToString(), string.Empty),
                        BudgetAmount = ReadDouble(reader, "BudgetAmount"),
                        SpentBudgetAmount = ReadDouble(reader, "SpentBudgetAmount"),
                        DirectCost = ReadDouble(reader, "DirectCost"),
                        IndirectCostDesign = ReadDouble(reader, "IndirectCostDesign"),
                        IndirectCostRow = ReadDouble(reader, "IndirectCostRow"),
                        IndirectCostUtilities = ReadDouble(reader, "IndirectCostUtilities"),
                        IndirectCostOther = ReadDouble(reader, "IndirectCostOther"),
                        TotalCost = ReadDouble(reader, "TotalCost"),
                        Benefit = ReadDouble(reader, "Benefit"),
                        ProjectCount = reader["ProjectCount"] == DBNull.Value ? 0L : Convert.ToInt64(reader["ProjectCount"], CultureInfo.InvariantCulture),
                        TreatmentCount = reader["TreatmentCount"] == DBNull.Value ? 0L : Convert.ToInt64(reader["TreatmentCount"], CultureInfo.InvariantCulture)
                    });
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (shouldClose && connection.State == ConnectionState.Open) await connection.CloseAsync();
            }

            return rows;
        }

        // ── Filter option loaders ───────────────────────────────────────────────

        private async Task<string?> ResolveCountyFilterNameAsync(int? district, string? countyValue, string? countyLabel)
        {
            var fromLabel = Normalize(countyLabel);
            if (!string.IsNullOrWhiteSpace(fromLabel))
            {
                var sep = fromLabel.IndexOf('-');
                if (sep > 0 && int.TryParse(fromLabel[..sep].Trim(), out _))
                    return fromLabel[(sep + 1)..].Trim();
                return fromLabel;
            }

            var fromValue = Normalize(countyValue);
            if (string.IsNullOrWhiteSpace(fromValue) || !int.TryParse(fromValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var countyId))
                return null;

            var q = _context.Counties.AsNoTracking().Where(c => c.Id == countyId && c.CountyName != null && c.CountyName != "");
            if (district.HasValue) q = q.Where(c => c.District == district.Value);
            var name = await q.Select(c => c.CountyName).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(name))
                name = await _context.Counties.AsNoTracking().Where(c => c.Id == countyId && c.CountyName != null && c.CountyName != "").Select(c => c.CountyName).FirstOrDefaultAsync();

            return Normalize(name);
        }

        private async Task<List<SummaryReportFilterOption>> LoadPlanningPartnerOptionsAsync(int? district, string? countyValue, string? countyLabel)
        {
            var normalizedCounty = await ResolveCountyFilterNameAsync(district, countyValue, countyLabel);
            var options = new List<SummaryReportFilterOption>();
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            try
            {
                if (shouldClose) await connection.OpenAsync();
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
SELECT DISTINCT LTRIM(RTRIM(COALESCE(NULLIF(MPO_Name, ''), NULLIF(MPO, '')))) AS PlanningPartner
FROM dbo.tbl_pb_PlanningPartner
WHERE COALESCE(NULLIF(LTRIM(RTRIM(MPO_Name)), ''), NULLIF(LTRIM(RTRIM(MPO)), '')) IS NOT NULL
  AND (@countyName IS NULL OR UPPER(LTRIM(RTRIM(County))) = UPPER(@countyName))
ORDER BY PlanningPartner;";
                AddParam(cmd, "@countyName", normalizedCounty);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var pp = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
                    if (!string.IsNullOrWhiteSpace(pp))
                        options.Add(new SummaryReportFilterOption { Value = pp, Label = pp });
                }
            }
            catch { return new List<SummaryReportFilterOption>(); }
            finally { if (shouldClose && connection.State == ConnectionState.Open) await connection.CloseAsync(); }

            return options;
        }

        private async Task<List<SummaryReportFilterOption>> LoadClassificationOptionsAsync(string columnName)
        {
            var col = string.Equals(columnName, "FuncClass", StringComparison.OrdinalIgnoreCase) ? "FuncClass" : "BusPlanNetwork";
            var options = new List<SummaryReportFilterOption>();
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            try
            {
                if (shouldClose) await connection.OpenAsync();
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = $@"
SELECT DISTINCT OptionValue FROM (
    SELECT LTRIM(RTRIM(CONVERT(VARCHAR(50), {col}))) AS OptionValue FROM dbo.tbl_pb_BridgeClassification
    UNION
    SELECT LTRIM(RTRIM(CONVERT(VARCHAR(50), {col}))) AS OptionValue FROM dbo.tbl_pb_PavementClassification
) ClassificationOptions
WHERE NULLIF(OptionValue, '') IS NOT NULL ORDER BY OptionValue;";
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var v = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
                    if (!string.IsNullOrWhiteSpace(v))
                        options.Add(new SummaryReportFilterOption { Value = v, Label = v });
                }
            }
            catch { return new List<SummaryReportFilterOption>(); }
            finally { if (shouldClose && connection.State == ConnectionState.Open) await connection.CloseAsync(); }

            return options;
        }

        private async Task<List<SummaryReportFilterOption>> LoadProjectTypeOptionsAsync(int scenarioId, int? district)
        {
            var q = _context.Projects.AsNoTracking().Where(p => p.ScenarioId == scenarioId);
            if (district.HasValue) q = q.Where(p => p.District.HasValue && p.District.Value == district.Value);

            var keys = (await q.Select(p => p.AssetType).Distinct().ToListAsync())
                .Select(NormalizeProjectTypeKey)
                .Where(k => k is "B" or "P" or "C")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(GetProjectTypeSortOrder).ThenBy(MapProjectTypeLabel)
                .ToList();

            return keys.Select(k => new SummaryReportFilterOption { Value = k, Label = MapProjectTypeLabel(k) }).ToList();
        }

        private async Task<List<string>> GetCargoAttributeValuesAsync(Guid libraryId, string attributeName)
        {
            var attrUpper = attributeName.Trim().ToUpperInvariant();
            return await _context.CargoData.AsNoTracking()
                .Join(_context.CargoAttributes.AsNoTracking(),
                    cd => new { cd.LibraryId, cd.AttributeNo, cd.AssetType },
                    ca => new { ca.LibraryId, ca.AttributeNo, ca.AssetType },
                    (cd, ca) => new { cd, ca })
                .Where(j => j.cd.LibraryId == libraryId
                    && j.ca.AttributeName != null
                    && j.ca.AttributeName.Trim().ToUpper() == attrUpper
                    && j.cd.TextValue != null && j.cd.TextValue != "")
                .Select(j => j.cd.TextValue!.Trim())
                .Distinct().OrderBy(t => t)
                .ToListAsync();
        }

        // ── Chart / measure helpers ─────────────────────────────────────────────

        private static (List<string> Labels, List<SummaryReportChartDataset> Datasets) BuildChartData(
            IReadOnlyCollection<SummaryReportAggregateRow> rows,
            SummaryReportMeasureColumn chartMeasure,
            string selectedGroupBy)
        {
            var yearBuckets = rows
                .Select(r => (Sort: r.YearSort ?? int.MaxValue, Label: NormalizeLabel(r.YearLabel, "Unscheduled")))
                .Distinct().OrderBy(b => b.Sort).ThenBy(b => b.Label).ToList();

            var datasets = rows
                .GroupBy(r => new { Label = ResolveChartGroupLabel(r.Category, r.Treatment, r.ProjectTypeKey, selectedGroupBy) })
                .OrderBy(g => g.Key.Label)
                .Select(g => new SummaryReportChartDataset
                {
                    Key = chartMeasure.Key,
                    Label = g.Key.Label,
                    ValueFormat = chartMeasure.ValueFormat,
                    Stack = "summary-report-group",
                    Values = yearBuckets.Select(yb => CalculateMeasureValue(
                        g.Where(r => (r.YearSort ?? int.MaxValue) == yb.Sort && NormalizeLabel(r.YearLabel, "Unscheduled") == yb.Label),
                        chartMeasure.Key)).ToList()
                })
                .ToList();

            return (yearBuckets.Select(b => b.Label).ToList(), datasets);
        }

        private static double CalculateMeasureValue(IEnumerable<SummaryReportAggregateRow> rows, string measure)
        {
            return measure switch
            {
                "budgetAmount" => Math.Round(rows.Sum(r => r.BudgetAmount), 2),
                "budgetSpentAmount" => Math.Round(rows.Sum(r => r.SpentBudgetAmount), 2),
                "benefit" => Math.Round(rows.Sum(r => r.Benefit), 2),
                "directCost" => Math.Round(rows.Sum(r => r.DirectCost), 2),
                "projectTreatmentCount" => rows.Sum(r => Convert.ToDouble(r.ProjectCount + r.TreatmentCount, CultureInfo.InvariantCulture)),
                _ => Math.Round(rows.Sum(r => r.TotalCost), 2)
            };
        }

        private static List<SummaryReportMeasureColumn> BuildTableMeasures(IReadOnlyCollection<string> selectedMeasures)
        {
            var list = new List<SummaryReportMeasureColumn>();
            if (selectedMeasures.Contains("budget")) AddMeasure(list, "budgetAmount", "Budget Total", "currency");
            if (selectedMeasures.Contains("budgetspent")) AddMeasure(list, "budgetSpentAmount", "Budget Spent", "currency");
            if (selectedMeasures.Contains("needs")) AddMeasure(list, "totalCost", "Needs", "currency");
            if (selectedMeasures.Contains("benefits")) AddMeasure(list, "benefit", "Benefits", "currency");
            if (selectedMeasures.Contains("projecttreatmentcount")) AddMeasure(list, "projectTreatmentCount", "Project Count / Treatment Count", "count");
            return list;
        }

        private static List<SummaryReportMeasureColumn> BuildChartMeasures(IReadOnlyCollection<string> selectedMeasures)
        {
            var list = new List<SummaryReportMeasureColumn>();
            if (selectedMeasures.Contains("needs")) { AddMeasure(list, "totalCost", "Needs", "currency"); return list; }
            if (selectedMeasures.Contains("budget")) { AddMeasure(list, "budgetAmount", "Budget Total", "currency"); return list; }
            if (selectedMeasures.Contains("budgetspent")) { AddMeasure(list, "budgetSpentAmount", "Budget Spent", "currency"); return list; }
            if (selectedMeasures.Contains("benefits")) { AddMeasure(list, "benefit", "Benefits", "currency"); return list; }
            if (selectedMeasures.Contains("projecttreatmentcount")) AddMeasure(list, "projectTreatmentCount", "Project Count / Treatment Count", "count");
            if (list.Count == 0) AddMeasure(list, "budgetAmount", "Budget Total", "currency");
            return list;
        }

        private static SummaryReportMeasureColumn ResolveSummaryMeasure(IReadOnlyCollection<string> selectedMeasures)
        {
            if (selectedMeasures.Contains("needs")) return new() { Key = "totalCost", Label = "Needs", ValueFormat = "currency" };
            if (selectedMeasures.Contains("budget")) return new() { Key = "budgetAmount", Label = "Budget Total", ValueFormat = "currency" };
            if (selectedMeasures.Contains("budgetspent")) return new() { Key = "budgetSpentAmount", Label = "Budget Spent", ValueFormat = "currency" };
            if (selectedMeasures.Contains("benefits")) return new() { Key = "benefit", Label = "Benefits", ValueFormat = "currency" };
            return new() { Key = "projectTreatmentCount", Label = "Project Count / Treatment Count", ValueFormat = "count" };
        }

        private static void AddMeasure(List<SummaryReportMeasureColumn> list, string key, string label, string fmt)
        {
            if (!list.Any(m => m.Key == key))
                list.Add(new SummaryReportMeasureColumn { Key = key, Label = label, ValueFormat = fmt });
        }

        // ── Label / type helpers ────────────────────────────────────────────────

        private static string ResolveChartGroupLabel(string category, string treatment, string projectType, string groupBy)
        {
            return groupBy switch
            {
                "treatment" => ResolveWildcard(treatment, "All Treatments"),
                "projecttype" => ResolveProjectTypeLabel(projectType),
                _ => ResolveWildcard(category, "All Categories")
            };
        }

        private static string ResolveWildcard(string? value, string wildFallback)
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "*", StringComparison.OrdinalIgnoreCase))
                return wildFallback;
            return value.Trim();
        }

        private static string ResolveProjectTypeLabel(string? assetType)
        {
            return NormalizeProjectTypeKey(assetType) switch
            {
                "P" => "Pavement",
                "B" => "Bridge",
                "C" => "Combined",
                _ => string.IsNullOrWhiteSpace(assetType) || assetType.Trim() == "*" ? "All Project Types" : assetType
            };
        }

        private static string ResolveProjectTypeSelectionLabel(HashSet<string>? types)
        {
            if (types == null) return "All Project Types";
            if (types.Count == 0) return "No Project Types";
            if (types.Count == 1) return ResolveProjectTypeLabel(types.First());
            return string.Join(", ", types.OrderBy(t => t).Select(ResolveProjectTypeLabel));
        }

        private static string NormalizeProjectTypeKey(string? t)
        {
            return t?.Trim().ToUpperInvariant() switch
            {
                "BRIDGE" or "B" => "B",
                "PAVEMENT" or "P" => "P",
                "COMBINED" or "C" => "C",
                _ => string.Empty
            };
        }

        private static string MapProjectTypeLabel(string k) => NormalizeProjectTypeKey(k) switch { "P" => "Pavement", "B" => "Bridge", "C" => "Combined", _ => k };
        private static int GetProjectTypeSortOrder(string k) => NormalizeProjectTypeKey(k) switch { "B" => 0, "P" => 1, "C" => 2, _ => 3 };

        private static string ResolveGroupByAxisLabel(string groupBy) => groupBy switch
        {
            "treatment" => "Treatment",
            "projecttype" => "Project Type",
            _ => "Category"
        };

        // ── Parse / normalize helpers ───────────────────────────────────────────

        private static IReadOnlyList<string> ParseMeasures(string? measures, string? measure)
        {
            var list = (measures ?? measure ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(m => m.ToLowerInvariant())
                .Select(m => m is "projectcount" or "treatmentcount" ? "projecttreatmentcount" : m is "cost" ? "needs" : m)
                .Where(m => m is "budget" or "needs" or "benefits" or "projecttreatmentcount" or "budgetspent")
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (list.Count == 0) list.Add("budget");
            return list;
        }

        private static IReadOnlyList<string> FilterUnavailableMeasures(IReadOnlyList<string> measures, bool hasBudget)
        {
            if (hasBudget) return measures;
            var filtered = measures.Where(m => m is not "budget" and not "budgetspent").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (filtered.Count == 0) filtered.Add("needs");
            return filtered;
        }

        private static HashSet<string>? ParseProjectTypes(string? projectTypes)
        {
            if (string.IsNullOrWhiteSpace(projectTypes)) return null;
            var set = projectTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeProjectTypeKey)
                .Where(k => k is "B" or "P" or "C")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return set.Count == 0 ? null : set;
        }

        private static string ParseGroupBy(string? groupBy) => groupBy?.Trim().ToLowerInvariant() switch
        {
            "treatment" => "treatment",
            "projecttype" => "projecttype",
            _ => "category"
        };

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeDelimited(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var vals = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return vals.Count == 0 ? null : string.Join(",", vals);
        }

        private static string? NormalizeNhs(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var vals = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(v => v.ToUpperInvariant()).Where(v => v is "ON" or "OFF").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (vals.Count == 0 || vals.Count == 2) return null;
            return string.Join(",", vals);
        }

        private static string NormalizeLabel(string? value, string fallback)
        {
            var n = value?.Trim();
            return string.IsNullOrWhiteSpace(n) ? fallback : n;
        }

        // ── Export helpers ──────────────────────────────────────────────────────

        private static IEnumerable<string> GetDimensionHeaders() =>
            new[] { "Year", "District", "County", "Planning Partner", "Treatment Category", "Treatment", "Project Type" };

        private static IEnumerable<string> GetDimensionValues(SummaryReportTableRow row) =>
            new[] { row.Year, row.District, row.County, row.PlanningPartner, row.Category, row.Treatment, row.ProjectType };

        private static double GetMeasureValue(SummaryReportTableRow row, string key) =>
            row.MeasureValues != null && row.MeasureValues.TryGetValue(key, out var v) ? v : 0d;

        private static double GetTotalValue(SummaryReportChartResponse response, string key) =>
            response.TableTotals != null && response.TableTotals.TryGetValue(key, out var v) ? v : 0d;

        private static string FormatMeasureForCsv(double value, string fmt) =>
            fmt == "count" ? value.ToString("0", CultureInfo.InvariantCulture) : value.ToString("C2", CultureInfo.GetCultureInfo("en-US"));

        private static string EscapeCsvValue(string value) => '"' + (value ?? string.Empty).Replace("\"", "\"\"") + '"';

        private static string BuildExportFileName(string scenarioName, IEnumerable<string> measureLabels, string prefix, string ext)
        {
            var scenPart = SanitizeFileSegment(scenarioName, "Scenario");
            var measurePart = string.Join("_", measureLabels.Select(l => SanitizeFileSegment(l, string.Empty)).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct(StringComparer.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(measurePart)
                ? $"{prefix}_{scenPart}_{DateTime.Now:yyyyMMddHHmmss}.{ext}"
                : $"{prefix}_{scenPart}_{measurePart}_{DateTime.Now:yyyyMMddHHmmss}.{ext}";
        }

        private static string SanitizeFileSegment(string? value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            return string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();
        }

        // ── Tiny ADO helpers ────────────────────────────────────────────────────

        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static double ReadDouble(System.Data.IDataReader reader, string column)
        {
            var val = reader[column];
            return val == DBNull.Value ? 0d : Convert.ToDouble(val, CultureInfo.InvariantCulture);
        }
    }
}
