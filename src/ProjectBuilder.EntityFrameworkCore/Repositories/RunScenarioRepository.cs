using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EventsArgs;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using PBLogic;
using ProjectBuilder.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using GisJsonHandler;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ProjectBuilder.Repositories
{
    public class RunScenarioRepository : EfCoreRepository<ProjectBuilderDbContext, ProjectBuilder.Entities.Scenario, int>, IRunScenarioRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<ScenarioParamater, string> _scenarioParametersRepo;
        private readonly ProjectBuilderDbContext _context;
        private readonly string _homeDirectory;

        public int ScenarioId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IRepository<ScenarioParamater, string> ScenarioParametersRepo => throw new NotImplementedException();


        public RunScenarioRepository(
            IDbContextProvider<ProjectBuilderDbContext> dbContextProvider,
            IRepository<ScenarioParamater, string> scenarioParametersRepo,
            ProjectBuilderDbContext dbContext,
            IConfiguration configuration)
            : base(dbContextProvider)
        {
            _scenarioParametersRepo = scenarioParametersRepo;
            _configuration = configuration;
            _context = dbContext;
            _homeDirectory = ResolveHomeDirectory(configuration);
        }
        
        private static string ResolveHomeDirectory(IConfiguration configuration)
        {
            var configured = configuration.GetSection("Configuration").GetValue<string>("HomeDirectory", ".");
            return string.IsNullOrWhiteSpace(configured) || configured == "."
                ? AppContext.BaseDirectory
                : configured;
        }
        public async Task<List<ProjectBuilder.Entities.Scenario>> GetScenariosListAsync()
        {
            var scenarios = await _context.Scenarios.ToListAsync();
            return scenarios;
        }
        public async Task<ScenarioEventArgs> CreateScenarioAsync(
            string scenarioName,
            Guid libraryId,
            int firstYear,
            int lastYear,
            int setDefault,
            string createdBy = "",
            bool isShared = false)
        {
            int scenarioId = -1;
            string errorMessage = "";
            bool success = await Task.Run(() =>
                DataManager.CreateNewScenario(
                    _configuration.GetConnectionString("Default"),
                    libraryId.ToString(),
                    scenarioName,
                    firstYear,
                    lastYear,
                    setDefault,
                    out scenarioId,
                    out errorMessage,
                    createdBy,
                    isShared));

            if (!success)
            {
                return new ScenarioEventArgs(errorMessage, LogLevel.Error, -1);
            }

            await Task.Run(() =>
                DataManager.CreateExtendedScenarioProjects(
                    _configuration.GetConnectionString("Default"),
                    scenarioId,
                    out errorMessage));

            return new ScenarioEventArgs(string.Empty, LogLevel.None, scenarioId);
        }
        public async Task<ProjectBuilder.Entities.Scenario> GetScenarioByPoolId(Guid poolId)
        {
            var scenario = _context.Scenarios.FirstOrDefault(scen => scen.LibraryId == poolId);
            return scenario;
        }

        public async Task UpdateScenarioByPoolIdAsync(Guid poolId, string name, string notes)
        {
            var scenario = _context.Scenarios.FirstOrDefault(s => s.LibraryId == poolId);
            if (scenario == null) return;
            scenario.ScenarioName = name;
            scenario.Notes = notes;
            await _context.SaveChangesAsync();
        }

        public async Task<ScenarioEventArgs> RunScenarioAsync(int scenarioId, string name, bool doCleanup)
        {
            string errorMessage = "";
            var lastRunAt = DateTime.Now;

            // MIXB ("Allow mixing bridge and pavement budgets") is set on the scenario's
            // Parameters page; ISNULL-to-0-then->0 matches how the engine reads other
            // boolean-like scenario parameters (e.g. IGNR) from tbl_pb_ScenParm.
            var mixBudgetsParm = await _context.ScenarioParameters
                .FirstOrDefaultAsync(p => p.ScenarioId == scenarioId && p.Id == "MIXB");
            bool mixAssetBudgets = (mixBudgetsParm?.ParameterValue ?? 0) > 0;

            var communique = new ScenarioCommunique
            {
                Commitment = false,
                District = null,
                ProjectsOnly = false,
                SingleTreatmentsOnly = false,
                MaxPriority = 10,
                MixAssetBudgets = mixAssetBudgets
            };

            bool success = await Task.Run(() =>
                SymphonyConductor.RunExtendedScenario(
                    _configuration.GetConnectionString("Default"),
                    scenarioId,
                    communique,
                    _homeDirectory,
                    false,
                    out errorMessage));

            var lastFinished = DateTime.Now;

            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                return new ScenarioEventArgs(errorMessage, LogLevel.Error, scenarioId);
            }

            // The optimization engine only touches its own result tables; LastRunBy/LastRunAt/
            // LastStarted/LastFinished must be stamped here (matching the legacy
            // RunScenarioUnitOfWork.RunScenario), otherwise "Last Run By" is left blank or shows
            // whatever identity the engine's own DB connection ran under (e.g. the app pool
            // account) instead of the signed-in user who triggered the run.
            // The optimization engine already finished; don't fail the job if this bookkeeping update times out.
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var dbContext = await GetDbContextAsync();
                var scenario = await dbContext.Scenarios.FindAsync(new object[] { scenarioId }, cts.Token);
                if (scenario != null)
                {
                    scenario.Stale = false;
                    scenario.LastRunBy = name;
                    scenario.LastRunAt = lastRunAt;
                    scenario.LastStarted = lastRunAt;
                    scenario.LastFinished = lastFinished;
                    dbContext.Scenarios.Update(scenario);
                    await dbContext.SaveChangesAsync(cts.Token);
                }
            }
            catch (Exception ex)
            {
                // RunExtendedScenario already updated tbl_pb_Scenarios; stale flag can be corrected on next page load.
                return new ScenarioEventArgs(
                    $"Scenario run completed but status update failed: {ex.Message}",
                    LogLevel.Warning,
                    scenarioId);
            }

            return new ScenarioEventArgs(string.Empty, LogLevel.None, scenarioId);
        }

        public async Task<ScenarioEventArgs> DeleteScenarioAsync(int scenarioId)
        {
            // Mirrors the legacy MVC flow (ScenariosController.DeleteScenerio): delete the
            // Scenario row itself via EF first, then run the generic cross-table cleanup that
            // wipes every other ScenId-tagged table (DataManager.DeleteScenario). Relying on the
            // generic cleanup alone to also remove the Scenarios row can silently no-op if
            // tbl_pb_Scenarios' primary key isn't registered as a formal DB constraint, which is
            // why delete previously appeared to succeed but left the scenario in the list.
            var scenario = await _context.Scenarios.FindAsync(scenarioId);
            if (scenario == null)
            {
                return new ScenarioEventArgs("Could not find the selected scenario.", LogLevel.Error, scenarioId);
            }

            _context.Scenarios.Remove(scenario);
            await _context.SaveChangesAsync();

            string errorMessage = "";
            bool success = await Task.Run(() =>
                DataManager.DeleteScenario(
                    _configuration.GetConnectionString("Default"),
                    scenarioId,
                    out errorMessage));

            // Best-effort cleanup of related tables; the scenario itself is already gone, so
            // don't fail the whole delete over leftover child rows (matches the legacy
            // behavior of logging this error without gating the response on it).
            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                Logger.LogError("DeleteScenarioAsync cross-table cleanup failed for ScenId={ScenarioId}: {ErrorMessage}", scenarioId, errorMessage);
            }

            return new ScenarioEventArgs(string.Empty, LogLevel.None, scenarioId);
        }

        public async Task<MapsResultModel> ExportScenarioResultsToJsonAsync(
            int scenarioId,
            bool indent,
            int? district = null,
            int? cnty = null,
            int? route = null,
            string section = null,
            string appliedTreatment = null,
            int? selectedYear = null)
        {
            string errorMessage = "";
            string jsonString = "";

            bool success = await Task.Run(() =>
                JsonExporter.ExportScenarioResultsToJson(
                    _configuration.GetConnectionString("Default"),
                    scenarioId,
                    indent,
                    out jsonString,
                    out errorMessage,
                    district,
                    cnty,
                    route,
                    null,
                    appliedTreatment));

            return new MapsResultModel(errorMessage, jsonString, !success);
        }
        public async Task<ProjectBuilder.Entities.Scenario> FindScenarioByIdAsync(int? scenid)
        {
            return await _context.Scenarios
                .Where(s => s.Id == scenid)
                .FirstOrDefaultAsync();
        }
    }

}
