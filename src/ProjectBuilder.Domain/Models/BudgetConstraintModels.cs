namespace ProjectBuilder.Models
{
    // A single edited budget cell (row x column) from the grid bulk-save.
    public class BudgetCellInput
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public decimal Value { get; set; }
    }

    // One imported budget value linked to a column name + row.
    public class BudgetImportConstraint
    {
        public string ColumnName { get; set; }
        public decimal BudgetValue { get; set; }
        public int RowNumber { get; set; }
    }

    // One imported criteria row (budget column name -> criteria string).
    public class BudgetImportCriteria
    {
        public string ColumnName { get; set; }
        public string CriteriaValue { get; set; }
    }

    // A descriptor used to build the downloadable Excel template's columns + criteria.
    public class BudgetTemplateColumn
    {
        public string ColumnName { get; set; }
        public string District { get; set; }
        public string IsInterstate { get; set; } = "N";
        public string AssetType { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }
}
