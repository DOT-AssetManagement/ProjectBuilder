using ProjectBuilder.Models.ProjectTreatment.Dtos;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Models.ScenarioBudgetConstraint.Dtos;
using ProjectBuilder.Models.ScenarioFilter.Dtos;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Scenario
{
    public interface IScenarioAppService : IApplicationService
    {
        Task<ScenarioDto> GetScenariosAsync(string searchString = "");
        Task<List<ScenarioDto>> GetAllAsync(string searchString = "");
        Task<ScenarioDto> GetAsync(Guid id);
        Task<ScenarioDto> CreateNewScenarioAsync(CreateScenarioDto scenarioDto);
        Task<bool> CheckScenarioNameAsync(string attemptedName);
        Task<bool> CheckScenarioDeletionAsync(int? scenId);
        Task<string> RunScenarioAsync(int? scenarioId);
        Task<bool> DeleteScenarioAsync(int scenarioId);
        Task<ScenarioDto> GetScenarioByPoolId(Guid poolId);
        Task UpdateScenarioByPoolIdAsync(Guid poolId, string name, string description);
        Task<ScenarioSettingsDto> GetScenarioSettingsAsync(int scenid);
        Task UpdateScenarioSettingsAsync(int scenid, ScenarioSettingsDto input);

        // ── Parameters page ──
        Task<List<ScenarioParameterDto>> GetScenarioParametersAsync(int scenid);
        Task<List<TreatmentShiftDto>> GetTreatmentShiftsAsync(int scenid);
        Task SaveParametersAsync(int scenid, SaveParametersInputDto input);

        // ── Analysis Method page ──
        Task<AnalysisMethodDto> GetAnalysisMethodAsync(int scenid);
        Task<List<string>> GetAttributeValuesAsync(int scenid, string assetType, string attributeName);
        Task SaveCriteriaAsync(int scenid, List<ScenarioFilterDto> filters, string notes);
        Task<int> GetMatchedTreatmentCountAsync(int scenid, List<ScenarioFilterDto> filters);

        // ── Budget Constraints page ──
        Task<BudgetConstraintsGridDto> GetBudgetConstraintsAsync(int scenid);
        Task UpdateBudgetConstraintsAsync(int scenid, List<BudgetCellDto> cells);
        Task<int> CreateBudgetCriteriaAsync(int scenid, string columnName, string criteria);
        Task UpdateBudgetCriteriaAsync(int columnId, int scenid, string columnName, string criteria);
        Task DeleteBudgetCriteriaAsync(int id);
        Task DeleteBudgetRowAsync(int scenid, int yearWork, int rowNumber);
        Task AddBudgetRowAsync(int scenid, int year);
        Task ClearBudgetConstraintsAsync(int scenid);
        Task<(byte[] Content, string FileName)> ExportBudgetConstraintsAsync(int scenid);
        Task<(byte[] Content, string FileName)> DownloadBudgetTemplateAsync(int scenid);
        Task ImportBudgetConstraintsAsync(int scenid, byte[] xlsxBytes);
    }
}
