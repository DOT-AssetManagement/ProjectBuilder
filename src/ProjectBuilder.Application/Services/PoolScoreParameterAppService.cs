using OfficeOpenXml;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.PoolScoreParameters;
using ProjectBuilder.Models.PoolScoreParameters.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Services
{
    [Authorize(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Default)]
    public class PoolScoreParameterAppService : ApplicationService, IPoolScoreParametersAppService
    {
        private readonly IPoolScoreParameterRepository _repository;
        private readonly IWorkCandidatesRepository _workCandidatesRepository;

        public PoolScoreParameterAppService(IPoolScoreParameterRepository repository, IWorkCandidatesRepository workCandidatesRepository)
        {
            _repository = repository;
            _workCandidatesRepository = workCandidatesRepository;
        }

        // Every scoreable measure column on tbl_dss_WorkCandidates (WorkCandidateEntity), the
        // exact set of names UspCreateScoreSQL can build a valid formula against — it takes
        // PoolScoreParameters.Measure as a literal column name. This intentionally excludes
        // WorkCandidateEntity's key/location/aggregate columns (Id, District, *_Score totals,
        // TotalScore, Cost, Benefit, etc.), which aren't configurable per-measure inputs.
        //
        // The separate tbl_dss_Measures admin lookup ("Look Up Data > DSS > Measures") was tried
        // first as the dropdown source but is a manually-curated catalog that isn't guaranteed to
        // be populated or kept in sync with this entity, so it isn't used here.
        private static readonly string[] MeasureNames =
        {
            "HSIP_Eligible", "Excess_Safety_Value", "Supported_by_RSA_Safety_Study",
            "Degree_of_Safety_Improvement", "Supports_Hazard_Mitigation", "Improves_Access_to_Emergency_Services",
            "Predicted_Safety_Benefit", "Safety_Score", "Travel_Time_Savings", "Operating_Cost_Savings",
            "Degree_of_Mobility_Improvement", "Freight_Improvement", "Facilitates_Transit_Use",
            "Facilitates_Active_Transportation", "Bike_Lanes_Constructed", "New_Cyclists",
            "Support_for_Job_Growth", "Support_for_Tourism", "Support_for_Economic_Goals",
            "Support_for_Recreation", "Consistency_with_Existing_Plans", "Predicted_Mobility_Benefit",
            "Mobility_and_Access_Score", "Sidewalks_and_Curb_Ramps_Constructed_or_Improved",
            "Change_in_Accessible_Destinations", "Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations",
            "Support_for_Environmental_Justice", "Predicted_Equity_and_Access_Benefit", "Equity_and_Access_Score",
            "Reduced_Flood_Closure_Risk", "Wetlands_Improved", "Wildlife_Crossings", "Reduced_Fuel_Consumption",
            "Degree_of_Environmental_Improvement", "Predicted_Environmental_Benefit", "Resilience_and_Environment_Score",
            "Consistency_with_Land_Use_Plans", "Pavement_Rehabilitated_or_Reconstructed",
            "Bridges_Rehabilitated_or_Replaced", "Culverts_Rehabilitated_or_Replaced",
            "Guardrail_Rehabilitated_or_Replaced", "Geotechnical_Assets_Rehabilitated_or_Replaced",
            "Facilities_Rehabilitated_or_Reconstructed", "Sidewalks_Rehabilitated_or_Reconstructed",
            "Predicted_Condition_and_Performance_Benefit", "Condition_and_Performance_Score",
            "ADT", "Percent_Trucks", "Population_Density", "Bike_Commute_Share", "Length", "Speed_Limit",
            "Detour_Distance", "Functional_Classification", "National_Highway_System", "Pennsylvania_Byway",
            "Bicycle_PA_Route", "Interstate_Emergency_Detour", "LFAR_BOF_Eligible"
        };

        // Feeds the Configuration Create/Edit modal's "Measure" dropdown. A free-text field let
        // users type a name that doesn't match any real column, silently producing a formula
        // that never resolves to real data.
        public Task<List<string>> GetMeasureNamesAsync()
        {
            return Task.FromResult(MeasureNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList());
        }

        // Excel template for the Import modal: one row per catalog measure name, pre-filled in
        // the Measure column so users only need to fill in the numeric scoring columns, matching
        // tbl_dss_RawImportedScoreParameters' column order exactly (ExcelHandler.ImportScoreParameters
        // bulk-inserts by column name/order).
        public async Task<(byte[] Content, string FileName)> DownloadScoreParametersTemplateAsync()
        {
            var measureNames = await GetMeasureNamesAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("ScoreParameters");

            var headers = new[]
            {
                "Measure", "Min", "ScoreAtMin", "Max", "ScoreAtMax", "Slope",
                "Intercept", "SafetyWeight", "MobilityWeight", "EquityWeight", "EnvWeight", "CondWeight"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            for (int i = 0; i < measureNames.Count; i++)
            {
                sheet.Cells[i + 2, 1].Value = measureNames[i];
            }
            sheet.Cells.AutoFitColumns();

            return (package.GetAsByteArray(), "ScoreParametersTemplate.xlsx");
        }

        public async Task<List<PoolScoreParameterDto>> GetListByPoolIdAsync(Guid poolId)
        {
            var entities = await _repository.GetListByPoolIdAsync(poolId);
            return entities.Select(ToDto).ToList();
        }

        // Exports the same rows/columns shown in the Configuration grid.
        public async Task<(byte[] Content, string FileName)> ExportAsync(Guid poolId, string poolName)
        {
            var parameters = await GetListByPoolIdAsync(poolId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("ScoreParameters");

            var headers = new[]
            {
                "Measure", "Min", "ScoreAtMin", "Max", "ScoreAtMax", "Slope",
                "SafetyWeight", "MobilityWeight", "EquityWeight", "EnvWeight", "CondWeight",
                "CreatedBy", "CreatedAt"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            for (int r = 0; r < parameters.Count; r++)
            {
                var p = parameters[r];
                int row = r + 2;
                sheet.Cells[row, 1].Value = p.Measure;
                sheet.Cells[row, 2].Value = p.Min;
                sheet.Cells[row, 3].Value = p.ScoreAtMin;
                sheet.Cells[row, 4].Value = p.Max;
                sheet.Cells[row, 5].Value = p.ScoreAtMax;
                sheet.Cells[row, 6].Value = p.Slope;
                sheet.Cells[row, 7].Value = p.SafetyWeight;
                sheet.Cells[row, 8].Value = p.MobilityWeight;
                sheet.Cells[row, 9].Value = p.EquityWeight;
                sheet.Cells[row, 10].Value = p.EnvWeight;
                sheet.Cells[row, 11].Value = p.CondWeight;
                sheet.Cells[row, 12].Value = p.CreatedBy;
                sheet.Cells[row, 13].Value = p.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss");
            }
            sheet.Cells.AutoFitColumns();

            return (package.GetAsByteArray(), $"Configuration_{SanitizeFileNameSegment(poolName)}.xlsx");
        }

        private static string SanitizeFileNameSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Pool";
            return string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();
        }

        public async Task<PoolScoreParameterDto?> GetByPoolIdAndMeasureAsync(Guid poolId, string measure)
        {
            var entity = await _repository.GetByPoolIdAndMeasureAsync(poolId, measure);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<PoolScoreParameterDto> CreateAsync(PoolScoreParameterCreateDto input)
        {
            var entity = new PoolScoreParameterEntity
            {
                PoolId = input.PoolId,
                Measure = input.Measure,
                Min = input.Min,
                ScoreAtMin = input.ScoreAtMin,
                Max = input.Max,
                ScoreAtMax = input.ScoreAtMax,
                Slope = input.Slope,
                SafetyWeight = input.SafetyWeight,
                MobilityWeight = input.MobilityWeight,
                EquityWeight = input.EquityWeight,
                EnvWeight = input.EnvWeight,
                CondWeight = input.CondWeight,
                CreatedBy = input.CreatedBy,
                CreatedAt = input.CreatedAt ?? DateTime.Now
            };
            var result = await _repository.InsertAsync(entity);
            await _workCandidatesRepository.UpdatePool(input.PoolId, false);
            return ToDto(result);
        }

        public async Task<PoolScoreParameterDto> UpdateAsync(PoolScoreParameterUpdateDto input)
        {
            var entity = new PoolScoreParameterEntity
            {
                PoolId = input.PoolId,
                Measure = input.Measure,
                Min = input.Min,
                ScoreAtMin = input.ScoreAtMin,
                Max = input.Max,
                ScoreAtMax = input.ScoreAtMax,
                Slope = input.Slope,
                SafetyWeight = input.SafetyWeight,
                MobilityWeight = input.MobilityWeight,
                EquityWeight = input.EquityWeight,
                EnvWeight = input.EnvWeight,
                CondWeight = input.CondWeight,
                CreatedBy = input.CreatedBy,
                CreatedAt = input.CreatedAt
            };
            await _repository.UpdateAsync(entity);
            await _workCandidatesRepository.UpdatePool(input.PoolId, false);
            return ToDto(entity);
        }

        public async Task<bool> DeleteAsync(Guid poolId, string measure)
        {
            var result = await _repository.DeleteAsync(poolId, measure);
            await _workCandidatesRepository.UpdatePool(poolId, false);
            return result;
        }

        public async Task<bool> ImportAsync(string excelFilePath, string? worksheetName, Guid poolId, bool fromScratch, bool keepUserCreated)
        {
            // Validate before anything touches the database — the legacy ExcelHandler.ImportScoreParameters
            // pipeline bulk-inserts whatever it parses straight into tbl_dss_RawImportedScoreParameters with
            // no checks of its own, so a bad file either silently produces unscorable formulas (unrecognized
            // Measure names) or throws an unhandled SQL exception (non-numeric junk in a FLOAT column).
            ValidateScoreParametersFile(excelFilePath, worksheetName);

            var result = _repository.Import(poolId, excelFilePath, worksheetName, fromScratch, keepUserCreated);
            // Bulk score-parameter import replaces the pool's scoring formulas, so UspCreateScoreSQL
            // must regenerate tbl_dss_PoolScoreSQL (scoreParametersUpdated: true) before scores can be
            // recalculated — otherwise every score column stays NULL and downstream charts/reports (e.g.
            // the Sankey flow diagram) render NaN.
            await _workCandidatesRepository.UpdatePool(poolId, true);
            return result;
        }

        // Numeric columns tbl_dss_RawImportedScoreParameters expects (Intercept excluded — it's
        // nullable/unused elsewhere in the app, so it isn't validated here).
        private static readonly string[] NumericColumns =
        {
            "Min", "ScoreAtMin", "Max", "ScoreAtMax", "Slope",
            "SafetyWeight", "MobilityWeight", "EquityWeight", "EnvWeight", "CondWeight"
        };

        private void ValidateScoreParametersFile(string excelFilePath, string? worksheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelWorksheet sheet;
            try
            {
                using var package = new ExcelPackage(new FileInfo(excelFilePath));
                var worksheets = package.Workbook.Worksheets;

                if (worksheets.Count > 1)
                {
                    // Multiple sheets — Tab Name must be provided and must exactly name one of
                    // them; unlike the single-sheet case there's no reasonable default to fall
                    // back to, so guessing (e.g. always the first sheet) risks silently importing
                    // the wrong data instead of what the user actually meant.
                    if (string.IsNullOrWhiteSpace(worksheetName))
                    {
                        var names = string.Join(", ", worksheets.Select(w => w.Name));
                        throw new UserFriendlyException($"This file has multiple sheets ({names}). Enter the exact sheet name to import from in \"Tab Name\".");
                    }

                    sheet = worksheets.FirstOrDefault(w => string.Equals(w.Name, worksheetName, StringComparison.OrdinalIgnoreCase));
                    if (sheet == null)
                    {
                        var names = string.Join(", ", worksheets.Select(w => w.Name));
                        throw new UserFriendlyException($"No sheet named \"{worksheetName}\" was found. Available sheets: {names}.");
                    }
                }
                else
                {
                    sheet = worksheets.FirstOrDefault();
                }

                if (sheet?.Dimension == null)
                {
                    throw new UserFriendlyException("The uploaded file has no readable worksheet data.");
                }

                ValidateWorksheet(sheet);
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read uploaded Excel file for worksheet validation.");
                throw new UserFriendlyException("The uploaded file could not be read. Make sure it's a valid Excel (.xlsx) file.");
            }
        }

        private void ValidateWorksheet(ExcelWorksheet sheet)
        {
            int lastCol = sheet.Dimension.End.Column;
            int lastRow = sheet.Dimension.End.Row;

            var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int c = 1; c <= lastCol; c++)
            {
                var header = Convert.ToString(sheet.Cells[1, c].Value, CultureInfo.InvariantCulture)?.Trim();
                if (!string.IsNullOrEmpty(header) && !headerIndex.ContainsKey(header))
                {
                    headerIndex[header] = c;
                }
            }

            var requiredColumns = new[] { "Measure" }.Concat(NumericColumns).ToArray();
            var missingColumns = requiredColumns.Where(c => !headerIndex.ContainsKey(c)).ToList();
            if (missingColumns.Count > 0)
            {
                throw new UserFriendlyException($"The uploaded file is missing required column(s): {string.Join(", ", missingColumns)}.");
            }

            var validMeasures = new HashSet<string>(MeasureNames, StringComparer.OrdinalIgnoreCase);
            var seenMeasures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var errors = new List<string>();
            int dataRows = 0;

            for (int r = 2; r <= lastRow; r++)
            {
                var measure = Convert.ToString(sheet.Cells[r, headerIndex["Measure"]].Value, CultureInfo.InvariantCulture)?.Trim();
                if (string.IsNullOrEmpty(measure))
                {
                    continue; // skip fully-blank trailing rows (e.g. unused rows left over from the template)
                }
                dataRows++;

                var rowIssues = new List<string>();

                if (!validMeasures.Contains(measure))
                {
                    rowIssues.Add($"\"{measure}\" is not a recognized measure");
                }
                else if (!seenMeasures.Add(measure))
                {
                    rowIssues.Add($"duplicate Measure \"{measure}\" (already appears earlier in this file)");
                }

                // Every numeric column is NOT NULL in tbl_dss_RawImportedScoreParameters, and the bulk
                // insert writes exactly what's in the cell — it does NOT fall back to the column's DB
                // default for a blank cell, it inserts DBNull and the bulk copy fails outright. So a
                // blank cell here must be rejected as a validation error, not silently allowed through.
                double? min = null, max = null;
                foreach (var col in NumericColumns)
                {
                    var cellValue = sheet.Cells[r, headerIndex[col]].Value;
                    if (cellValue == null || (cellValue is string s && string.IsNullOrWhiteSpace(s)))
                    {
                        rowIssues.Add($"{col} is required");
                        continue;
                    }

                    if (!TryGetNumber(cellValue, out var value))
                    {
                        rowIssues.Add($"{col} value \"{cellValue}\" is not a valid number");
                        continue;
                    }

                    if (col == "Min") min = value;
                    if (col == "Max") max = value;
                }

                if (min.HasValue && max.HasValue && min.Value > max.Value)
                {
                    rowIssues.Add($"Min ({min}) must be less than or equal to Max ({max})");
                }

                if (rowIssues.Count > 0)
                {
                    errors.Add($"Row {r} (\"{measure}\"): {string.Join("; ", rowIssues)}.");
                }
            }

            if (dataRows == 0)
            {
                throw new UserFriendlyException("The uploaded file has no data rows.");
            }

            if (errors.Count > 0)
            {
                const int maxErrorsToShow = 20;
                var shown = errors.Take(maxErrorsToShow).ToList();
                var suffix = errors.Count > maxErrorsToShow ? $"\n(and {errors.Count - maxErrorsToShow} more issue(s))" : "";
                throw new UserFriendlyException("Import failed validation:\n" + string.Join("\n", shown) + suffix);
            }
        }

        private static bool TryGetNumber(object cellValue, out double value)
        {
            switch (cellValue)
            {
                case double d: value = d; return true;
                case int i: value = i; return true;
                case decimal m: value = (double)m; return true;
                default:
                    return double.TryParse(Convert.ToString(cellValue, CultureInfo.InvariantCulture),
                        NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
            }
        }

        private static PoolScoreParameterDto ToDto(PoolScoreParameterEntity e) => new PoolScoreParameterDto
        {
            PoolId = e.PoolId,
            Measure = e.Measure,
            Min = e.Min,
            ScoreAtMin = e.ScoreAtMin,
            Max = e.Max,
            ScoreAtMax = e.ScoreAtMax,
            Slope = e.Slope,
            SafetyWeight = e.SafetyWeight,
            MobilityWeight = e.MobilityWeight,
            EquityWeight = e.EquityWeight,
            EnvWeight = e.EnvWeight,
            CondWeight = e.CondWeight,
            CreatedBy = e.CreatedBy,
            CreatedAt = e.CreatedAt
        };
    }
}
