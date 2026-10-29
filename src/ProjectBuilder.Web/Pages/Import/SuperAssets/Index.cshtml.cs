using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Web.Jobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.Import.SuperAssets
{
    public class IndexModel : PageModel
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public IndexModel(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(IFormFile excelfile, string tabname)
        {
            if (excelfile == null || excelfile.Length == 0)
                return new JsonResult(new { error = "No file uploaded." }) { StatusCode = 400 };

            var filePath = Path.Combine(Directory.CreateTempSubdirectory().FullName, Path.GetRandomFileName());
            using (var stream = new FileStream(filePath, FileMode.Create))
                await excelfile.CopyToAsync(stream);

            var jobId = _backgroundJobClient.Enqueue<ImportPoolDataJob>(
                job => job.RunAsync(filePath, PoolImportKind.SuperAssets, tabname));

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
