
using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

public interface IProjectCandidateRepository : IBasicRepository<ProjectCandidateEntity, Guid>
{
    Task<List<ProjectCandidateEntity>> GetAll();
    Task<List<ProjectCandidateEntity>> GetProjectsByPoolId(Guid poolId);
    Task<bool> HasProjectsByPoolIdAsync(Guid poolId);
   // Task<List<ProjectCandidate>> GetProjectsByScenarioId(Guid scenarioId);
    Task<List<string>> GetDistinctDescriptions();
    Task<string> GetGisJsonStringOutput(Guid scenarioId, double RelativeEfficiencyThreshold);
}
