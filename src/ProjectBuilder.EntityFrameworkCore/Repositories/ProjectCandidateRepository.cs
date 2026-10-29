
using PBLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Users;

public class ProjectCandidateRepository : EfCoreRepository<ProjectBuilderDbContext, ProjectCandidateEntity, Guid>, IProjectCandidateRepository
{
    private readonly ProjectBuilderDbContext _dbContext;
    private readonly ICurrentUser _curruntUser;
    private readonly string _connectionString = string.Empty;
    public ProjectCandidateRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider, IConfiguration configuration, ProjectBuilderDbContext dbContext, ICurrentUser curruntUser)
        : base(dbContextProvider)
    {
        _dbContext = dbContext;
        _curruntUser = curruntUser;
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<List<ProjectCandidateEntity>> GetAll()
    {

        var rows = await (from pc in _dbContext.ProjectCandidates.AsNoTracking()
                          join c in _dbContext.Counties.AsNoTracking()
                              on (int?)pc.Cnty equals c.Id into countyJoin
                          from c in countyJoin.DefaultIfEmpty()
                          select new { Project = pc, CountyName = c != null ? c.CountyName : null }).ToListAsync();

        return rows.Select(r =>
        {
            r.Project.CountyName = r.CountyName;
            return r.Project;
        }).ToList();
    }

    public async Task<List<string>> GetDistinctDescriptions()
    {
        var distinctDescriptions = await _dbContext.ProjectCandidates
                                                  .AsNoTracking()
                                                  .Where(pc => pc.Description != null)
                                                  .Select(pc => pc.Description)
                                                  .Distinct()
                                                  .ToListAsync();

        return distinctDescriptions;
    }

    //public async Task<List<ProjectCandidate>> GetProjectsByScenarioId(Guid scenarioId)
    //{
    //    var scenario = await _dbContext.Scenarios.AsNoTracking()
    //        .FirstOrDefaultAsync(x => x.Id == scenarioId);
    //    if (scenario == default)
    //    {
    //        throw new Exception("No Scenario Found against ID");
    //    }
    //    var projects = await _dbContext.ProjectCandidates.AsNoTracking()
    //        .Where(pc => pc.PoolId == scenario.PoolId)
    //        .ToListAsync();
    //    return projects;
    //}

    public async Task<List<ProjectCandidateEntity>> GetProjectsByPoolId(Guid poolId)
    {
        /*var projects = await _dbContext.ProjectCandidates.AsNoTracking()
            .Where(pc => pc.PoolId == poolId)
            .ToListAsync();

        return projects;*/

        var rows = await (from pc in _dbContext.ProjectCandidates.AsNoTracking()
                                   join c in _dbContext.Counties.AsNoTracking()
                                       on (int?)pc.Cnty equals c.Id into countyJoin
                                   from c in countyJoin.DefaultIfEmpty()
                                   where pc.LibraryId == poolId
                                   select new { Project = pc, CountyName = c != null ? c.CountyName : null }).ToListAsync();

        // pc.Id has a protected setter (Entity<Guid>), so it can't be assigned inside the
        // LINQ projection above; select the whole entity instead and enrich in memory.
        var projects = rows.Select(r =>
        {
            r.Project.CountyName = r.CountyName;
            return r.Project;
        }).ToList();

        return projects;
    }


    public async Task<bool> HasProjectsByPoolIdAsync(Guid poolId)
    {
        return await _dbContext.ProjectCandidates.AsNoTracking().AnyAsync(pc => pc.LibraryId == poolId);
    }

    public async Task<string> GetGisJsonStringOutput(Guid scenarioId, double RelativeEfficiencyThreshold)
    {
        string errors;
        string json;
        var response = DataManager.GetGisJsonStringOutput(_connectionString, scenarioId, RelativeEfficiencyThreshold, true, out json, out errors);

        return json;
    }
}
