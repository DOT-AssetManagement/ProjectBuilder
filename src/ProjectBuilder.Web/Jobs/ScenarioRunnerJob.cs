using Hangfire;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Repositories;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace ProjectBuilder.Web.Jobs
{
    public class ScenarioRunnerJob : ITransientDependency
    {
        private readonly IRunScenarioRepository _scenarioRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<ScenarioRunnerJob> _logger;

        public ScenarioRunnerJob(
            IRunScenarioRepository scenarioRepository,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<ScenarioRunnerJob> logger)
        {
            _scenarioRepository = scenarioRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        // No retries — scenario runs are long and not idempotent.
        [AutomaticRetry(Attempts = 0)]
        public async Task RunAsync(int scenarioId, string userName)
        {
            _logger.LogInformation("Scenario run started: id={ScenarioId} user={UserName}", scenarioId, userName);

            // RunScenarioAsync calls SymphonyConductor (uses raw connection string) then
            // GetDbContextAsync() for the stale-flag update — the latter requires an active UoW.
            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            var result = await _scenarioRepository.RunScenarioAsync(scenarioId, userName, true);
            await uow.CompleteAsync();

            if (result.Level == Microsoft.Extensions.Logging.LogLevel.Error)
            {
                var msg = result.ErrorMessage ?? "Scenario run failed.";
                _logger.LogError("Scenario run failed: id={ScenarioId} error={Error}", scenarioId, msg);
                throw new InvalidOperationException("Scenario run failed. Please contact support if the problem persists.");
            }

            if (result.Level == Microsoft.Extensions.Logging.LogLevel.Warning)
            {
                _logger.LogWarning("Scenario run completed with warnings: id={ScenarioId} message={Message}",
                    scenarioId, result.ErrorMessage);
            }
            else
            {
                _logger.LogInformation("Scenario run completed: id={ScenarioId}", scenarioId);
            }
        }
    }
}
