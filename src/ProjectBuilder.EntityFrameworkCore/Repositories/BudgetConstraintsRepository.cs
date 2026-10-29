using Microsoft.EntityFrameworkCore;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Helpers;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    // Ported from EditScenarioController budget actions. Uses the directly-injected DbContext
    // throughout (single connection) to avoid the UoW cross-connection lock timeout.
    public class BudgetConstraintsRepository : EfCoreRepository<ProjectBuilderDbContext, BudgetConstraint, int>, IBudgetConstraintsRepository
    {
        private readonly ProjectBuilderDbContext _context;

        public BudgetConstraintsRepository(IDbContextProvider<ProjectBuilderDbContext> provider, ProjectBuilderDbContext context)
            : base(provider)
        {
            _context = context;
        }

        public async Task<Guid?> GetScenarioLibraryIdAsync(int scenId)
            => await _context.Scenarios.Where(s => s.Id == scenId).Select(s => s.LibraryId).FirstOrDefaultAsync();

        public async Task<(int? FirstYear, int? LastYear)> GetScenarioYearsAsync(int scenId)
        {
            var first = await _context.ScenarioParameters.Where(p => p.ScenarioId == scenId && p.Id == "YFST").Select(p => p.ParameterValue).FirstOrDefaultAsync();
            var last = await _context.ScenarioParameters.Where(p => p.ScenarioId == scenId && p.Id == "YLST").Select(p => p.ParameterValue).FirstOrDefaultAsync();
            return (first.HasValue ? (int)first.Value : (int?)null, last.HasValue ? (int)last.Value : (int?)null);
        }

        public async Task<List<BudgetConstraintColumn>> GetColumnsAsync(int scenId)
            => await _context.BudgetConstraintsColumns.Where(c => c.ScenId == scenId).OrderBy(c => c.Id).ToListAsync();

        public async Task<List<BudgetConstraint>> GetConstraintsAsync(int scenId)
            => await _context.BudgetConstraints.Where(c => c.ScenId == scenId).ToListAsync();

        public async Task<List<BudgetConstraintCriteria>> GetCriteriaListAsync(int scenId)
            => await _context.BudgetConstraintsCriteria.Where(c => c.ScenId == scenId).ToListAsync();

        public async Task<List<FilteringAttribute>> GetFilteringAttributesAsync(Guid libraryId)
            => await _context.FilteringAttributes.Where(v => v.LibraryId == libraryId).ToListAsync();

        public async Task<(int Bridges, int Pavements)> GetTreatmentCountsAsync(Guid libraryId)
        {
            var bridges = await _context.CustomTreatments.CountAsync(t => t.LibraryId == libraryId && t.AssetType == "B");
            var pavements = await _context.CustomTreatments.CountAsync(t => t.LibraryId == libraryId && t.AssetType == "P");
            return (bridges, pavements);
        }

        // Any scenario whose budget constraints change needs a re-run before results are trusted.
        // Mirrors TreatmentRepository.UpdateScenarioNotes, but budget writes already have the
        // scenario id directly rather than a library id.
        private async Task SetScenarioStaleAsync(int scenId)
        {
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenId);
            if (scenario == null) return;

            scenario.Stale = true;
            scenario.Notes = "Budget constraints have been modified. Please perform a scenario re-run";
        }

        public async Task UpdateBudgetCellsAsync(int scenId, List<BudgetCellInput> cells)
        {
            var libraryId = await GetScenarioLibraryIdAsync(scenId);
            var filteringAttributes = libraryId.HasValue
                ? await _context.FilteringAttributes.Where(v => v.LibraryId == libraryId.Value).ToListAsync()
                : new List<FilteringAttribute>();

            foreach (var cell in cells)
            {
                var constraint = await _context.BudgetConstraints
                    .FirstOrDefaultAsync(c => c.ScenId == scenId && c.RowNumber == cell.Row && c.BCCId == cell.Col);

                if (constraint == null)
                {
                    _context.BudgetConstraints.Add(new BudgetConstraint { ScenId = scenId, RowNumber = cell.Row, BCCId = cell.Col, BudgetValue = cell.Value });
                }
                else
                {
                    constraint.BudgetValue = cell.Value;
                }

                var column = await _context.BudgetConstraintsColumns.FirstOrDefaultAsync(c => c.Id == cell.Col && c.ScenId == scenId);
                if (column == null) continue;

                // Resolve the year for this row from its Year/YearWork column.
                var yearConstraint = await (from bc in _context.BudgetConstraints
                                            join col in _context.BudgetConstraintsColumns on bc.BCCId equals col.Id
                                            where bc.ScenId == scenId && bc.RowNumber == cell.Row
                                                && (col.ColumnName == "Year" || col.ColumnName == "YearWork")
                                            select bc).FirstOrDefaultAsync();
                int year = yearConstraint != null ? Convert.ToInt32(yearConstraint.BudgetValue) : 0;

                var criteriaRow = await _context.BudgetConstraintsCriteria.FirstOrDefaultAsync(c => c.BCCId == cell.Col && c.ScenId == scenId);
                var parsed = BudgetCriteriaParser.Parse(criteriaRow?.Criteria, column.ColumnName);
                var attr = filteringAttributes.FirstOrDefault(f => f.AttributeName.Equals(parsed.AttributeName, StringComparison.OrdinalIgnoreCase));
                if (attr == null) continue;

                var extended = await _context.ScenarioBudgetWithAttributes.FirstOrDefaultAsync(e =>
                    e.YearWork == year && e.ScenId == scenId && e.District == parsed.BudgetDistrict
                    && e.IsInterstate == parsed.IsInterstate && e.AssetType == parsed.AssetType
                    && e.AttributeName == parsed.AttributeName && e.AttributeValue == parsed.AttributeValue);

                if (extended == null)
                {
                    _context.ScenarioBudgetWithAttributes.Add(new ScenarioBudgetWithAttribute
                    {
                        YearWork = year,
                        ScenId = scenId,
                        District = parsed.BudgetDistrict,
                        IsInterstate = parsed.IsInterstate,
                        AssetType = parsed.AssetType,
                        AttributeName = parsed.AttributeName,
                        AttributeValue = parsed.AttributeValue,
                        Budget = cell.Value
                    });
                }
                else
                {
                    extended.Budget = cell.Value;
                }
            }

            await SetScenarioStaleAsync(scenId);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CreateCriteriaAsync(int scenId, string columnName, string criteria)
        {
            var column = new BudgetConstraintColumn { ScenId = scenId, ColumnName = columnName };
            _context.BudgetConstraintsColumns.Add(column);
            await _context.SaveChangesAsync();

            _context.BudgetConstraintsCriteria.Add(new BudgetConstraintCriteria { ScenId = scenId, BCCId = column.Id, Criteria = criteria });

            // Rows may already exist (created via Add Row before this column existed) -- seed this
            // new column at 0 for every existing row, otherwise GetBudgetConstraintsAsync builds a
            // cell with no BudgetConstraint record at all for it (RowNumber/BCCId both null), which
            // the grid can't render as an editable/savable cell.
            var existingRowNumbers = await _context.BudgetConstraints.Where(c => c.ScenId == scenId).Select(c => c.RowNumber).Distinct().ToListAsync();
            foreach (var rowNumber in existingRowNumbers)
            {
                _context.BudgetConstraints.Add(new BudgetConstraint { ScenId = scenId, RowNumber = rowNumber, BCCId = column.Id, BudgetValue = 0 });
            }

            await SetScenarioStaleAsync(scenId);
            await _context.SaveChangesAsync();
            return column.Id;
        }

        public async Task UpdateCriteriaAsync(int columnId, int scenId, string columnName, string criteria)
        {
            var column = await _context.BudgetConstraintsColumns.FirstOrDefaultAsync(c => c.Id == columnId && c.ScenId == scenId);
            if (column == null) return;
            column.ColumnName = columnName;

            var existing = await _context.BudgetConstraintsCriteria.FirstOrDefaultAsync(c => c.BCCId == columnId && c.ScenId == scenId);
            if (existing == null)
            {
                _context.BudgetConstraintsCriteria.Add(new BudgetConstraintCriteria { ScenId = scenId, BCCId = columnId, Criteria = criteria });
            }
            else
            {
                existing.Criteria = criteria;
            }
            await SetScenarioStaleAsync(scenId);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCriteriaAsync(int id)
        {
            var criteria = await _context.BudgetConstraintsCriteria.FirstOrDefaultAsync(c => c.Id == id);
            if (criteria == null) return;

            int scenId = criteria.ScenId;
            int bccId = criteria.BCCId;
            var parsed = BudgetCriteriaParser.Parse(criteria.Criteria, string.Empty);

            var attributesToDelete = await _context.ScenarioBudgetWithAttributes
                .Where(a => a.ScenId == scenId
                    && (parsed.BudgetDistrict == null || a.District == parsed.BudgetDistrict)
                    && (parsed.IsInterstate == "*" || a.IsInterstate == parsed.IsInterstate)
                    && (parsed.AssetType == null || a.AssetType == parsed.AssetType)
                    && a.AttributeName == parsed.AttributeName
                    && a.AttributeValue == parsed.AttributeValue)
                .ToListAsync();
            if (attributesToDelete.Count > 0) _context.ScenarioBudgetWithAttributes.RemoveRange(attributesToDelete);

            // FK_BudgetConstraintsCriteria_Columns / BudgetConstraints → Columns: remove dependents
            // and SaveChanges before deleting the column (EF has no navigation graph for these
            // tables, so a single SaveChanges can delete the column first and violate the FK).
            _context.BudgetConstraintsCriteria.Remove(criteria);

            var relatedBudgets = await _context.BudgetConstraints.Where(b => b.BCCId == bccId).ToListAsync();
            if (relatedBudgets.Count > 0) _context.BudgetConstraints.RemoveRange(relatedBudgets);

            await SetScenarioStaleAsync(scenId);
            await _context.SaveChangesAsync();

            var relatedColumns = await _context.BudgetConstraintsColumns.Where(c => c.Id == bccId).ToListAsync();
            if (relatedColumns.Count > 0) _context.BudgetConstraintsColumns.RemoveRange(relatedColumns);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteRowAsync(int scenId, int yearWork, int rowNumber)
        {
            var attrs = await _context.ScenarioBudgetWithAttributes.Where(a => a.ScenId == scenId && a.YearWork == yearWork).ToListAsync();
            if (attrs.Count > 0) _context.ScenarioBudgetWithAttributes.RemoveRange(attrs);

            var constraints = await _context.BudgetConstraints.Where(a => a.ScenId == scenId && a.RowNumber == rowNumber).ToListAsync();
            if (constraints.Count > 0) _context.BudgetConstraints.RemoveRange(constraints);

            await SetScenarioStaleAsync(scenId);
            await _context.SaveChangesAsync();
        }

        // Adds a new budget row (year) so a scenario with no import history can still build a
        // budget entirely by hand. The grid/GetBudgetConstraintsAsync render the row's "Year"
        // column read-only (not through the normal per-cell edit path), identified by column name
        // ("Year"/"YearWork", matching what ImportBudgetConstraintsAsync already requires by name)
        // -- NOT by column order, since a real criteria column can easily be created before any
        // row/Year column exists (e.g. via Edit Budgets before ever clicking Add Row). If no such
        // column exists yet for this scenario, it's created here automatically, with no criteria
        // row attached (same as Import, which never requires criteria for the Year/YearWork
        // column). Its value is set directly rather than through the criteria/attribute-matching
        // logic UpdateBudgetCellsAsync uses for real dollar cells -- mirroring how
        // SaveImportAsync's extendedRows build explicitly skips the Year/YearWork column instead
        // of matching it against filtering attributes. Every other existing column is seeded at 0
        // so the row has a real BudgetConstraint record for every column immediately (same as an
        // imported row), instead of leaving cells with no record at all, which the grid can't
        // render as an editable/savable cell.
        public async Task AddRowAsync(int scenId, int year)
        {
            var columns = await _context.BudgetConstraintsColumns.Where(c => c.ScenId == scenId).ToListAsync();

            var yearColumn = columns.FirstOrDefault(c =>
                c.ColumnName.Equals("Year", StringComparison.OrdinalIgnoreCase) || c.ColumnName.Equals("YearWork", StringComparison.OrdinalIgnoreCase));
            if (yearColumn == null)
            {
                yearColumn = new BudgetConstraintColumn { ScenId = scenId, ColumnName = "Year" };
                _context.BudgetConstraintsColumns.Add(yearColumn);
                await _context.SaveChangesAsync();
            }

            var existingRowNumbers = await _context.BudgetConstraints.Where(c => c.ScenId == scenId).Select(c => c.RowNumber).ToListAsync();
            int nextRow = existingRowNumbers.Count == 0 ? 1 : existingRowNumbers.Max() + 1;

            _context.BudgetConstraints.Add(new BudgetConstraint { ScenId = scenId, RowNumber = nextRow, BCCId = yearColumn.Id, BudgetValue = year });
            foreach (var col in columns.Where(c => c.Id != yearColumn.Id))
            {
                _context.BudgetConstraints.Add(new BudgetConstraint { ScenId = scenId, RowNumber = nextRow, BCCId = col.Id, BudgetValue = 0 });
            }

            await SetScenarioStaleAsync(scenId);
            await _context.SaveChangesAsync();
        }

        // Full reset of a scenario's budget setup: values, criteria, and columns. Keeping
        // criteria/columns around after a values-only clear would be pointless — Import is the
        // only way to add rows back, and SaveImportAsync below replaces criteria/columns on every
        // import anyway — so this matches what actually happens rather than a partial clear.
        public async Task ClearBudgetsAsync(int scenId)
        {
            var attrs = await _context.ScenarioBudgetWithAttributes.Where(a => a.ScenId == scenId).ToListAsync();
            if (attrs.Count > 0) _context.ScenarioBudgetWithAttributes.RemoveRange(attrs);

            var constraints = await _context.BudgetConstraints.Where(a => a.ScenId == scenId).ToListAsync();
            if (constraints.Count > 0) _context.BudgetConstraints.RemoveRange(constraints);

            var criteria = await _context.BudgetConstraintsCriteria.Where(c => c.ScenId == scenId).ToListAsync();
            if (criteria.Count > 0) _context.BudgetConstraintsCriteria.RemoveRange(criteria);
            await SetScenarioStaleAsync(scenId);
            await _context.SaveChangesAsync();

            var columns = await _context.BudgetConstraintsColumns.Where(c => c.ScenId == scenId).ToListAsync();
            if (columns.Count > 0) _context.BudgetConstraintsColumns.RemoveRange(columns);
            await _context.SaveChangesAsync();
        }

        public Task<List<BudgetTemplateColumn>> GetTemplateColumnsAsync(int scenId)
        {
            // The pb-abp ScenarioTreatments view does not expose ImportTimeGeneratedId, so the
            // cargo-derived template columns aren't available; return the legacy default columns.
            // (Import reads the actual uploaded sheet headers, so this does not constrain import.)
            var defaults = new[]
            {
                "District 11 Preservation", "District 03 Interstate Preservation", "District 08 Interstate Preservation",
                "District 09 Preservation", "District 12 Interstate Preservation", "District 05 Preservation",
                "District 09 Interstate Preservation", "District 08 Preservation", "District 11 Maintenance",
                "District 03 Interstate Maintenance"
            };

            var result = defaults.Select(columnName =>
            {
                var parts = columnName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return new BudgetTemplateColumn
                {
                    ColumnName = columnName,
                    District = parts.Length > 1 ? parts[1] : "00",
                    IsInterstate = columnName.Contains("Interstate", StringComparison.OrdinalIgnoreCase) ? "Y" : "N",
                    AssetType = "B",
                    AttributeName = "CATEGORY",
                    AttributeValue = columnName.Contains("Maintenance", StringComparison.OrdinalIgnoreCase) ? "Maintenance" : "Preservation"
                };
            }).ToList();

            return Task.FromResult(result);
        }

        public async Task SaveImportAsync(int scenId, List<string> columnNames, List<BudgetImportConstraint> constraints,
            List<BudgetImportCriteria> criteria, List<ScenarioBudgetWithAttribute> withAttributes)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.BudgetConstraintsCriteria.RemoveRange(_context.BudgetConstraintsCriteria.Where(c => c.ScenId == scenId));
                _context.BudgetConstraints.RemoveRange(_context.BudgetConstraints.Where(c => c.ScenId == scenId));
                _context.ScenarioBudgetWithAttributes.RemoveRange(_context.ScenarioBudgetWithAttributes.Where(c => c.ScenId == scenId));
                await _context.SaveChangesAsync();

                _context.BudgetConstraintsColumns.RemoveRange(_context.BudgetConstraintsColumns.Where(c => c.ScenId == scenId));
                await _context.SaveChangesAsync();

                var columns = columnNames.Select(name => new BudgetConstraintColumn { ScenId = scenId, ColumnName = name }).ToList();
                _context.BudgetConstraintsColumns.AddRange(columns);
                await _context.SaveChangesAsync();

                var columnMap = columns.ToDictionary(c => c.ColumnName, c => c.Id, StringComparer.OrdinalIgnoreCase);

                _context.BudgetConstraints.AddRange(constraints.Select(item => new BudgetConstraint
                {
                    ScenId = scenId,
                    BCCId = columnMap[item.ColumnName],
                    BudgetValue = item.BudgetValue,
                    RowNumber = item.RowNumber
                }));

                _context.BudgetConstraintsCriteria.AddRange(criteria.Select(item => new BudgetConstraintCriteria
                {
                    ScenId = scenId,
                    BCCId = columnMap[item.ColumnName],
                    Criteria = item.CriteriaValue
                }));

                if (withAttributes != null && withAttributes.Count > 0)
                {
                    _context.ScenarioBudgetWithAttributes.AddRange(withAttributes);
                }

                await SetScenarioStaleAsync(scenId);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
