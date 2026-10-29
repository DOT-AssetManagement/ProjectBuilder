using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.EntityFrameworkCore.Repositories
{
    public class DssScenarioRepository : EfCoreRepository<ProjectBuilderDbContext, DssScenario, Guid>, IDssScenarioRepository
    {
        private readonly string _connectionString;

        public DssScenarioRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider, IConfiguration configuration)
            : base(dbContextProvider)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public async Task<DssScenario> GetByPoolIdAsync(Guid poolId)
        {
            var ctx = await GetDbContextAsync();
            return ctx.DssScenarios.FirstOrDefault(s => s.PoolId == poolId);
        }

        public async Task<DssScenario> GetByIdAsync(Guid id)
        {
            return await FindAsync(id);
        }

        public async Task DeleteByPoolIdAsync(Guid poolId)
        {
            var ctx = await GetDbContextAsync();
            var scenario = ctx.DssScenarios.FirstOrDefault(s => s.PoolId == poolId);
            if (scenario == null) return;
            ctx.DssScenarios.Remove(scenario);
            await ctx.SaveChangesAsync();
        }

        public async Task<List<DssScenario>> GetByPoolIdsAsync(List<Guid> poolIds)
        {
            var ctx = await GetDbContextAsync();
            return await ctx.DssScenarios.Where(s => s.PoolId != null && poolIds.Contains(s.PoolId.Value)).ToListAsync();
        }

        public async Task<DssScenario> CreateForPoolAsync(Guid poolId, string scenName, string description, string createdBy)
        {
            var ctx = await GetDbContextAsync();
            var scenario = new DssScenario
            {
                Id = Guid.NewGuid(),
                PoolId = poolId,
                ScenName = scenName,
                Description = description,
                // DEAContext.LoadFromDatabase looks these up and calls them as stored procedures
                // verbatim — leaving them null makes it try to execute an empty command text.
                DeaInputSproc = "dbo.UspGetProjectCandidates",
                DeaOutputSproc = "dbo.UspGetScenarioOutput",
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now
            };
            ctx.DssScenarios.Add(scenario);
            await ctx.SaveChangesAsync();
            return scenario;
        }

        public async Task UpdateByPoolIdAsync(Guid poolId, string scenName, string description)
        {
            var ctx = await GetDbContextAsync();
            var scenario = ctx.DssScenarios.FirstOrDefault(s => s.PoolId == poolId);
            if (scenario == null) return;
            scenario.ScenName = scenName;
            scenario.Description = description;
            await ctx.SaveChangesAsync();
        }

        public async Task<(bool Ok, string ErrorMessage)> RunScenarioAsync(Guid scenarioId)
        {
            var ok = PBLogic.DataManager.RunScenario(_connectionString, scenarioId, out string errorMessage);
            return (ok, errorMessage);
        }
    }
}
