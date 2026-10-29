using ProjectBuilder.Models.ProjectCandidate.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.ProjectCandidate
{
    public interface IProjectCandidateAppService : ICrudAppService<ProjectCandidateDto, Guid, GetProjectCandidateInput, CreateProjectCandidateDto, UpdateProjectCandidateDto>
    {
        Task<List<ProjectCandidateDto>> GetAllAsync();
        Task<List<ProjectCandidateDto>> GetAllProjectsAsync(Guid libraryId);
        Task<List<string>> GetDistinctDescriptions();
        Task<string> GetGisJsonStringOutput(Guid scenarioId, double relativeEfficiencyThreshold);
    }
}
