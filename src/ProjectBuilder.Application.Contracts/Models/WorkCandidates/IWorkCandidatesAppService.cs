using ProjectBuilder.Models.WorkCandidates.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.WorkCandidates
{
    public interface IWorkCandidatesAppService : ICrudAppService<WorkCandidatesDto, Guid, GetWorkCandidatesInput, CreateWorkCandidatesDto, UpdateWorkCandidatesDto>
    {
        Task<List<WorkCandidatesDto>> GetAllByPoolOrProjectIdAsync(Guid libraryId, Guid projectId);
        Task<WorkCandidatesDto> GetByProjectId(Guid projectId);
        Task<WorkCandidatesDto> UpdateWorkCandidateAsync(Guid id, UpdateWorkCandidatesDto input, Guid libraryId);
        Task<bool> CreateWorkCandidateAsync(CreateWorkCandidatesDto input, Guid libraryId);
        Task<List<WorkCandidatesDto>> GetAllByPoolAndWorkCandidateIds(Guid libraryId, List<Guid> workCandidateIds);
        Task<(byte[] Content, string FileName)> ExportAsync(Guid poolId, string poolName);
        Task<List<string>> GetDistinctProjectNamesAsync(Guid poolId);
    }
}
