using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.PoolScoreParameters.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.Pools
{
    [Authorize]
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class PoolScoreParametersModel : PageModel
    {
        private readonly PoolScoreParameterAppService _appService;
        private readonly IPoolAppService _poolAppService;
        private readonly ICurrentUser _currentUser;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPermissionChecker _permissionChecker;

        public List<PoolScoreParameterDto> PoolScoreParameters { get; set; } = new();
        public Guid PoolId { get; set; }
        public string PoolName { get; set; }
        public string? CreatedBy { get; set; }
        public string Import { get; set; } = "false";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public PoolScoreParametersModel(
            PoolScoreParameterAppService appService,
            IPoolAppService poolAppService,
            ICurrentUser currentUser,
            IWebHostEnvironment hostingEnvironment,
            IPermissionChecker permissionChecker)
        {
            _appService = appService;
            _poolAppService = poolAppService;
            _currentUser = currentUser;
            _hostingEnvironment = hostingEnvironment;
            _permissionChecker = permissionChecker;
        }

        public async Task<IActionResult> OnGetAsync(Guid poolId, string import = "false")
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Default))
                return Forbid();

            PoolId = poolId;
            Import = import;
            ErrorMessage = TempData["Error"] as string;
            SuccessMessage = TempData["Success"] as string;
            CreatedBy = _currentUser.UserName;
            PoolScoreParameters = await _appService.GetListByPoolIdAsync(poolId);

            var pool = await _poolAppService.GetAsync(poolId);
            PoolName = pool?.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePoolScoreParameterAsync(PoolScoreParameterUpdateDto input)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Edit))
                return Forbid();

            await _appService.UpdateAsync(input);
            return RedirectToPage("/Pools/PoolScoreParameters", new { poolId = input.PoolId });
        }

        public async Task<IActionResult> OnPostCreatePoolScoreParameterAsync(PoolScoreParameterCreateDto input)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Create))
                return Forbid();

            input.CreatedBy = _currentUser.UserName;
            input.CreatedAt = DateTime.Now;
            await _appService.CreateAsync(input);
            return RedirectToPage("/Pools/PoolScoreParameters", new { poolId = input.PoolId });
        }

        public async Task<IActionResult> OnGetDeletePoolScoreParameterAsync(Guid poolId, string measure)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Delete))
                return Forbid();

            await _appService.DeleteAsync(poolId, measure);
            return RedirectToPage("/Pools/PoolScoreParameters", new { poolId });
        }

        public async Task<IActionResult> OnGetMeasuresAsync()
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Default))
                return Forbid();

            var measures = await _appService.GetMeasureNamesAsync();
            return new JsonResult(measures);
        }

        public async Task<IActionResult> OnGetExportAsync(Guid poolId)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Configuration.Default))
                return Forbid();

            var pool = await _poolAppService.GetAsync(poolId);
            var (content, fileName) = await _appService.ExportAsync(poolId, pool?.Name);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnGetDownloadScoreParametersTemplateAsync()
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.ImportConfiguration))
                return Forbid();

            var (content, fileName) = await _appService.DownloadScoreParametersTemplateAsync();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> OnPostCheckMeasureExistsAsync(PoolScoreParameterCreateDto input)
        {
            var existing = await _appService.GetByPoolIdAndMeasureAsync(input.PoolId, input.Measure);
            return new JsonResult(new { exists = existing != null });
        }

        public async Task<IActionResult> OnPostImportPoolScoreParametersAsync(IFormFile? excelfile, Guid PoolId, string tabname, bool deleteAll, bool keepAll, bool keepUserCreated)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.ImportConfiguration))
                return Forbid();

            if (excelfile == null || excelfile.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel (.xlsx) file.";
                return RedirectToPage("/Pools/PoolScoreParameters", new { poolId = PoolId });
            }

            string filePath = await SaveUploadedFileAsync(excelfile);

            try
            {
                bool fromScratch = !keepAll;
                await _appService.ImportAsync(filePath, tabname, PoolId, fromScratch, keepUserCreated);
                TempData["Success"] = "Configuration imported successfully.";
            }
            catch (UserFriendlyException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToPage("/Pools/PoolScoreParameters", new { poolId = PoolId });
            }
            catch (Exception ex)
            {
                // Anything not already turned into a friendly validation message (e.g. an
                // unanticipated data issue the legacy import pipeline chokes on) still needs to
                // land back on this page instead of the generic global error screen, which loses
                // all context and just bounces the user to /Scenarios/Index. Redirects without
                // import=true so the modal stays closed and only the error banner shows.
                TempData["Error"] = "Import failed: " + ex.Message;
                return RedirectToPage("/Pools/PoolScoreParameters", new { poolId = PoolId });
            }

            return RedirectToPage("/Pools/PoolScoreParameters", new { poolId = PoolId });
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
