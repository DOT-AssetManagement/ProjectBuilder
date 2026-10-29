using Microsoft.EntityFrameworkCore;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    // Ported from EditScenarioController (AnalysisMethod / GetAttributeValues / SaveCriteria /
    // UpdateScenarioNotes). Uses the directly-injected DbContext throughout (single connection)
    // to avoid the UoW cross-connection lock timeout.
    public class AnalysisMethodRepository : EfCoreRepository<ProjectBuilderDbContext, ScenarioFilter>, IAnalysisMethodRepository
    {
        private const string ScenarioFilterModifiedNote = "Scenario treatments have been modified. To apply the changes, please perform a scenario re-run.";
        private readonly ProjectBuilderDbContext _context;

        public AnalysisMethodRepository(IDbContextProvider<ProjectBuilderDbContext> provider, ProjectBuilderDbContext context)
            : base(provider)
        {
            _context = context;
        }

        public async Task<Guid?> GetScenarioLibraryIdAsync(int scenId)
        {
            return await _context.Scenarios
                .Where(s => s.Id == scenId)
                .Select(s => s.LibraryId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<FilteringAttribute>> GetFilteringAttributesAsync(Guid libraryId)
        {
            var attributes = await _context.FilteringAttributes
                .Where(v => v.LibraryId == libraryId)
                .ToListAsync();

            return attributes
                .OrderBy(v => GetAssetTypeSortOrder(v.AssetType))
                .ThenBy(v => v.AttributeName)
                .ToList();
        }

        public async Task<List<ScenarioFilter>> GetScenarioFiltersAsync(int scenId)
        {
            return await _context.ScenarioFilters
                .Where(f => f.ScenId == scenId)
                .OrderBy(f => f.FilterNo)
                .ToListAsync();
        }

        public async Task<List<string>> GetAttributeValuesAsync(int scenId, string assetType, string attributeName)
        {
            var libraryId = await GetScenarioLibraryIdAsync(scenId);
            if (!libraryId.HasValue)
            {
                return new List<string>();
            }

            var normalizedAssetType = NormalizeAssetType(assetType);
            var normalizedAttributeName = attributeName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedAttributeName))
            {
                return new List<string>();
            }

            // 1) Distinct values straight off the library treatments (matching the attribute by
            //    property/column name).
            var propertyName = ResolveUserTreatmentPropertyName(normalizedAttributeName);
            if (propertyName != null)
            {
                var libraryValues = await _context.CustomTreatments
                    .Where(t => t.LibraryId == libraryId.Value
                        && (string.IsNullOrWhiteSpace(normalizedAssetType)
                            || normalizedAssetType == "*"
                            || t.AssetType == normalizedAssetType))
                    .Select(t => new { t.AssetType, Value = EF.Property<object>(t, propertyName) })
                    .ToListAsync();

                var ordered = OrderDistinctValues(libraryValues.Select(v => (v.AssetType, v.Value?.ToString())));
                if (ordered.Count > 0)
                {
                    return ordered;
                }
            }

            // 2) Fallback to the CargoAttributeOptions view.
            var cargoValues = await _context.CargoAttributeOptions
                .Where(v => v.LibraryId == libraryId.Value
                    && (string.IsNullOrWhiteSpace(normalizedAssetType)
                        || normalizedAssetType == "*"
                        || v.AssetType == normalizedAssetType)
                    && v.AttributeName == normalizedAttributeName)
                .Select(v => new { v.AssetType, v.AttributeValue })
                .ToListAsync();

            return OrderDistinctValues(cargoValues.Select(v => (v.AssetType, v.AttributeValue)));
        }

        public async Task SaveCriteriaAsync(int scenId, List<ScenarioFilter> filters, string criteriaNotes)
        {
            var existing = _context.ScenarioFilters.Where(f => f.ScenId == scenId);
            _context.ScenarioFilters.RemoveRange(existing);

            if (filters != null)
            {
                var toAdd = filters
                    .Where(f => !string.IsNullOrWhiteSpace(f.AttributeName))
                    .ToList();
                if (toAdd.Count > 0)
                {
                    await _context.ScenarioFilters.AddRangeAsync(toAdd);
                }
            }

            // Stale/Notes is the shared "why does this scenario need a re-run" status message
            // every scenario-modifying save action stomps with its own canned text — it is
            // NOT the user-facing Criteria Editor "Notes" field, which is CriteriaNotes below
            // and persists independently of that status bookkeeping.
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenId);
            if (scenario != null)
            {
                scenario.Stale = true;
                scenario.Notes = ScenarioFilterModifiedNote;
                scenario.CriteriaNotes = criteriaNotes ?? string.Empty;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateScenarioNotesAsync(int scenId, string notes)
        {
            await MarkScenarioStaleAsync(scenId, notes, save: true);
        }

        public async Task<int> GetMatchedTreatmentCountAsync(int scenId, List<ScenarioFilter> filters)
        {
            var libraryId = await GetScenarioLibraryIdAsync(scenId);
            if (!libraryId.HasValue)
            {
                return 0;
            }

            var activeFilters = (filters ?? new List<ScenarioFilter>())
                .Where(f => !string.IsNullOrWhiteSpace(f.AttributeName) && !string.IsNullOrWhiteSpace(f.Operation))
                .ToList();

            var treatments = await _context.CustomTreatments
                .AsNoTracking()
                .Where(t => t.LibraryId == libraryId.Value)
                .ToListAsync();

            if (treatments.Count == 0)
            {
                return 0;
            }

            if (activeFilters.Count == 0)
            {
                return treatments.Count;
            }

            var attributeNames = activeFilters
                .Select(f => f.AttributeName!.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            // Cargo-backed attributes (e.g. CATEGORY) live in tbl_lib_Cargo*, not on UserTreatment columns.
            var cargoRows = await (
                from d in _context.CargoData.AsNoTracking()
                join a in _context.CargoAttributes.AsNoTracking()
                    on new { d.LibraryId, d.AttributeNo, d.AssetType }
                    equals new { a.LibraryId, a.AttributeNo, a.AssetType }
                where d.LibraryId == libraryId.Value
                      && attributeNames.Contains(a.AttributeName.ToUpper())
                select new CargoAttributeValue
                {
                    TreatmentId = d.ImportTimeGeneratedGuid,
                    AttributeName = a.AttributeName.ToUpper(),
                    AttributeType = a.AttributeType,
                    TextValue = d.TextValue,
                    NumericValue = d.NumericValue
                }).ToListAsync();

            var cargoByTreatment = cargoRows
                .GroupBy(r => r.TreatmentId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.AttributeName)
                          .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase));

            var groups = activeFilters.GroupBy(f => f.FilterNo).ToList();
            var matched = 0;

            foreach (var treatment in treatments)
            {
                cargoByTreatment.TryGetValue(treatment.Id, out var treatmentCargo);
                if (TreatmentMatchesFilterGroups(treatment, treatmentCargo, groups))
                {
                    matched++;
                }
            }

            return matched;
        }

        private sealed class CargoAttributeValue
        {
            public Guid TreatmentId { get; set; }
            public string AttributeName { get; set; } = string.Empty;
            public string? AttributeType { get; set; }
            public string? TextValue { get; set; }
            public double? NumericValue { get; set; }
        }

        private static bool TreatmentMatchesFilterGroups(
            UserTreatment treatment,
            Dictionary<string, CargoAttributeValue>? treatmentCargo,
            List<IGrouping<int, ScenarioFilter>> groups)
        {
            // Groups are OR-ed; conditions within a group are AND-ed (same as buildCriteria / ScenarioFilter).
            foreach (var group in groups)
            {
                var groupOk = true;
                foreach (var filter in group)
                {
                    if (!EvaluateFilterAgainstTreatment(treatment, treatmentCargo, filter))
                    {
                        groupOk = false;
                        break;
                    }
                }

                if (groupOk)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool EvaluateFilterAgainstTreatment(
            UserTreatment treatment,
            Dictionary<string, CargoAttributeValue>? treatmentCargo,
            ScenarioFilter filter)
        {
            var assetType = filter.AssetType?.Trim();
            if (!string.IsNullOrWhiteSpace(assetType)
                && assetType != "*"
                && !string.Equals(treatment.AssetType, assetType, StringComparison.OrdinalIgnoreCase))
            {
                // Criterion targets a different asset type — does not constrain this treatment.
                return true;
            }

            var attributeName = filter.AttributeName!.Trim();
            var operation = filter.Operation!.Trim();
            var textValue = filter.TextValue;

            var propertyName = ResolveUserTreatmentPropertyName(attributeName);
            if (propertyName != null)
            {
                var property = typeof(UserTreatment).GetProperty(propertyName);
                var raw = property?.GetValue(treatment);
                return CompareValues(raw?.ToString(), textValue, operation, isNumericHint: IsNumericType(property?.PropertyType));
            }

            if (treatmentCargo != null
                && treatmentCargo.TryGetValue(attributeName.ToUpperInvariant(), out var cargo))
            {
                var isNumeric = string.Equals(cargo.AttributeType, "N", StringComparison.OrdinalIgnoreCase)
                                || cargo.NumericValue != null;
                var left = isNumeric && cargo.NumericValue != null
                    ? cargo.NumericValue.Value.ToString(CultureInfo.InvariantCulture)
                    : cargo.TextValue;
                return CompareValues(left, textValue, operation, isNumericHint: isNumeric);
            }

            // Attribute missing on this treatment → criterion fails (same as real ScenarioFilter).
            return false;
        }

        private static bool IsNumericType(Type? type)
        {
            if (type == null)
            {
                return false;
            }

            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(byte)
                || type == typeof(short)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);
        }

        private static bool CompareValues(string? leftRaw, string? rightRaw, string operation, bool isNumericHint)
        {
            if (leftRaw == null && rightRaw == null)
            {
                return false;
            }

            var left = leftRaw ?? string.Empty;
            var right = rightRaw ?? string.Empty;

            if (isNumericHint
                && double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out var leftNum)
                && double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out var rightNum))
            {
                return operation switch
                {
                    "=" => leftNum == rightNum,
                    "<>" => leftNum != rightNum,
                    ">" => leftNum > rightNum,
                    "<" => leftNum < rightNum,
                    ">=" => leftNum >= rightNum,
                    "<=" => leftNum <= rightNum,
                    "{}" => right.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                 .Any(part => double.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out var p) && p == leftNum),
                    _ => throw new UserFriendlyException($"Unsupported operator '{operation}'.")
                };
            }

            var cmp = string.Compare(left, right, StringComparison.Ordinal);
            return operation switch
            {
                "=" => cmp == 0,
                "<>" => cmp != 0,
                ">" => cmp > 0,
                "<" => cmp < 0,
                ">=" => cmp >= 0,
                "<=" => cmp <= 0,
                "{}" => right.ToUpperInvariant()
                             .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                             .Contains(left.ToUpperInvariant()),
                _ => throw new UserFriendlyException($"Unsupported operator '{operation}'.")
            };
        }

        private async Task MarkScenarioStaleAsync(int scenId, string notes, bool save)
        {
            var scenario = await _context.Scenarios.FirstOrDefaultAsync(s => s.Id == scenId);
            if (scenario != null)
            {
                scenario.Stale = true;
                scenario.Notes = notes;
                if (save)
                {
                    await _context.SaveChangesAsync();
                }
            }
        }

        private static string ResolveUserTreatmentPropertyName(string attributeName)
        {
            var normalized = attributeName?.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            var property = typeof(UserTreatment)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p =>
                    string.Equals(p.Name, normalized, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(p.GetCustomAttribute<ColumnAttribute>()?.Name, normalized, StringComparison.OrdinalIgnoreCase));

            return property?.Name;
        }

        private static List<string> OrderDistinctValues(IEnumerable<(string AssetType, string Value)> values)
        {
            // Distinct by display value first, then sort. Prefer numeric ascending when
            // values parse as numbers (e.g. 2, 10, 20 instead of text order 10, 2, 20);
            // non-numeric values sort after numbers, then by case-insensitive text.
            return values
                .Where(v => !string.IsNullOrWhiteSpace(v.Value))
                .GroupBy(v => v.Value)
                .Select(g => g.OrderBy(x => GetAssetTypeSortOrder(x.AssetType)).First().Value)
                .OrderBy(v => TryGetNumericSortKey(v, out _) ? 0 : 1)
                .ThenBy(v => TryGetNumericSortKey(v, out var n) ? n : 0m)
                .ThenBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool TryGetNumericSortKey(string value, out decimal number)
        {
            return decimal.TryParse(
                value?.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out number);
        }

        private static int GetAssetTypeSortOrder(string assetType)
        {
            return NormalizeAssetType(assetType) switch
            {
                "*" => 0,
                "B" => 1,
                "P" => 2,
                _ => 3
            };
        }

        private static string NormalizeAssetType(string assetType)
        {
            return string.IsNullOrWhiteSpace(assetType) ? string.Empty : assetType.Trim().ToUpperInvariant();
        }
    }
}
