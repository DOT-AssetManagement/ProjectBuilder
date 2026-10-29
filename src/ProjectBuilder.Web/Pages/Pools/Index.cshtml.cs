using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.Pool.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.Pools
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPoolAppService _poolAppService;
        private readonly IDssScenarioRepository _dssScenarioRepository;
        private readonly IWorkCandidatesRepository _workCandidatesRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ICurrentUser _currentUser;
        private readonly string _connectionString;
        private readonly ILogger<IndexModel> _logger;

        public List<CandidatePoolDto> Pools { get; set; }
        public Guid CurrentUserId => _currentUser.Id ?? Guid.Empty;
        public bool IsAdmin { get; private set; }

        public IndexModel(
            IPoolAppService poolAppService,
            IDssScenarioRepository dssScenarioRepository,
            IWorkCandidatesRepository workCandidatesRepository,
            IBackgroundJobClient backgroundJobClient,
            IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration,
            IPermissionChecker permissionChecker,
            ICurrentUser currentUser,
            ILogger<IndexModel> logger)
        {
            _poolAppService = poolAppService;
            _dssScenarioRepository = dssScenarioRepository;
            _workCandidatesRepository = workCandidatesRepository;
            _connectionString = configuration.GetConnectionString("Default");
            _backgroundJobClient = backgroundJobClient;
            _hostingEnvironment = hostingEnvironment;
            _permissionChecker = permissionChecker;
            _currentUser = currentUser;
            _logger = logger;
        }

        public string SearchString { get; set; }
        public string PoolId { get; set; }
        public string Import { get; set; }
        public string PoolName { get; set; }
        public string SessionMsg { get; set; }
        public PoolDto PoolData { get; set; }
        public Dictionary<Guid, int> WorkCandidateCounts { get; set; } = new();
        public async Task OnGet(String searchString = "", String poolId = "", String import = "false")
        {
            SessionMsg = HttpContext.Session.GetString("SessionMsg");
            HttpContext.Session.Remove("SessionMsg");

            IsAdmin = await _permissionChecker.IsGrantedAsync(AdministrationPermissions.Default);

            SearchString = searchString;
            Pools = await _poolAppService.GetAllAsync(searchString, poolId);
            var poolIds = Pools?.Select(p => p.Id).ToList() ?? new List<Guid>();
            WorkCandidateCounts = await _workCandidatesRepository.GetCountsByPoolIdsAsync(poolIds);

            PoolId = poolId;
            Import = import;
            PoolName = "";
            if (poolId != "")
            {
                Guid poolIdNew = Guid.Parse(poolId);
                PoolData = await _poolAppService.GetAsync(poolIdNew);
                PoolName = PoolData.Name;
            }
        }

        [BindProperty]
        public PoolDto Pool { get; set; }
        public async Task<IActionResult> OnGetGetPool(Guid id)
        {
            Pool = await _poolAppService.GetAsync(id);
            return new JsonResult(Pool);
        }

        public async Task<IActionResult> OnPostCreateAsync(CreatePoolDto newPool)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Create))
                return Forbid();

            if (ModelState.IsValid)
            {

                Pool = await _poolAppService.CheckName(newPool.Name);
                if (Pool != null)
                {                   
                    var result = new { success = false, message = "Candidate + Pool name exists. Please try with new name." };
                    return new JsonResult(result);

                }
                else
                {
                    Guid? poolId = await _poolAppService.Create(newPool);

                    var createdBy = User.Identity?.Name ?? "system";
                    await _poolAppService.CreateDssScenarioAsync((Guid)poolId, newPool.Name, newPool.Description, createdBy);

                    HttpContext.Session.SetString("SessionMsg", "Candidate + pool created. You can import your work candidates now.");

                    var result = new { success = true };
                    return new JsonResult(result);
                }

                
            }
            else
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return new JsonResult(new { success = false, message = $"Validation failed: {errors}" });
            }
        }

        // "Delete" on the Pools grid used to only call ClearAsync (wipes this pool's Work/Project
        // Candidates via UspDeletePoolWorkData) without ever removing the tbl_dss_UserPools row
        // itself — so the pool always reappeared in the list after "deleting" it. This now
        // clears candidate data, removes the pool's own 1:1 DSS scenario (orphaned otherwise —
        // nothing can look it up again once the pool is gone), then deletes the pool row.
        public async Task<IActionResult> OnGetDeletePool(Guid id)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Clear))
                return Forbid();

            try
            {
                await _poolAppService.ClearAsync(id);
                await _dssScenarioRepository.DeleteByPoolIdAsync(id);
                await _poolAppService.DeleteAsync(id);
                HttpContext.Session.SetString("SessionMsg", "Candidate + pool deleted.");
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete pool {PoolId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Failed to delete the pool. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetHasWorkCandidatesAsync(Guid id)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Run))
                return Forbid();

            var hasWorkCandidates = await _workCandidatesRepository.HasByPoolIdAsync(id);
            return new JsonResult(new { hasWorkCandidates });
        }

        // Runs a DSS pool through the DEA engine (DataManager.RunScenario), not the PB
        // treatment-optimization pipeline (tbl_pb_Scenarios) — a pool has no library
        // treatments, so running it through the PB pipeline always reports "produced no projects".
        public async Task<IActionResult> OnPostRunDssScenarioAsync(Guid poolId)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Run))
                return Forbid();

            if (!await _workCandidatesRepository.HasByPoolIdAsync(poolId))
                return BadRequest(new { error = "No work candidates available for this candidate +." });

            var dssScenario = await _dssScenarioRepository.GetByPoolIdAsync(poolId);
            if (dssScenario == null)
            {
                // Self-heals pools created before DSS scenario auto-creation existed (or whose
                // creation failed), mirroring OnPostEnsureScenarioAsync's PB-scenario self-heal.
                var pool = await _poolAppService.GetAsync(poolId);
                if (pool == null)
                    return NotFound(new { error = "Candidate + pool not found." });

                var createdBy = User.Identity?.Name ?? "system";
                dssScenario = await _dssScenarioRepository.CreateForPoolAsync(poolId, pool.Name, pool.Description, createdBy);
            }

            var jobId = _backgroundJobClient.Enqueue<DssScenarioRunnerJob>(job => job.RunAsync(dssScenario.Id));
            return new JsonResult(new { success = true, jobId });
        }

        public async Task<IActionResult> OnPostUpdatePoolAsync(UpdatePoolDto input)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.Edit))
                return Forbid();

            if (string.IsNullOrWhiteSpace(input?.Name) || string.IsNullOrWhiteSpace(input.Description))
            {
                return new JsonResult(new { success = false, message = "Candidate + Name and Description are required." });
            }

            input.Name = input.Name.Trim();
            input.Description = input.Description.Trim();

            var existing = await _poolAppService.CheckName(input.Name);
            if (existing == null || existing.Id == input.Id)
            {
                await _poolAppService.UpdateAsync(input.Id, input);
                await _poolAppService.UpdateDssScenarioAsync(input.Id, input.Name, input.Description);
                HttpContext.Session.SetString("SessionMsg", "Candidate + pool updated.");
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false, message = "Candidate + Pool name exists. Please try with a new name." });
        }



        public async Task<IActionResult> OnPostImportWorkCandidatesAsync(IFormFile? excelfile, Guid PoolId, string tabname)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.ImportWorkCandidates))
                return Forbid();

            if (excelfile == null || excelfile.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            string filePath = await SaveUploadedFile(excelfile);

            if (string.IsNullOrEmpty(filePath))
            {
                return StatusCode(500, new { success = false, message = "File upload failed" });
            }

            
            _poolAppService.ImportWorkCandidates(filePath, "Northwest RPO", tabname, PoolId, forgiving: true);

            HttpContext.Session.SetString("SessionMsg", "Work Candidates are imported. You can run the candidate + pool now.");

            return RedirectToPage("/Pools/Index");
        }

        private async Task<string> SaveUploadedFile(IFormFile file)
        {
            try
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                // Log the exception
                return null;
            }
        }


        public async Task<IActionResult> OnPostExportAsync(Guid poolId, string? effThreshold)
        {
            if (!await _permissionChecker.IsGrantedAsync(SupplementalCandidatesPermissions.PoolsPageActions.ExportProjectsOrCandidate))
                return Forbid();

            if (string.IsNullOrWhiteSpace(effThreshold)
                || !double.TryParse(effThreshold.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold)
                || threshold < 0 || threshold > 1)
            {
                return BadRequest("Relative efficiency threshold must be a number from 0.0 to 1.0. Blank spaces are not allowed.");
            }

            string xlsxFilePath = Path.Combine(Path.GetTempPath(), $"Export_{Guid.NewGuid()}.xlsx");

            try
            {
                var ok = PBLogic.DataManager.ExportPoolData(_connectionString, poolId, threshold, xlsxFilePath, out string errorMessage);

                if (!ok)
                {
                    _logger.LogError("Failed to export pool {PoolId}: {Error}", poolId, errorMessage);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Export failed. Please try again.");
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(xlsxFilePath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export pool {PoolId}.", poolId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Export failed. Please try again.");
            }
            finally
            {
                if (System.IO.File.Exists(xlsxFilePath))
                {
                    System.IO.File.Delete(xlsxFilePath);
                }
            }
        }

    }
}


