using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ProjectBuilder.Web.Jobs
{
    public static class PoolImportKind
    {
        public const string AssetSegmentation = "AssetSegmentation";
        public const string SuperAssets = "SuperAssets";
        public const string BridgeToPavement = "BridgeToPavement";
        public const string BridgeToPavementDss = "BridgeToPavementDss";
    }

    public class ImportPoolDataJob : ITransientDependency
    {
        private readonly IPoolRepository _poolRepository;
        private readonly ILogger<ImportPoolDataJob> _logger;

        public ImportPoolDataJob(IPoolRepository poolRepository, ILogger<ImportPoolDataJob> logger)
        {
            _poolRepository = poolRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        public Task RunAsync(string filePath, string importKind, string? tabName)
        {
            _logger.LogInformation("Pool data import started: kind={ImportKind}", importKind);
            try
            {
                switch (importKind)
                {
                    case PoolImportKind.AssetSegmentation:
                        _poolRepository.ImportAssetSegmentation(filePath, tabName);
                        break;
                    case PoolImportKind.SuperAssets:
                        _poolRepository.ImportSuperAssets(filePath, tabName);
                        break;
                    case PoolImportKind.BridgeToPavement:
                        _poolRepository.ImportBridgeToPavements(filePath, tabName);
                        break;
                    case PoolImportKind.BridgeToPavementDss:
                        _poolRepository.ImportBridgeToPavementsDss(filePath, tabName);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown import kind '{importKind}'.");
                }
                _logger.LogInformation("Pool data import completed: kind={ImportKind}", importKind);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
