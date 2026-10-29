using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectBuilder.Helpers
{
    // Pure helpers for the Budget Constraints criteria strings, ported verbatim from the legacy
    // EditScenarioController. A criteria string looks like:
    //   DISTRICT='11' AND INTERSTATE='Y' AND ASSETTYPE='B' AND CATEGORY='Preservation'
    public static class BudgetCriteriaParser
    {
        public const string CategoryAttributeName = "CATEGORY";

        public static (string BudgetDistrict, string IsInterstate, string AssetType, string AttributeName, string AttributeValue)
            Parse(string criteriaValue, string defaultAttributeName)
        {
            string budgetDistrict = null;
            string isInterstate = "0";
            string assetType = null;
            string attributeName = defaultAttributeName;
            string attributeValue = null;

            if (string.IsNullOrWhiteSpace(criteriaValue))
            {
                return (budgetDistrict, isInterstate, assetType, attributeName, attributeValue);
            }

            string attributeCondition = null;
            foreach (var condition in GetConditions(criteriaValue))
            {
                if (IsField(condition, "DISTRICT"))
                {
                    budgetDistrict = GetQuotedValue(condition);
                }
                else if (IsField(condition, "INTERSTATE"))
                {
                    var v = GetQuotedValue(condition)?.ToUpperInvariant();
                    isInterstate = v == "Y" ? "1" : v == "N" ? "0" : v == "*" ? "*" : isInterstate;
                }
                else if (IsField(condition, "ASSETTYPE"))
                {
                    var v = GetQuotedValue(condition)?.ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(v)) assetType = v;
                }
                else if (attributeCondition == null)
                {
                    attributeCondition = condition;
                }
            }

            if (!string.IsNullOrWhiteSpace(attributeCondition))
            {
                attributeName = GetFieldName(attributeCondition);
                attributeValue = GetQuotedValue(attributeCondition);
            }

            return (budgetDistrict, isInterstate, assetType, attributeName, attributeValue);
        }

        public static List<string> GetConditions(string criteriaValue)
        {
            if (string.IsNullOrWhiteSpace(criteriaValue))
            {
                return new List<string>();
            }

            return criteriaValue.Split(new[] { "AND" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();
        }

        public static bool IsField(string condition, string fieldName)
            => string.Equals(GetFieldName(condition), fieldName, StringComparison.OrdinalIgnoreCase);

        public static string GetFieldName(string condition)
        {
            var trimmed = condition?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            var equalsIndex = trimmed.IndexOf('=');
            var fieldName = equalsIndex >= 0 ? trimmed[..equalsIndex].Trim() : trimmed;

            if (fieldName.StartsWith("[") && fieldName.Contains("]", StringComparison.Ordinal))
            {
                fieldName = fieldName.Substring(1, fieldName.IndexOf("]", StringComparison.Ordinal) - 1);
            }

            return fieldName.Trim();
        }

        public static string GetQuotedValue(string condition)
        {
            var match = Regex.Match(condition ?? string.Empty, @"'([^']*)'");
            return match.Success ? match.Groups[1].Value : null;
        }

        public static bool TryValidate(string criteriaValue, out string errorMessage)
        {
            var conditions = GetConditions(criteriaValue);
            var fieldNames = conditions
                .Select(GetFieldName)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();

            if (conditions.Count != 4 || fieldNames.Count != 4)
            {
                errorMessage = $"must contain exactly 4 conditions: DISTRICT, INTERSTATE, ASSETTYPE, and {CategoryAttributeName}.";
                return false;
            }

            var coreFields = new[] { "DISTRICT", "INTERSTATE", "ASSETTYPE" };
            var hasAllCore = coreFields.All(req =>
                fieldNames.Count(f => string.Equals(f, req, StringComparison.OrdinalIgnoreCase)) == 1);

            if (!hasAllCore)
            {
                errorMessage = "must include exactly one DISTRICT, one INTERSTATE, and one ASSETTYPE condition.";
                return false;
            }

            var attributeFields = fieldNames
                .Where(f => !coreFields.Contains(f, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (attributeFields.Count != 1 || !string.Equals(attributeFields[0], CategoryAttributeName, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"must include {CategoryAttributeName} as the only attribute condition in addition to DISTRICT, INTERSTATE, and ASSETTYPE.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public static string NormalizeFieldNames(string criteriaValue)
        {
            if (string.IsNullOrWhiteSpace(criteriaValue))
            {
                return string.Empty;
            }

            var normalized = criteriaValue.Split(new[] { "AND" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(condition =>
                {
                    var equalsIndex = condition.IndexOf('=');
                    var fieldName = GetFieldName(condition).ToUpperInvariant();
                    return equalsIndex < 0 ? fieldName : $"{fieldName}{condition[equalsIndex..].TrimStart()}";
                });

            return string.Join(" AND ", normalized);
        }

        public static int GetAttributePriority(string attributeName)
            => attributeName?.Trim().ToUpperInvariant() switch
            {
                "CATEGORY" => 0,
                "BUDGET" => 1,
                "PROGRAM" => 2,
                _ => 10
            };

        public static string BuildColumnName(string district, string isInterstate, string assetType, string attributeName, string attributeValue)
        {
            var assetLabel = assetType switch { "B" => "Bridge", "P" => "Pavement", _ => assetType };
            var interstateLabel = string.Equals(isInterstate, "Y", StringComparison.OrdinalIgnoreCase) ? "Interstate" : "Non-Interstate";
            var attributeLabel = string.Equals(attributeName, "CATEGORY", StringComparison.OrdinalIgnoreCase) ? attributeValue : $"{attributeName} {attributeValue}";
            return $"District {district} {assetLabel} {interstateLabel} {attributeLabel}";
        }

        public static int GetAssetTypeSortOrder(string assetType)
            => (string.IsNullOrWhiteSpace(assetType) ? string.Empty : assetType.Trim().ToUpperInvariant()) switch
            {
                "*" => 0,
                "B" => 1,
                "P" => 2,
                _ => 3
            };

        public static string NormalizeAssetType(string assetType)
            => string.IsNullOrWhiteSpace(assetType) ? string.Empty : assetType.Trim().ToUpperInvariant();
    }
}
