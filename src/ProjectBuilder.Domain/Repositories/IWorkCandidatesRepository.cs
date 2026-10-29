
using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

public interface IWorkCandidatesRepository : IBasicRepository<WorkCandidateEntity, Guid>
{
    Task<List<WorkCandidateEntity>> GetListByPoolOrProjectId(Guid id, Guid Projectid);

    Task<WorkCandidateEntity> GetByProjectId(Guid projectId);
    Task<bool> CreateWorkCandidate(WorkCandidateEntity workCandidate);

    Task<bool> UpdateWorkCandidate(Guid workCandidateId);

    Task<bool> UpdatePool(Guid poolId, bool scoreParametersUpdated);
    Task<List<WorkCandidateEntity>> GetAllByPoolAndWorkCandidateIds(Guid poolId, List<Guid> treworkCandidateIdsatIds);

    Task<List<string>> GetDistinctProjectNames(Guid poolId);

    Task<bool> HasByPoolIdAsync(Guid poolId);

    Task<Dictionary<Guid, int>> GetCountsByPoolIdsAsync(IReadOnlyCollection<Guid> poolIds);

}
