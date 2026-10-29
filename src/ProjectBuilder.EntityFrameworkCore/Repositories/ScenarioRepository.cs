
using PBLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Users;

public class ScenarioRepository : EfCoreRepository<ProjectBuilderDbContext, Scenario, int>, IScenarioRepository
{
    private readonly ProjectBuilderDbContext _dbContext;
    private readonly ICurrentUser _curruntUser;

    private readonly string _connectionString = string.Empty;
    public ScenarioRepository(IDbContextProvider<ProjectBuilderDbContext> dbContextProvider,
        ProjectBuilderDbContext dbContext,
        IConfiguration configuration,
        ICurrentUser curruntUser = null)
        : base(dbContextProvider)
    {
        _dbContext = dbContext;
        _connectionString = configuration.GetConnectionString("Default");
        _curruntUser = curruntUser;
    }

    public async Task<bool> CheckName(string scenarioName)
    {
        var result = await _dbContext.Scenarios.AsNoTracking()
            .AnyAsync(x => x.ScenarioName.ToLower() == scenarioName.ToLower());
        return result;
    }

    public async Task<Scenario> GetScenarioByPoolId(Guid poolId)
    {
        var scenario = _dbContext.Scenarios.FirstOrDefault(scen => scen.LibraryId == poolId);
        return scenario;
    }

    public async Task<Guid?> Create(Scenario scenario)
    {
        string errors;
        Guid? scenarioId;
        var result = DataManager.CreateScenario(_connectionString, scenario.ScenarioName, scenario.Description, scenario.LibraryId.Value, scenario.DeaInputSproc, scenario.DeaOutputSproc, out scenarioId, out errors);
        if (!result)
            throw new Exception(errors);
        return scenarioId;
    }
    //public async Task<List<Scenario>> GetScenarioByUserId(String name)
    //{
    //    var userPoolsIds = await _dbContext.Libraries.AsNoTracking()
    //        .Where(x => x.UserId == _curruntUser.Id)
    //        .Select(x=> x.Id)
    //        .ToListAsync();


    //    var userScenarios = await _dbContext.Scenarios.AsNoTracking()
    //        .Include(x => x.LibraryId)
    //        .Where(s => userPoolsIds.Contains(s.LibraryId) && (name == "" || s.ScenarioName == name))
    //        .ToListAsync();
    //    /*var userScenarios = await (from scenario in _dbContext.Scenarios.AsNoTracking()
    //                               join pool in _dbContext.TblUserPools.AsNoTracking()
    //                               on scenario.PoolId equals pool.Id
    //                               where userPoolsIds.Contains(scenario.PoolId) && (string.IsNullOrEmpty(name) || scenario.ScenName == name)
    //                               select new 
    //                               {
    //                                   scenario.ScenName,
    //                                   pool.Name
    //                               }).ToListAsync();*/



    //    return userScenarios;
    //}

    public async Task<List<Scenario>> GetScenarioByUserId(Guid userId, String name)
    {
        var userPoolsIds = await _dbContext.Libraries.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .ToListAsync();


        var userScenarios = await _dbContext.Scenarios.AsNoTracking()
            .Include(x => x.LibraryId)
            .Where(s => userPoolsIds.Contains(s.LibraryId.Value) && (name == "" || s.ScenarioName == name))
            .ToListAsync();
        /*var userScenarios = await (from scenario in _dbContext.Scenarios.AsNoTracking()
                                   join pool in _dbContext.TblUserPools.AsNoTracking()
                                   on scenario.PoolId equals pool.Id
                                   where userPoolsIds.Contains(scenario.PoolId) && (string.IsNullOrEmpty(name) || scenario.ScenName == name)
                                   select new 
                                   {
                                       scenario.ScenName,
                                       pool.Name
                                   }).ToListAsync();*/



        return userScenarios;
    }

    public async Task<bool> RunScenario(Guid id)
    {
        string errors;
        var result = DataManager.RunScenario(_connectionString, id, out errors);
        return result;
    }
    public byte Export(Guid? poolId, double? effThreshold, string xlsxFilePath)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string storedProcedure = "dbo.UspGetScenarioOutput";
            if (!string.IsNullOrEmpty(storedProcedure))
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new SqlCommand(storedProcedure, connection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 24000
                    };

                    if (poolId != null)
                    {
                        command.Parameters.Add("@PoolId", SqlDbType.UniqueIdentifier).Value = poolId.Value;
                    }
                    if (effThreshold != null)
                    {
                        command.Parameters.Add("@RelativeEfficiencyThreshold", SqlDbType.Float).Value = effThreshold.Value;
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        DataSet dataSet = new DataSet();
                        dataSet.Load(reader, LoadOption.OverwriteChanges, "Projects", "Pavements", "Bridges");

                        using (var workbook = new ExcelPackage())
                        {
                            var workSheetNames = new List<string> { "Projects", "Pavements", "Bridges" };

                            for (int i = 0; i < workSheetNames.Count; i++)
                            {
                                string sheetName = workSheetNames[i];
                                DataTable dataTable = dataSet.Tables[i];

                                if (dataTable != null)
                                {
                                    ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add(sheetName);
                                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                                }
                            }

                            workbook.SaveAs(new FileInfo(xlsxFilePath));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during export: {ex.Message}");
            return 0; // Failure
        }

        return 1; // Success
    }
}

