using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPoolScoreParameterRepository
{
    Task<List<PoolScoreParameterEntity>> GetListByPoolIdAsync(Guid poolId);
    Task<PoolScoreParameterEntity?> GetByPoolIdAndMeasureAsync(Guid poolId, string measure);
    Task<PoolScoreParameterEntity> InsertAsync(PoolScoreParameterEntity entity);
    Task<bool> UpdateAsync(PoolScoreParameterEntity entity);
    Task<bool> DeleteAsync(Guid poolId, string measure);
    bool Import(Guid poolId, string excelFilePath, string? worksheetName, bool fromScratch, bool keepUserCreated);
    bool CreatePoolScoreParameters(Guid poolId);
}
