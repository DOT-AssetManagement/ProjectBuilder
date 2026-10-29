using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Repositories
{
    public interface IDssScenarioRepository
    {
        Task<DssScenario> GetByPoolIdAsync(Guid poolId);
        Task<List<DssScenario>> GetByPoolIdsAsync(List<Guid> poolIds);
        Task<DssScenario> GetByIdAsync(Guid id);
        Task DeleteByPoolIdAsync(Guid poolId);
        Task<DssScenario> CreateForPoolAsync(Guid poolId, string scenName, string description, string createdBy);
        Task UpdateByPoolIdAsync(Guid poolId, string scenName, string description);
        Task<(bool Ok, string ErrorMessage)> RunScenarioAsync(Guid scenarioId);
    }
}
