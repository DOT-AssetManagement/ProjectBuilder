using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IBudgetConstraintsRepository : IRepository<BudgetConstraint, int>
    {
        Task<Guid?> GetScenarioLibraryIdAsync(int scenId);
        Task<(int? FirstYear, int? LastYear)> GetScenarioYearsAsync(int scenId);
        Task<List<BudgetConstraintColumn>> GetColumnsAsync(int scenId);
        Task<List<BudgetConstraint>> GetConstraintsAsync(int scenId);
        Task<List<BudgetConstraintCriteria>> GetCriteriaListAsync(int scenId);
        Task<List<FilteringAttribute>> GetFilteringAttributesAsync(Guid libraryId);
        Task<(int Bridges, int Pavements)> GetTreatmentCountsAsync(Guid libraryId);

        Task UpdateBudgetCellsAsync(int scenId, List<BudgetCellInput> cells);
        Task<int> CreateCriteriaAsync(int scenId, string columnName, string criteria);
        Task UpdateCriteriaAsync(int columnId, int scenId, string columnName, string criteria);
        Task DeleteCriteriaAsync(int id);
        Task DeleteRowAsync(int scenId, int yearWork, int rowNumber);
        Task AddRowAsync(int scenId, int year);
        Task ClearBudgetsAsync(int scenId);

        Task<List<BudgetTemplateColumn>> GetTemplateColumnsAsync(int scenId);
        Task SaveImportAsync(int scenId, List<string> columnNames, List<BudgetImportConstraint> constraints,
            List<BudgetImportCriteria> criteria, List<ScenarioBudgetWithAttribute> withAttributes);
    }
}
