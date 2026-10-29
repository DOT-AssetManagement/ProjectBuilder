using Hangfire;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Repositories;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ProjectBuilder.Web.Jobs
{
    public class DssScenarioRunnerJob : ITransientDependency
    {
        private readonly IDssScenarioRepository _dssScenarioRepository;
        private readonly ILogger<DssScenarioRunnerJob> _logger;

        public DssScenarioRunnerJob(
            IDssScenarioRepository dssScenarioRepository,
            ILogger<DssScenarioRunnerJob> logger)
        {
            _dssScenarioRepository = dssScenarioRepository;
            _logger = logger;
        }

        // No retries — scenario runs are long and not idempotent.
        [AutomaticRetry(Attempts = 0)]
        public async Task RunAsync(Guid scenarioId)
        {
            _logger.LogInformation("DSS scenario run started: id={ScenarioId}", scenarioId);

            var (ok, errorMessage) = await _dssScenarioRepository.RunScenarioAsync(scenarioId);

            if (!ok)
            {
                var msg = errorMessage ?? "DSS scenario run failed.";
                _logger.LogError("DSS scenario run failed: id={ScenarioId} error={Error}", scenarioId, msg);
                throw new InvalidOperationException(msg);
            }

            _logger.LogInformation("DSS scenario run completed: id={ScenarioId}", scenarioId);
        }
    }
}
