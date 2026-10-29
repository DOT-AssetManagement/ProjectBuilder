using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using ProjectBuilder.Models.CandidatePool;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Models.Treatment;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using ProjectBuilder.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.CandidatePools
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWorkManager _filter;
        private readonly ICandidatePoolRepository _candidate;
        private readonly ITreatmentRepository _treatment;
        private readonly IRunScenarioRepository _scenario;
        private readonly IdentityUserManager _userManager;
        private readonly ICurrentUser _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICandidatePoolAppService _candidatePoolAppService;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IUnitOfWorkManager filter, ICandidatePoolRepository candidate, IRunScenarioRepository scenario, IdentityUserManager userManager, ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor, ITreatmentRepository treatment, ICandidatePoolAppService candidatePoolAppService, IUserRepository userRepository, IPermissionChecker permissionChecker, ILogger<IndexModel> logger)
        {
            _filter = filter;
            _candidate = candidate;
            _scenario = scenario;
            _userManager = userManager;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _treatment = treatment;
            _candidatePoolAppService = candidatePoolAppService;
            _userRepository = userRepository;
            _permissionChecker = permissionChecker;
            _logger = logger;
        }

        [BindProperty(Name = "libraryId")]
        public Guid? Id { get; set; }

        public bool IsImporting { get; set; } = false;
        [BindProperty(Name = "isshared")]
        public bool IsShared { get; set; }
        public bool FromScenario { get; set; } = false;
        [BindProperty(Name = "isemptylibrary")]
        public string SearchString { get; set; }
        public bool IsEmptyLibrary { get; set; } = false;
        public CandidatePoolDto Pools { get; set; } = new();
        public Guid Libid { get; set; }
        public ImportMASAndB2PViewModel ImportData { get; set; }

        public async Task<IActionResult> OnGet(int? scenid, bool? importing, string searchString = "")
        {
            Pools = await _candidatePoolAppService.GetCandidatePoolDataAsync(scenid, importing, searchString);

            if (Pools == null)
            {
                return RedirectToPage("/CandidatePools/Index");
            }

            try
            {
                var currentUser = await GetOrCreateUser();
                ApplayUserFilter(currentUser.Id);

                Pools = await _candidatePoolAppService.ListAsync(scenid, importing);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load candidate pools list.");
                HttpContext.Session.SetString("ErrorDetails", "Please contact support if the problem persists.");
                return RedirectToPage("/ErrorPage/Error");
            }
        }

        public async Task<IActionResult> OnGetListAsync(int? scenid, bool? importing)
        {
            try
            {
                var currentUser = await GetOrCreateUser();
                ApplayUserFilter(currentUser.Id);

                Pools = await _candidatePoolAppService.ListAsync(scenid, importing);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load candidate pools list.");
                HttpContext.Session.SetString("ErrorDetails", "Please contact support if the problem persists.");
                return RedirectToPage("/ErrorPage/Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostCreateLibraryAsync(CreateCandidatePoolDto libraryViewModel)
        {
            ValidateLibrary(libraryViewModel);
            if (!ModelState.IsValid)
            {

            }
            //return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to create a library." });
            var currentUser = await GetOrCreateUser();
            await _candidatePoolAppService.CreateNewLibrary(libraryViewModel, currentUser.Id);
            if (libraryViewModel.CreatScenpage == true)
            {
                return RedirectToAction("CreateScenario", "Scenarios");
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateScenaLibrary([FromBody] CreateCandidatePoolDto libraryViewModel)
        {
            ValidateLibrary(libraryViewModel);
            if (!ModelState.IsValid)
            {
                return Page();
            }
            //return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to create a library." });
            var currentUser = await GetOrCreateUser();
            await _candidatePoolAppService.CreateNewLibrary(libraryViewModel, currentUser.Id);
            return new JsonResult(Libid);
        }

        private void ValidateLibrary(CreateCandidatePoolDto libraryViewModel)
        {
            if (ModelState.IsValid)
                return;
            if (!ModelState.ContainsKey("assettype") || (ModelState.TryGetValue("assettype", out ModelStateEntry? modelState) && modelState?.ValidationState == ModelValidationState.Invalid))
                libraryViewModel.SelectedAsset = null;
            foreach (var key in ModelState.Keys)
            {
                if (key == "libraryname")
                    continue;
                var modelstate = ModelState.GetValueOrDefault(key);
                if (modelstate is not null)
                    modelstate.ValidationState = ModelValidationState.Skipped;
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditLibrary(UpdateCandidatePoolDto libraryViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = await _candidatePoolAppService.EditLibraryAsync(libraryViewModel);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToPage("/CandidatePools/Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateSharedLibrary(CreateCandidatePoolDto libraryViewModel)
        {   
            var currentUser = await GetOrCreateUser();
            await _candidatePoolAppService.CreateNewLibrary(libraryViewModel, currentUser.Id, $"{libraryViewModel.Id}");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLibrary(CandidatePoolDto libraryViewModel)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Delete))
                return Forbid();

            var (success, error) = await _candidate.DeactivateCandidatePool(libraryViewModel.Id);
            if (!success)
            {
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(error)
                    ? "Could not delete the selected library."
                    : $"Could not delete the selected library: {error}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetLibrary([FromBody] Guid id)
        {
            if (id == null)
            {
                return BadRequest("Library ID is required.");
            }

            var library = await _candidatePoolAppService.GetAsync(id);

            if (library == null)
            {
                return NotFound("Library not found.");
            }

            return new JsonResult(library);
        }

        [HttpPost]
        public async Task<IActionResult> GetLibraryWithScenario([FromBody] Guid id)
        {
            //var library = await _candidate.FindAsync(id);
            var library = await _candidatePoolAppService.GetAsync(id);
            var AllScenarios = await _scenario.GetListAsync();
            var scenarios = AllScenarios.Find(a => a.LibraryId == id);


            var response = new
            {
                ScenarioName = scenarios?.ScenarioName ?? string.Empty,
                LibraryName = library.Name
            };

            return new JsonResult(response);

        }

        [HttpPost]
        public async Task<JsonResult> CheckLibraryName(string libraryname, string libraryid)
        {
            var isValid = await _candidatePoolAppService.CheckLibraryNameAsync(libraryname, libraryid);

            if (!isValid)
            {
                return new JsonResult(new { error = "Name already exists" }) { StatusCode = 400 };
            }

            return new JsonResult(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ImportTreatments(ImportTreatmentViewModel data)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return new JsonResult(errors) { StatusCode = 400 };
            }

            var errormessage = await _candidatePoolAppService.ImportTreatmentsAsync(data);

            return string.IsNullOrEmpty(errormessage)
                ? new JsonResult(new { success = true })
                : new JsonResult(new { error = errormessage }) { StatusCode = 400 };
        }

        [HttpPost]
        public async Task<IActionResult> ImportMASAndB2P(ImportMASAndB2PViewModel data)
        {
            if (!ModelState.IsValid)
                return BadRequest("An error occurred while trying to upload the file.");

            var errormessage = await _candidatePoolAppService.ImportMASAndB2PAsync(data);

            return data.IsMAS
                ? RedirectToPage("ImportMAS")
                : RedirectToPage("ImportB2P");
        }
        public IActionResult ImportB2P()
        {
            ImportData = new ImportMASAndB2PViewModel { IsMAS = false };
            return Page();
        }

        public IActionResult ImportMAS()
        {
            ImportData = new ImportMASAndB2PViewModel { IsMAS = true };
            return Page();
        }

        public async Task<Volo.Abp.Identity.IdentityUser> GetOrCreateUser()
        {
            // Resolve the current ABP identity user (AbpUsers) by id, falling back to email.
            Volo.Abp.Identity.IdentityUser user = null;
            if (_currentUser.Id.HasValue)
                user = await _userManager.FindByIdAsync(_currentUser.Id.Value.ToString());

            if (user == null)
            {
                var email = _currentUser.Email;
                if (!string.IsNullOrEmpty(email))
                    user = await _userManager.FindByEmailAsync(email);
            }

            if (user == null)
                throw new Exception("User is not authenticated.");

            return user;
        }

        public void ApplayUserFilter(Guid userId)
        {
            _candidate.UserId = userId;
        }
    }
}









