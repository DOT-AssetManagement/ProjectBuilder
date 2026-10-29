using System.Collections.Generic;
using ProjectBuilder.Models.ScenarioFilter.Dtos;

namespace ProjectBuilder.Models.ScenarioBudgetConstraint.Dtos
{
    // Grid + criteria model for the Budget Constraints page.
    public class BudgetConstraintsGridDto
    {
        public int ScenarioId { get; set; }
        public string ScenarioName { get; set; }
        public string ScenarioNotes { get; set; }
        public bool Locked { get; set; }
        public List<string> Headers { get; set; } = new();
        public List<List<BudgetCellViewDto>> Rows { get; set; } = new();
        public List<BudgetCriteriaDto> CriteriaList { get; set; } = new();
        public List<FilteringAttributeDto> FilteringAttributes { get; set; } = new();
        public int BridgesCount { get; set; }
        public int PavementsCount { get; set; }
    }

    // One grid cell (with the identifiers needed to post an edit back).
    public class BudgetCellViewDto
    {
        public int? RowNumber { get; set; }
        public int? BCCId { get; set; }
        public decimal? Value { get; set; }
    }

    // A criteria row shown in the Edit-Budgets modal.
    public class BudgetCriteriaDto
    {
        public int ColumnId { get; set; }
        public string ColumnName { get; set; }
        public string Criteria { get; set; }
        public int CriteriaId { get; set; }
    }

    // One edited cell posted from the grid bulk-save.
    public class BudgetCellDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public decimal Value { get; set; }
    }
}
