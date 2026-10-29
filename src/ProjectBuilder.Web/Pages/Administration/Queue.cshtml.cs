using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace ProjectBuilder.Web.Pages.Administration
{
    [Authorize]
    public class QueueModel : PageModel
    {
        private static readonly string[] Statuses = { "Processing", "Succeeded", "Failed", "Deleted" };
        private const int PageSize = 25;

        private readonly IRunScenarioRepository _runScenarioRepository;
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly ILogger<QueueModel> _logger;

        public List<QueueRow> Rows { get; private set; } = new();

        public string Status { get; private set; } = "Processing";

        public int Page { get; private set; } = 1;

        public int TotalPages { get; private set; } = 1;

        [TempData]
        public string CancelError { get; set; }

        public QueueModel(
            IRunScenarioRepository runScenarioRepository,
            IIdentityUserRepository identityUserRepository,
            ILogger<QueueModel> logger)
        {
            _runScenarioRepository = runScenarioRepository;
            _identityUserRepository = identityUserRepository;
            _logger = logger;
        }

        public async Task OnGetAsync(string status, int page = 1)
        {
            Status = Statuses.FirstOrDefault(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase)) ?? "Processing";

            var monitor = JobStorage.Current.GetMonitoringApi();

            if (Status == "Processing")
            {
                Page = 1;
                TotalPages = 1;
                Rows = await BuildProcessingRowsAsync(monitor);
                return;
            }

            var totalCount = Status switch
            {
                "Succeeded" => monitor.SucceededListCount(),
                "Failed" => monitor.FailedCount(),
                _ => monitor.DeletedListCount()
            };

            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            Page = Math.Min(Math.Max(1, page), TotalPages);
            var from = (Page - 1) * PageSize;

            Rows = Status switch
            {
                "Succeeded" => await BuildSucceededRowsAsync(monitor, from, PageSize),
                "Failed" => await BuildFailedRowsAsync(monitor, from, PageSize),
                _ => await BuildDeletedRowsAsync(monitor, from, PageSize)
            };
        }

        private async Task<List<QueueRow>> BuildProcessingRowsAsync(IMonitoringApi monitor)
        {
            var processing = monitor.ProcessingJobs(0, 200)
                .Where(entry => entry.Value?.Job != null && entry.Value.LoadException == null)
                .OrderBy(entry => entry.Value.StartedAt)
                .ToList();

            var previousRuns = BuildPreviousRunLookup(monitor);
            var utcNow = DateTime.UtcNow;

            var rows = new List<QueueRow>();
            var position = 0;

            foreach (var entry in processing)
            {
                var jobId = entry.Key;
                var dto = entry.Value;
                var job = dto.Job;

                var (name, description) = await ResolveNameAndDescriptionAsync(job);

                DateTime? queuedTime = null;
                try
                {
                    queuedTime = monitor.JobDetails(jobId)?.CreatedAt;
                }
                catch
                {
                    // Hangfire storage hiccup — leave Queued Time blank rather than fail the whole page.
                }

                var previousRunKey = PreviousRunKey(job);
                TimeSpan? previousRunTime = previousRunKey != null && previousRuns.TryGetValue(previousRunKey, out var prev)
                    ? prev
                    : null;

                // Hangfire/SQL Server hand back UTC instants tagged as DateTimeKind.Unspecified.
                // Force Utc so ToString("o") emits the "Z" suffix — otherwise the browser
                // reinterprets the timestamp as local time and the ticking timer drifts by
                // the client's UTC offset.
                var startTimeUtc = dto.StartedAt.HasValue
                    ? DateTime.SpecifyKind(dto.StartedAt.Value, DateTimeKind.Utc)
                    : (DateTime?)null;

                rows.Add(new QueueRow
                {
                    Position = position++,
                    JobId = jobId,
                    Name = name,
                    Description = description,
                    QueuedBy = await ResolveQueuedByAsync(job),
                    QueuedTime = queuedTime,
                    StartTime = startTimeUtc,
                    CurrentRunTime = startTimeUtc.HasValue ? utcNow - startTimeUtc.Value : (TimeSpan?)null,
                    PreviousRunTime = previousRunTime
                });
            }

            return rows;
        }

        private async Task<List<QueueRow>> BuildSucceededRowsAsync(IMonitoringApi monitor, int from, int count)
        {
            var succeeded = monitor.SucceededJobs(from, count)
                .Where(entry => entry.Value?.Job != null)
                .OrderByDescending(entry => entry.Value.SucceededAt)
                .ToList();

            var rows = new List<QueueRow>();
            var position = from;

            foreach (var entry in succeeded)
            {
                var dto = entry.Value;
                var job = dto.Job;
                var (name, description) = await ResolveNameAndDescriptionAsync(job);

                var endTime = dto.SucceededAt;
                var duration = dto.TotalDuration.HasValue ? TimeSpan.FromMilliseconds(dto.TotalDuration.Value) : (TimeSpan?)null;
                var startTime = endTime.HasValue && duration.HasValue ? endTime.Value - duration.Value : (DateTime?)null;

                rows.Add(new QueueRow
                {
                    Position = position++,
                    JobId = entry.Key,
                    Name = name,
                    Description = description,
                    QueuedBy = await ResolveQueuedByAsync(job),
                    StartTime = startTime,
                    EndTime = endTime,
                    RunDuration = duration
                });
            }

            return rows;
        }

        private async Task<List<QueueRow>> BuildFailedRowsAsync(IMonitoringApi monitor, int from, int count)
        {
            var failed = monitor.FailedJobs(from, count)
                .Where(entry => entry.Value?.Job != null)
                .OrderByDescending(entry => entry.Value.FailedAt)
                .ToList();

            var rows = new List<QueueRow>();
            var position = from;

            foreach (var entry in failed)
            {
                var dto = entry.Value;
                var job = dto.Job;
                var (name, description) = await ResolveNameAndDescriptionAsync(job);

                rows.Add(new QueueRow
                {
                    Position = position++,
                    JobId = entry.Key,
                    Name = name,
                    Description = description,
                    QueuedBy = await ResolveQueuedByAsync(job),
                    EndTime = dto.FailedAt,
                    ErrorMessage = string.IsNullOrWhiteSpace(dto.ExceptionMessage) ? dto.Reason : dto.ExceptionMessage
                });
            }

            return rows;
        }

        private async Task<List<QueueRow>> BuildDeletedRowsAsync(IMonitoringApi monitor, int from, int count)
        {
            var deleted = monitor.DeletedJobs(from, count)
                .Where(entry => entry.Value?.Job != null)
                .OrderByDescending(entry => entry.Value.DeletedAt)
                .ToList();

            var rows = new List<QueueRow>();
            var position = from;

            foreach (var entry in deleted)
            {
                var dto = entry.Value;
                var job = dto.Job;
                var (name, description) = await ResolveNameAndDescriptionAsync(job);

                rows.Add(new QueueRow
                {
                    Position = position++,
                    JobId = entry.Key,
                    Name = name,
                    Description = description,
                    QueuedBy = await ResolveQueuedByAsync(job),
                    EndTime = dto.DeletedAt
                });
            }

            return rows;
        }

        public IActionResult OnPostCancel(string jobId, string status)
        {
            if (!string.IsNullOrWhiteSpace(jobId))
            {
                try
                {
                    BackgroundJob.Delete(jobId);
                }
                catch (Exception ex)
                {
                    // Deleting a job that's actively processing has to update the same Hangfire
                    // job row the running worker is touching — under load this can lock-wait and
                    // time out at the SQL level. Surface it instead of a 500 page.
                    _logger.LogWarning("Failed to cancel Hangfire job {JobId}: {Error}", ProjectBuilder.Web.Common.LogSanitizer.SanitizeForLog(jobId), ProjectBuilder.Web.Common.LogSanitizer.SanitizeForLog(ex.Message));
                    CancelError = "Could not cancel this job — the database is busy with the job that's currently running. Please try again in a moment.";
                }
            }

            return RedirectToPage(new { status });
        }

        private async Task<(string Name, string Description)> ResolveNameAndDescriptionAsync(Job job)
        {
            switch (job.Type.Name)
            {
                case nameof(ScenarioRunnerJob):
                {
                    var scenarioId = job.Args.Count > 0 && job.Args[0] is int id ? id : (int?)null;
                    if (scenarioId.HasValue)
                    {
                        var scenario = await _runScenarioRepository.FindScenarioByIdAsync(scenarioId);
                        if (scenario != null)
                        {
                            return (scenario.ScenarioName, "Run Simulation");
                        }
                    }
                    return (scenarioId.HasValue ? $"Scenario #{scenarioId}" : "Scenario", "Run Simulation");
                }

                case nameof(DssScenarioRunnerJob):
                    return ("DSS Scenario", "Run DSS Simulation");

                case nameof(CopyLibraryJob):
                {
                    var name = job.Args.Count > 0 && job.Args[0] is CreateCandidatePoolDto dto ? dto.Name : null;
                    return (string.IsNullOrWhiteSpace(name) ? "Library" : name, "Copy Treatment Pool Library");
                }

                case nameof(ImportLibraryJob):
                {
                    var filePath = job.Args.Count > 0 ? job.Args[0] as string : null;
                    var importType = job.Args.Count > 2 ? job.Args[2] as string : null;
                    var fileName = string.IsNullOrWhiteSpace(filePath) ? "Import" : System.IO.Path.GetFileName(filePath);
                    var description = string.IsNullOrWhiteSpace(importType)
                        ? "Import Treatments"
                        : $"Import Treatments ({importType})";
                    return (fileName, description);
                }

                case nameof(FetchBridgeReferenceDataJob):
                {
                    var requestedBy = job.Args.Count > 0 ? job.Args[0] as string : null;
                    return (string.IsNullOrWhiteSpace(requestedBy) ? "Bridge Reference Data" : requestedBy, "Fetch Bridge Reference Data");
                }

                case nameof(ImportPoolDataJob):
                {
                    var filePath = job.Args.Count > 0 ? job.Args[0] as string : null;
                    var importKind = job.Args.Count > 1 ? job.Args[1] as string : null;
                    var fileName = string.IsNullOrWhiteSpace(filePath) ? "Import" : System.IO.Path.GetFileName(filePath);
                    var description = importKind switch
                    {
                        PoolImportKind.AssetSegmentation => "Import MAS",
                        PoolImportKind.SuperAssets => "Import Super Assets",
                        PoolImportKind.BridgeToPavement => "Import B2P Crosswalk",
                        _ => "Import Pool Data"
                    };
                    return (fileName, description);
                }

                default:
                    return (job.Type.Name, job.Method.Name);
            }
        }

        private async Task<string> ResolveQueuedByAsync(Job job)
        {
            if (job.Type.Name == nameof(ScenarioRunnerJob) && job.Args.Count > 1 && job.Args[1] is string userName
                && !string.IsNullOrWhiteSpace(userName))
            {
                return userName;
            }

            if (job.Type.Name == nameof(CopyLibraryJob) && job.Args.Count > 1 && job.Args[1] is Guid userId)
            {
                var user = await _identityUserRepository.FindAsync(userId);
                return user?.UserName ?? userId.ToString();
            }

            return "—";
        }

        private static string PreviousRunKey(Job job)
        {
            object keyArg = job.Type.Name switch
            {
                nameof(ScenarioRunnerJob) => job.Args.Count > 0 ? job.Args[0] : null,
                nameof(DssScenarioRunnerJob) => job.Args.Count > 0 ? job.Args[0] : null,
                nameof(ImportLibraryJob) => job.Args.Count > 1 ? job.Args[1] : null,
                nameof(ImportPoolDataJob) => job.Args.Count > 1 ? job.Args[1] : null,
                _ => null
            };

            return keyArg == null ? null : $"{job.Type.Name}:{keyArg}";
        }

        private static Dictionary<string, TimeSpan> BuildPreviousRunLookup(IMonitoringApi monitor)
        {
            var latest = new Dictionary<string, (DateTime When, TimeSpan Duration)>();

            void Consider(Job job, DateTime? when, long? durationMs)
            {
                var key = PreviousRunKey(job);
                if (key == null || when == null || durationMs == null)
                {
                    return;
                }

                if (!latest.TryGetValue(key, out var existing) || when.Value > existing.When)
                {
                    latest[key] = (when.Value, TimeSpan.FromMilliseconds(durationMs.Value));
                }
            }

            try
            {
                foreach (var entry in monitor.SucceededJobs(0, 100))
                {
                    var dto = entry.Value;
                    if (dto?.Job != null && dto.LoadException == null)
                    {
                        Consider(dto.Job, dto.SucceededAt, dto.TotalDuration);
                    }
                }
            }
            catch
            {
                // Best-effort — leave Previous Run Time blank if storage can't be read.
            }

            return latest.ToDictionary(kv => kv.Key, kv => kv.Value.Duration);
        }
    }

    public class QueueRow
    {
        public int Position { get; set; }
        public string JobId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string QueuedBy { get; set; }
        public DateTime? QueuedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public TimeSpan? CurrentRunTime { get; set; }
        public TimeSpan? PreviousRunTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? RunDuration { get; set; }
        public string ErrorMessage { get; set; }
    }
}
