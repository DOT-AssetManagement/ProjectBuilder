using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Web.Jobs;
using System;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.Import.BridgeReferenceData
{
    public class IndexModel : PageModel
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICurrentUser _currentUser;

        public IndexModel(IBackgroundJobClient backgroundJobClient, ICurrentUser currentUser)
        {
            _backgroundJobClient = backgroundJobClient;
            _currentUser = currentUser;
        }

        public void OnGet() { }

        public IActionResult OnPostAsync()
        {
            var requestedBy = _currentUser.UserName ?? _currentUser.Email ?? "Admin";
            var jobId = _backgroundJobClient.Enqueue<FetchBridgeReferenceDataJob>(
                job => job.RunAsync(requestedBy));

            return new JsonResult(new { success = true, jobId });
        }

        public IActionResult OnGetJobStatus(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
                return new JsonResult(new { status = "NotFound" });

            var connection = JobStorage.Current.GetConnection();
            var jobData = connection.GetJobData(jobId);
            if (jobData == null)
                return new JsonResult(new { status = "NotFound" });

            var stateData = connection.GetStateData(jobId);
            string? message = stateData?.Reason;

            if (string.Equals(jobData.State, "Failed", StringComparison.OrdinalIgnoreCase)
                && stateData?.Data != null
                && stateData.Data.TryGetValue("ExceptionMessage", out var exMsg)
                && !string.IsNullOrWhiteSpace(exMsg))
            {
                message = exMsg;
            }

            return new JsonResult(new { status = jobData.State, message });
        }
    }
}
