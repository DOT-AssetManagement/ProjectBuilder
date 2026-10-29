using Hangfire;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Repositories;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ProjectBuilder.Web.Jobs
{
    public class ImportLibraryJob : ITransientDependency
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly ILogger<ImportLibraryJob> _logger;

        public ImportLibraryJob(
            ITreatmentRepository treatmentRepository,
            ILogger<ImportLibraryJob> logger)
        {
            _treatmentRepository = treatmentRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task RunAsync(
            string filePath,
            Guid libraryId,
            string importType,
            string? tabName,
            bool fromScratch,
            bool keepAll)
        {
            _logger.LogInformation("Treatment import started: library={LibraryId} type={ImportType}", libraryId, ProjectBuilder.Web.Common.LogSanitizer.SanitizeForLog(importType));
            try
            {
                var validationError = _treatmentRepository.ValidateTreatmentExcelFile(filePath, tabName, importType);
                if (!string.IsNullOrWhiteSpace(validationError))
                    throw new InvalidOperationException(validationError);

                var source = importType switch
                {
                    "P" => "PAMS",
                    "D" => "DecisionSpace",
                    _   => "BAMS"
                };

                var importError = await _treatmentRepository.ImportTreatmentsFromExcelFile(
                    source, importType, filePath, tabName, libraryId, fromScratch, keepAll);

                if (!string.IsNullOrWhiteSpace(importError))
                    throw new InvalidOperationException(importError);

                _logger.LogInformation("Treatment import completed: library={LibraryId}", libraryId);
            }
            finally
            {
                _treatmentRepository.DeleteExcelFile(filePath);
            }
        }
    }
}
