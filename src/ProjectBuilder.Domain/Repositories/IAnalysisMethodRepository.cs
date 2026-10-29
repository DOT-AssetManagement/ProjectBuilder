using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IAnalysisMethodRepository : IRepository<ScenarioFilter>
    {
        Task<Guid?> GetScenarioLibraryIdAsync(int scenId);
        Task<List<FilteringAttribute>> GetFilteringAttributesAsync(Guid libraryId);
        Task<List<ScenarioFilter>> GetScenarioFiltersAsync(int scenId);
        Task<List<string>> GetAttributeValuesAsync(int scenId, string assetType, string attributeName);
        Task SaveCriteriaAsync(int scenId, List<ScenarioFilter> filters, string criteriaNotes);
        Task UpdateScenarioNotesAsync(int scenId, string notes);
        Task<int> GetMatchedTreatmentCountAsync(int scenId, List<ScenarioFilter> filters);
    }
}
