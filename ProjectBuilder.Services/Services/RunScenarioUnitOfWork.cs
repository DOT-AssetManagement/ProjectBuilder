using GisJsonHandler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PBLogic;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.DataAccess;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Services.Services
{
    public class RunScenarioUnitOfWork : IRunScenarioUnitOfWork
    {
	
		private readonly string _connectionString;
        private string _homeDirectory;
        private int _scenarioId;
        private readonly Dictionary<string, object> _filter;
        public bool IsPending { get { return ScenarioParametersRepo.IsPending || ScenariosBudgetsRepo.IsPending; } }
        public int ScenarioId
        {
            get { return _scenarioId; }
            set
            {
                if (_scenarioId != value)
                {
                    _scenarioId = value;
                    _filter[nameof(ScenarioId)] = value;
                    ScenarioParametersRepo.ApplyFilter(_filter);
                    ScenariosBudgetsRepo.ApplyFilter(_filter);
                }
            }
        }
		private ProjectBuilderDbContext _context;
		public IRepository<ScenarioParameterModel> ScenarioParametersRepo { get; }
        public IRepository<ScenarioModel> ScenariosRepo { get; }
        public IRepository<ScenarioBudgetFlatModel> ScenariosBudgetsRepo { get; }

        public event Action<Core.ErrorEventArgs> ErrorOccured;
        public RunScenarioUnitOfWork(IRepository<ScenarioParameterModel> scenarioParametersRepo, IRepository<ScenarioModel> scenariosRepo,
                                     IRepository<ScenarioBudgetFlatModel> scenariosBudgetsRepo, IConfiguration configuration, ProjectBuilderDbContext context)
        {
            ScenarioParametersRepo = scenarioParametersRepo;
            ScenariosRepo = scenariosRepo;
			_context=context;
			ScenariosBudgetsRepo = scenariosBudgetsRepo;
            _connectionString = configuration.GetConnectionString("Default");
            _homeDirectory = configuration.GetSection("Configuration").GetValue("HomeDirectory", ".") == "." ? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) : configuration.GetSection("Configuration").GetValue("HomeDirectory", ".");
            scenarioParametersRepo.ErrorOccured += RaiseErrorOccured;
            scenariosRepo.ErrorOccured += RaiseErrorOccured;
            scenariosBudgetsRepo.ErrorOccured += RaiseErrorOccured;
            _filter = new Dictionary<string, object> { { nameof(ScenarioId), -1 } };
        }

        private void RaiseErrorOccured(Core.ErrorEventArgs error)
        {
            ErrorOccured?.Invoke(error);
        }
        public async Task SaveChangesAsync(CancellationToken token = default)
        {
            if (ScenarioParametersRepo.IsPending)
                await ScenarioParametersRepo.SaveChangesAsync(token);
            if (ScenariosBudgetsRepo.IsPending)
                await ScenariosBudgetsRepo.SaveChangesAsync(token);
        }

        public async Task<ScenarioEventArgs> CreateScenario(string scenarioName,  Guid libraryId, int firstYear, int lastYear, int setDefault, string CreatedBy = "")
        {
            int scenarioId = -1;
            string errorMessage = "";
            var success = await Task.Run(() => DataManager.CreateNewScenario(_connectionString, libraryId.ToString(),scenarioName,firstYear,lastYear, setDefault, out scenarioId, out errorMessage, CreatedBy));
            if (success)
            {
                await Task.Run(() => DataManager.CreateExtendedScenarioProjects(_connectionString, scenarioId, out errorMessage));
                return new(string.Empty, LogLevel.None, scenarioId);
            }
            var scenarioEventArgs = new ScenarioEventArgs(errorMessage, LogLevel.Error, -1);
            RaiseErrorOccured(scenarioEventArgs);
            return scenarioEventArgs;
        }
        public async Task<ScenarioEventArgs> RunScenario(int scenarioId, string name, bool doCleanup)
        {
            var lastRunAt = DateTime.Now;
            string errorMessage = "";
            var scenarioCommunique = new ScenarioCommunique() { Commitment = false, District = null, MaxPriority = 10, MixAssetBudgets = false };
            //var success = await Task.Run(() => SymphonyConductor.RunExtendedScenario(_connectionString, _homeDirectory, scenarioId, scenarioCommunique, 8, doCleanup, out errorMessage));
            var success = await Task.Run(() => SymphonyConductor.RunExtendedScenario(_connectionString, _homeDirectory, scenarioId, false,out errorMessage));
            
            var lastFinished = DateTime.Now;
            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                var scenarioEventArgs = new ScenarioEventArgs(errorMessage,LogLevel.Error,scenarioId);
                RaiseErrorOccured(scenarioEventArgs);
                return scenarioEventArgs;
            }
            var pairs = new Dictionary<string, object>
            {
                { "LastRunBy", name },
                { "LastRunAt", lastRunAt },
                { "LastStarted", lastRunAt },
                { "LastFinished", lastFinished }
            };
			var scenario = await _context.Scenarios.FindAsync(scenarioId);
            if (scenario != null) 
            {
                scenario.Stale = false; 
                _context.Scenarios.Update(scenario); 
                _context.SaveChanges(); 
            }
            var scenario2 = ScenariosRepo.FindAsync(scenarioId);
			await ScenariosRepo.UpdateAsync(scenarioId, pairs);
            await ScenariosRepo.SaveChangesAsync();
            return new(string.Empty, LogLevel.None, scenarioId);
        }

		public async Task<ScenarioEventArgs> DeleteScenario(int scenarioId)
        {
            string errorMessage = "";
            var success = await Task.Run(() => DataManager.DeleteScenario(_connectionString, scenarioId, out errorMessage));
            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                var scenarioEventArgs = new ScenarioEventArgs(errorMessage,LogLevel.Error, scenarioId);
                RaiseErrorOccured(scenarioEventArgs);
                return scenarioEventArgs;
            }
            return new(string.Empty, LogLevel.None, scenarioId);
        }

        public async Task<MapsResultModel> ExportScenarioResultsToJson(int scenarioId, bool indent,
                int? district = null, int? cnty = null, int? route = null, string section = null, string appliedTreatment = null, int? selectedYear = null)
        {
            string errorMessage = "";
            string jsonString = "";
            var success = await Task.Run(() => JsonExporter.ExportScenarioResultsToJson(_connectionString, scenarioId, indent, out jsonString, out errorMessage, district, cnty, route, null, appliedTreatment));
            return new(errorMessage,jsonString,!success);
        }
        
        public void ClearPendingOperations()
        {
            ScenarioParametersRepo.ClearPendingChanges();
            ScenariosBudgetsRepo.ClearPendingChanges();
        }
    }
}
