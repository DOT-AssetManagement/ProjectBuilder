using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Pool.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Pool
{
    public interface IPoolAppService : ICrudAppService<PoolDto, Guid, GetPoolInput, CreatePoolDto, UpdatePoolDto>
    {
        Task<PoolDto> CheckName(string poolName);
        Task<List<CandidatePoolDto>> GetAllAsync(string name, string poolId);
        Task<Guid?> Create(CreatePoolDto input);
        Task ClearAsync(Guid id);
        bool ImportBridgeToPavements(string excelFilePath);
        Task<bool> ImportAssetSegmentationAsync(string excelFilePath, string worksheetName);
        Task<bool> ImportSuperAssetsAsync(string excelFilePath, string worksheetName);
        bool ImportWorkCandidates(string excelFilePath, string importSourceName, string worksheetName, Guid poolId, bool forgiving = false, bool deleteAll = false);

        Task<string> GetDssScenarioNameByPoolIdAsync(Guid poolId);
        Task CreateDssScenarioAsync(Guid poolId, string name, string description, string createdBy);
        Task UpdateDssScenarioAsync(Guid poolId, string name, string description);
    }
}
