using ProjectBuilder.Models.PoolScoreParameters.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.PoolScoreParameters
{
    public interface IPoolScoreParametersAppService
    {
        Task<List<PoolScoreParameterDto>> GetListByPoolIdAsync(Guid poolId);
        Task<PoolScoreParameterDto?> GetByPoolIdAndMeasureAsync(Guid poolId, string measure);
        Task<PoolScoreParameterDto> CreateAsync(PoolScoreParameterCreateDto input);
        Task<PoolScoreParameterDto> UpdateAsync(PoolScoreParameterUpdateDto input);
        Task<bool> DeleteAsync(Guid poolId, string measure);
        Task<bool> ImportAsync(string excelFilePath, string? worksheetName, Guid poolId, bool fromScratch, bool keepUserCreated);
        Task<List<string>> GetMeasureNamesAsync();
        Task<(byte[] Content, string FileName)> DownloadScoreParametersTemplateAsync();
        Task<(byte[] Content, string FileName)> ExportAsync(Guid poolId, string poolName);
    }
}
