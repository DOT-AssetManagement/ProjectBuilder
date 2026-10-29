using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IScenarioParametersRepository : IRepository<ScenarioParamater>
    {
        Task UpdateAsync(ScenarioParamater newValue, params string[] properties);
        Task<ScenarioParamater> FindScenarioParameter(int? scenid, string parameterid);
        Task<bool> DeleteScenarioParameterAsync(int scenid, string scenparamid);

        // Parameters page (Analysis Method tab groupings + Treatment Shift tab).
        Task<List<ScenarioParamater>> GetByScenarioAsync(int scenid);
        Task<List<TreatmentShiftRow>> GetTreatmentShiftsAsync(int scenid);
        Task SaveParametersPageAsync(int scenid, Dictionary<string, double?> parameterValues, List<TreatmentShiftRow> shifts, string notes);
    }
}
