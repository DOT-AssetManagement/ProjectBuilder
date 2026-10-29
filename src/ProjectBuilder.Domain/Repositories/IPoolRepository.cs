
using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPoolRepository
{
    Task<UserPoolEntity> CheckName(string poolName);
    Task<Guid?> CreatePool(string name, string description, Guid userId, string createdBy, bool isSharedLibrary = false);
    bool ClearPool(Guid poolId);
    List<UserPoolEntity> GetPoolsByUserId(Guid userId, string name, string poolId, bool canViewAll = false);
    bool Import(string excelFilePath, string? importSourceName, string? worksheetName, Guid poolId, bool forgiving, bool deleteAll = false);
    bool ImportBridgeToPavements(string excelFilePath, string? worksheetName = null);
    bool ImportBridgeToPavementsDss(string excelFilePath, string? worksheetName = null);
    bool ImportAssetSegmentation(string excelFilePath, string? worksheetName);
    bool ImportSuperAssets(string excelFilePath, string? worksheetName);
    Task<int> ReplaceBridgeReferenceDataAsync(List<BridgeReferenceData> rows);

    Task<UserPoolEntity> GetAsync(Guid id);
    Task<List<UserPoolEntity>> GetListAsync();
    Task<UserPoolEntity> UpdateAsync(UserPoolEntity entity);
    Task DeleteAsync(Guid id);
}
