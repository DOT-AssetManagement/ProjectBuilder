
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.WorkCandidates;
using ProjectBuilder.Models.WorkCandidates.Dtos;
using ProjectBuilder.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;


namespace ProjectBuilder.Web.Pages.WorkCandidates
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IWorkCandidatesAppService _workCandidatesAppService;
        private readonly IPoolAppService _poolAppService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ILogger<IndexModel> _logger;

        public List<WorkCandidatesDto> WorkCandidates { get; set; }

        public string PoolId { get; set; }
        public string PoolName { get; set; }
        public string ProjectCandidateId { get; set; }
        public string Import { get; set; } = "false";

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(
            IWorkCandidatesAppService workCandidatesAppService,
            IPoolAppService poolAppService,
            IWebHostEnvironment hostingEnvironment,
            IPermissionChecker permissionChecker,
            ILogger<IndexModel> logger)
        {
            _workCandidatesAppService = workCandidatesAppService;
            _poolAppService = poolAppService;
            _hostingEnvironment = hostingEnvironment;
            _permissionChecker = permissionChecker;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet(Guid id, Guid Projectid, string import = "false")
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Default))
                return Forbid();

            WorkCandidates = await _workCandidatesAppService.GetAllByPoolOrProjectIdAsync(id, Projectid);
            PoolId = id.ToString();
            Import = import;

            var pool = await _poolAppService.GetAsync(id);
            PoolName = pool?.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostCreate(CreateWorkCandidatesDto input, Guid PoolId)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Create))
                return Forbid();

            await _workCandidatesAppService.CreateWorkCandidateAsync(input, PoolId);
            return RedirectToPage("/WorkCandidates/Index", new { id = PoolId });
        }

        public async Task<IActionResult> OnPostUpdate(Guid id, UpdateWorkCandidatesDto input, Guid PoolId)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Edit))
                return Forbid();

            await _workCandidatesAppService.UpdateWorkCandidateAsync(id, input, PoolId);
            return RedirectToPage("/WorkCandidates/Index", new { id = PoolId });
        }

        public async Task<IActionResult> OnGetExport(Guid poolId)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Default))
                return Forbid();

            var pool = await _poolAppService.GetAsync(poolId);
            var (content, fileName) = await _workCandidatesAppService.ExportAsync(poolId, pool?.Name);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnGetDelete(Guid id, Guid PoolId)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Delete))
                return Forbid();

            await _workCandidatesAppService.DeleteAsync(id);
            return RedirectToPage("/WorkCandidates/Index", new { id = PoolId });
        }

        public async Task<IActionResult> OnPostImportWorkCandidatesAsync(IFormFile? excelfile, Guid PoolId, string tabname, bool deleteAll = true)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.ImportWorkCandidates))
                return Forbid();

            if (excelfile == null || excelfile.Length == 0)
            {
                ErrorMessage = "Please upload a valid Excel (.xlsx) file.";
                return RedirectToPage("/WorkCandidates/Index", new { id = PoolId });
            }

            string filePath = await SaveUploadedFileAsync(excelfile);

            try
            {
                _poolAppService.ImportWorkCandidates(filePath, "Northwest RPO", tabname, PoolId, forgiving: true, deleteAll: deleteAll);
                SuccessMessage = "Work candidates imported successfully.";
            }
            catch (UserFriendlyException ex)
            {
                ErrorMessage = ex.Message;
                return RedirectToPage("/WorkCandidates/Index", new { id = PoolId });
            }
            catch (Exception ex)
            {
                // Anything not already turned into a friendly validation message still needs to
                // land back on this page instead of the generic global error screen, which loses
                // all context and bounces the user to /Scenarios/Index. Redirects without
                // import=true so the modal stays closed and only the error banner shows.
                _logger.LogError(ex, "Failed to import work candidates for pool {PoolId}.", PoolId);
                ErrorMessage = "Import failed. Please try again.";
                return RedirectToPage("/WorkCandidates/Index", new { id = PoolId });
            }

            return RedirectToPage("/WorkCandidates/Index", new { id = PoolId });
        }

        private async Task<string> SaveUploadedFileAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            return filePath;
        }
    }
}
