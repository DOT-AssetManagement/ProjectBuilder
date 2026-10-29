using ProjectBuilder.Entities;
using ProjectBuilder.EventsArgs;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IRunScenarioRepository : IRepository<Scenario, int>
    {
        int ScenarioId { get; set; }
        IRepository<ScenarioParamater, string> ScenarioParametersRepo { get; }
        Task<List<ProjectBuilder.Entities.Scenario>> GetScenariosListAsync();
        Task<ScenarioEventArgs> CreateScenarioAsync(
            string scenarioName,
            Guid libraryId,
            int firstYear,
            int lastYear,
            int setDefault,
            string createdBy = "",
            bool isShared = false);
        Task<ProjectBuilder.Entities.Scenario> GetScenarioByPoolId(Guid poolId);
        Task UpdateScenarioByPoolIdAsync(Guid poolId, string name, string notes);

        Task<ScenarioEventArgs> RunScenarioAsync(int scenarioId, string name, bool doCleanup);
        Task<ScenarioEventArgs> DeleteScenarioAsync(int scenarioId);

        Task<MapsResultModel> ExportScenarioResultsToJsonAsync(
            int scenarioId,
            bool indent,
            int? district = null,
            int? cnty = null,
            int? route = null,
            string section = null,
            string appliedTreatment = null,
            int? selectedYear = null);
        Task<ProjectBuilder.Entities.Scenario> FindScenarioByIdAsync(int? scenid);
    }

}
