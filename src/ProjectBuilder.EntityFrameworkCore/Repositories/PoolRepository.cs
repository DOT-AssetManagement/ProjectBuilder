
using PBLogic;
using ExcelWrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PoolRepository : IPoolRepository
{
    private readonly string _connectionString = string.Empty;
    private readonly ProjectBuilderDbContext _dbContext;

    public PoolRepository(IConfiguration configuration, ProjectBuilderDbContext dbContext)
    {
        _connectionString = configuration.GetConnectionString("Default");
        _dbContext = dbContext;
    }

    public async Task<UserPoolEntity> CheckName(string poolName)
    {
        return await _dbContext.UserPools.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToLower() == poolName.ToLower());
    }

    public bool ClearPool(Guid poolId)
    {
        string errors;
        var result = DataManager.ClearPoolWorkCandidateData(_connectionString, poolId, out errors);
        if (!result) throw new Exception(errors);
        return true;
    }

    public async Task<Guid?> CreatePool(string name, string description, Guid userId, string createdBy, bool isSharedLibrary = false)
    {
        var pool = new UserPoolEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            UserId = userId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now,
            IsActive = true,
            IsSharedLibrary = isSharedLibrary
        };
        _dbContext.UserPools.Add(pool);
        await _dbContext.SaveChangesAsync();
        return pool.Id;
    }

    public List<UserPoolEntity> GetPoolsByUserId(Guid userId, string name, string poolId, bool canViewAll = false)
    {
        return _dbContext.UserPools
            .Where(pool => (canViewAll || pool.UserId == userId || pool.IsSharedLibrary == true)
                        && (string.IsNullOrEmpty(name) || pool.Name == name)
                        && (string.IsNullOrEmpty(poolId) || pool.Id == Guid.Parse(poolId)))
            .OrderByDescending(pool => pool.CreatedAt)
            .ToList();
    }

    public bool Import(string excelFilePath, string? importSourceName, string? worksheetName, Guid poolId, bool forgiving, bool deleteAll = false)
    {
        string errors;
        Guid? session;
        var result = DataManager.ImportDSSWorkCandidatesFromExcel(_connectionString, excelFilePath, importSourceName, worksheetName, poolId, forgiving, deleteAll, out session, out errors);
        if (!result) throw new Exception(errors);
        return true;
    }

    public bool ImportBridgeToPavements(string excelFilePath, string? worksheetName = null)
    {
        string? errorMessage;
        bool result = ExcelHandler.ImportBridgeToPavement(_connectionString, excelFilePath, worksheetName, out errorMessage);
        if (!result)
            throw new Exception(errorMessage ?? "An error occurred during the import process.");
        return true;
    }

    public bool ImportBridgeToPavementsDss(string excelFilePath, string? worksheetName = null)
    {
        string? errorMessage;
        bool result = ExcelHandler.ImportBridgeToPavementToDss(_connectionString, excelFilePath, worksheetName, out errorMessage);
        if (!result)
            throw new Exception(errorMessage ?? "An error occurred during the import process.");
        return true;
    }

    public bool ImportAssetSegmentation(string excelFilePath, string? worksheetName)
    {
        bool result = ExcelHandler.ImportMaintainableAssetSegmentation(_connectionString, excelFilePath, worksheetName, false, out string? errorMessage);
        if (!result)
            throw new Exception(errorMessage ?? "An error occurred during the import process.");
        return true;
    }

    public bool ImportSuperAssets(string excelFilePath, string? worksheetName)
    {
        string? errorMessage;
        bool result = DataManager.ImportPAMSSuperAssets(_connectionString, excelFilePath, worksheetName, out errorMessage);
        if (!result)
            throw new Exception(errorMessage ?? "An error occurred during the import process.");
        return true;
    }

    public async Task<int> ReplaceBridgeReferenceDataAsync(List<BridgeReferenceData> rows)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE tbl_pb_BridgeReferenceDataFromPennshare");

        const int batchSize = 1000;
        for (int i = 0; i < rows.Count; i += batchSize)
        {
            _dbContext.BridgeReferenceData.AddRange(rows.Skip(i).Take(batchSize));
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
        }

        await transaction.CommitAsync();
        return rows.Count;
    }

    public async Task<UserPoolEntity> GetAsync(Guid id)
    {
        return await _dbContext.UserPools.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<UserPoolEntity>> GetListAsync()
    {
        return await _dbContext.UserPools.ToListAsync();
    }

    public async Task<UserPoolEntity> UpdateAsync(UserPoolEntity entity)
    {
        _dbContext.UserPools.Update(entity);
        _dbContext.Entry(entity).Property(e => e.LibNo).IsModified = false;
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbContext.UserPools.FirstOrDefaultAsync(p => p.Id == id);
        if (entity != null)
        {
            _dbContext.UserPools.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
