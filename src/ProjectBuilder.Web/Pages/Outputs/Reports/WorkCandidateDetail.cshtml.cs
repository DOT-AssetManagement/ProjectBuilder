
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.Pool.Dtos;
using ProjectBuilder.Permissions;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;

namespace ProjectBuilder.Web.Pages.Outputs.Reports
{
    public class WorkCandidateDetailModel : PageModel
    {
        private readonly string _connectionString = string.Empty;
        private readonly IPoolAppService _poolsappservice;
        private readonly IPermissionChecker _permissionChecker;
        public List<CandidatePoolDto> Pools { get; set; }

        public Guid? PoolId { get; set; }
        public string PoolName { get; set; }

        public string jsonResult { get; set; }


        public WorkCandidateDetailModel(IPoolAppService poolsappservice, IConfiguration configuration, IPermissionChecker permissionChecker)
        {
            _connectionString = configuration.GetConnectionString("Default");
            _poolsappservice = poolsappservice;
            _permissionChecker = permissionChecker;
        }
        public async Task OnGetAsync(Guid? poolId, string searchString = "")
        {
            string viewQuery = "SELECT * FROM dbo.VwReportWorkCandidateDetail";
            DataTable dataTable = new DataTable();

            if (!string.IsNullOrEmpty(viewQuery))
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = viewQuery;
                    command.CommandType = CommandType.Text; 
                    command.CommandTimeout = 24000;

                    if (poolId != null)
                    {
                        command.CommandText += " WHERE PoolId = @PoolId";
                        command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                    }

                    using (var dataAdapter = new SqlDataAdapter(command))
                    {
                        dataAdapter.Fill(dataTable);
                    }
                }
            }

            jsonResult = JsonConvert.SerializeObject(dataTable);

            if (poolId.HasValue)
            {
                var pool = await _poolsappservice.GetAsync(poolId.Value);
                PoolId = poolId.Value;
                PoolName = pool.Name;
            }
            

            Pools = await _poolsappservice.GetAllAsync("", "");
        }

        // Mirrors Outputs/Reports/Projects.cshtml.cs's OnPostExport, but exports this report's
        // own view data (VwReportWorkCandidateDetail) rather than the unrelated
        // DataManager.ExportPoolData pool-wide export — this page shows a different dataset.
        public async Task<IActionResult> OnGetExportAsync(Guid poolId)
        {
            if (!await _permissionChecker.IsGrantedAsync(ReportsPermissions.Reports.WorkCandidateDetailExport))
                return Forbid();

            var dataTable = new DataTable();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using var command = new SqlCommand("SELECT * FROM dbo.VwReportWorkCandidateDetail WHERE PoolId = @PoolId", connection);
                command.CommandTimeout = 24000;
                command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId;
                using var dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataTable);
            }

            var pool = await _poolsappservice.GetAsync(poolId);

            // PoolId/WorkCandidateId are raw internal ids, never shown in this page's own grid
            // (every exported row already belongs to the one selected pool) — drop them, and
            // rename the rest to the same friendly labels the grid already shows instead of the
            // view's raw SQL column names.
            dataTable.Columns.Remove("PoolId");
            dataTable.Columns.Remove("WorkCandidateId");
            RenameColumn(dataTable, "Cost000", "Cost ($000)");
            RenameColumn(dataTable, "SafetyScore", "Safety Score");
            RenameColumn(dataTable, "MobilityScore", "Mobility Score");
            RenameColumn(dataTable, "EquityScore", "Equity Score");
            RenameColumn(dataTable, "ResilienceScore", "Resilience Score");
            RenameColumn(dataTable, "ConditionScore", "Condition Score");
            RenameColumn(dataTable, "Benefit000", "Benefit ($000)");
            RenameColumn(dataTable, "RelativeEfficiency", "Relative Efficiency");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var workbook = new ExcelPackage();
            var worksheet = workbook.Workbook.Worksheets.Add("Work Candidate Detail");
            worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
            StyleHeaderRow(worksheet, dataTable.Columns.Count);
            var bytes = await workbook.GetAsByteArrayAsync();

            var fileName = $"WorkCandidateDetail_{pool?.Name ?? poolId.ToString()}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private static void RenameColumn(DataTable table, string existingName, string newName)
        {
            if (table.Columns.Contains(existingName)) table.Columns[existingName].ColumnName = newName;
        }

        // Bold white-on-teal header, matching this page's own Tabulator grid header color
        // (.tabulator-header background #669999), frozen so it stays visible while scrolling,
        // with auto-fit columns so labels aren't clipped.
        private static void StyleHeaderRow(ExcelWorksheet worksheet, int columnCount)
        {
            using var headerRange = worksheet.Cells[1, 1, 1, columnCount];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#669999"));
            headerRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            headerRange.Style.WrapText = true;
            worksheet.Row(1).Height = 30;
            worksheet.View.FreezePanes(2, 1);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }
}
