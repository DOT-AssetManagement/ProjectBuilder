using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using System;
using System.Collections.Generic;

namespace ProjectBuilder.Web.Jobs
{
    /// <summary>
    /// Prevents duplicate Hangfire scenario runs for the same ScenID.
    /// </summary>
    internal static class ScenarioRunJobHelper
    {
        private const string JobTypeName = "ScenarioRunnerJob";

        public static bool HasPendingJob(int scenarioId)
        {
            if (scenarioId <= 0)
                return false;

            try
            {
                var monitor = JobStorage.Current.GetMonitoringApi();

                foreach (var job in monitor.ProcessingJobs(0, 100))
                {
                    if (JobTargetsScenario(job.Value?.Job, scenarioId))
                        return true;
                }

                foreach (var queue in monitor.Queues())
                {
                    var enqueued = monitor.EnqueuedJobs(queue.Name, 0, 200);
                    foreach (var job in enqueued)
                    {
                        if (JobTargetsScenario(job.Value?.Job, scenarioId))
                            return true;
                    }
                }

                var scheduled = monitor.ScheduledJobs(0, 200);
                foreach (var job in scheduled)
                {
                    if (JobTargetsScenario(job.Value?.Job, scenarioId))
                        return true;
                }
            }
            catch
            {
                // If Hangfire storage is unavailable, fall back to DB lock checks only.
            }

            return false;
        }

        private static bool JobTargetsScenario(Job? job, int scenarioId)
        {
            if (job == null || job.Type?.Name != JobTypeName)
                return false;

            if (job.Args == null || job.Args.Count == 0)
                return false;

            return job.Args[0] is int id && id == scenarioId
                || job.Args[0] is long longId && longId == scenarioId
                || string.Equals(job.Args[0]?.ToString(), scenarioId.ToString(), StringComparison.Ordinal);
        }
    }
}
