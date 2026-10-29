using PBLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PoolScoreParameterRepository : IPoolScoreParameterRepository
{
    private readonly string _connectionString = string.Empty;
    private readonly ProjectBuilderDbContext _dbContext;

    public PoolScoreParameterRepository(IConfiguration configuration, ProjectBuilderDbContext dbContext)
    {
        _connectionString = configuration.GetConnectionString("Default");
        _dbContext = dbContext;
    }

    public async Task<List<PoolScoreParameterEntity>> GetListByPoolIdAsync(Guid poolId)
    {
        return await _dbContext.PoolScoreParameters
            .Where(x => x.PoolId == poolId)
            .OrderBy(x => x.Measure)
            .ToListAsync();
    }

    public async Task<PoolScoreParameterEntity?> GetByPoolIdAndMeasureAsync(Guid poolId, string measure)
    {
        return await _dbContext.PoolScoreParameters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PoolId == poolId && x.Measure.ToLower() == measure.ToLower());
    }

    public async Task<PoolScoreParameterEntity> InsertAsync(PoolScoreParameterEntity entity)
    {
        _dbContext.PoolScoreParameters.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(PoolScoreParameterEntity entity)
    {
        var existing = await _dbContext.PoolScoreParameters
            .FirstOrDefaultAsync(x => x.PoolId == entity.PoolId && x.Measure.ToLower() == entity.Measure.ToLower());
        if (existing == null) return false;

        existing.Min = entity.Min;
        existing.ScoreAtMin = entity.ScoreAtMin;
        existing.Max = entity.Max;
        existing.ScoreAtMax = entity.ScoreAtMax;
        existing.Slope = entity.Slope;
        existing.SafetyWeight = entity.SafetyWeight;
        existing.MobilityWeight = entity.MobilityWeight;
        existing.EquityWeight = entity.EquityWeight;
        existing.EnvWeight = entity.EnvWeight;
        existing.CondWeight = entity.CondWeight;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid poolId, string measure)
    {
        var existing = await _dbContext.PoolScoreParameters
            .FirstOrDefaultAsync(x => x.PoolId == poolId && x.Measure.ToLower() == measure.ToLower());
        if (existing == null) return false;

        _dbContext.PoolScoreParameters.Remove(existing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public bool Import(Guid poolId, string excelFilePath, string? worksheetName, bool fromScratch, bool keepUserCreated)
    {
        string? errorMessage;
        var result = DataManager.ImportScoreParameters(_connectionString, poolId, excelFilePath, worksheetName, out errorMessage, fromScratch, keepUserCreated);
        if (!result) throw new Exception(errorMessage);
        return true;
    }

    public bool CreatePoolScoreParameters(Guid poolId)
    {
        throw new NotImplementedException();
    }
}
