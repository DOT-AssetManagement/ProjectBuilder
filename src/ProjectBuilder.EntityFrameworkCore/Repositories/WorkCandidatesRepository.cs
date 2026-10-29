using DEALib;
using PBLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

public class WorkCandidatesRepository : EfCoreRepository<ProjectBuilderDbContext, WorkCandidateEntity, Guid>, IWorkCandidatesRepository
{
    private readonly string _connectionString = string.Empty;
    private readonly ProjectBuilderDbContext _dbContext;
    public WorkCandidatesRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider, ProjectBuilderDbContext dbContext, IConfiguration configuration)
        : base(dbContextProvider)
    {
        _dbContext = dbContext;
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<List<WorkCandidateEntity>> GetListByPoolOrProjectId(Guid poolId, Guid projectId)
    {
        var query = projectId != Guid.Empty
            ? _dbContext.WorkCandidates.Where(x => x.ProjectCandidateId == projectId)
            : _dbContext.WorkCandidates.Where(x => x.UserLibraryId == poolId);

        return await query.ToListAsync();
    }

    public async Task<WorkCandidateEntity> GetByProjectId(Guid projectId)
    {
        var result = await _dbContext.WorkCandidates
        .FirstOrDefaultAsync(x => x.ProjectCandidateId == projectId);

        return result;
    }
    public async Task<List<WorkCandidateEntity>> GetAllByPoolAndWorkCandidateIds(Guid poolId, List<Guid> workCandidateIds)
    {
        var results = await _dbContext.WorkCandidates
            .Where(x => x.UserLibraryId == poolId && workCandidateIds.Contains(x.Id))
            .ToListAsync();
        return results;
    }
    public async Task<bool> UpdatePool(Guid poolId, bool scoreParametersUpdated)
    {
        string errors;
        var response = DataManager.UpdatePool(_connectionString, poolId, scoreParametersUpdated, out errors);

        return response;
    }
    public async Task<bool> CreateWorkCandidate(WorkCandidateEntity workCandidate)
    {
        string? errors;
        Guid? workCandidateId;
        Guid? projectCandidateId;
        var response = DataManager.CreateNewWorkCandidate(_connectionString, workCandidate, out workCandidateId, out projectCandidateId, out errors);

        return response;
    }
    public async Task<bool> UpdateWorkCandidate(Guid workCandidateId)
    {
        string errors;
        var response = DataManager.UpdateWorkCandidate(_connectionString, workCandidateId, out errors);

        return response;
    }

    public async Task<List<string>> GetDistinctProjectNames(Guid poolId)
    {
        return await _dbContext.WorkCandidates
            .Where(x => x.UserLibraryId == poolId && x.ProjectName != null)
            .Select(x => x.ProjectName)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> HasByPoolIdAsync(Guid poolId)
    {
        return await _dbContext.WorkCandidates.AsNoTracking()
            .AnyAsync(x => x.UserLibraryId == poolId);
    }

    public async Task<Dictionary<Guid, int>> GetCountsByPoolIdsAsync(IReadOnlyCollection<Guid> poolIds)
    {
        if (poolIds == null || poolIds.Count == 0)
            return new Dictionary<Guid, int>();

        return await _dbContext.WorkCandidates.AsNoTracking()
            .Where(x => x.UserLibraryId.HasValue && poolIds.Contains(x.UserLibraryId.Value))
            .GroupBy(x => x.UserLibraryId!.Value)
            .Select(g => new { PoolId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PoolId, x => x.Count);
    }
}
