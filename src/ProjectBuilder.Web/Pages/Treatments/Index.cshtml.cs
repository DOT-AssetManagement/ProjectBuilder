using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.CandidatePool;
using ProjectBuilder.Models.ProjectTreatment;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Treatment;
using ProjectBuilder.Models.UserTreatment.Dtos;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
using ProjectBuilder.Permissions;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;

namespace ProjectBuilder.Web.Pages.Treatments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ITreatmentAppService _treatmentAppService;
        private readonly IProjectTreatmentAppService _projectTreatmentAppService;
        private readonly ICandidatePoolAppService _candidatePoolAppService;
        private readonly IScenarioAppService _scenarioAppService;
        private readonly IPermissionChecker _permissionChecker;

        public IndexModel(
            ITreatmentAppService treatmentAppService,
            IProjectTreatmentAppService projectTreatmentAppService,
            ICandidatePoolAppService candidatePoolAppService,
            IScenarioAppService scenarioAppService,
            IPermissionChecker permissionChecker)
        {
            _treatmentAppService = treatmentAppService;
            _projectTreatmentAppService = projectTreatmentAppService;
            _candidatePoolAppService = candidatePoolAppService;
            _scenarioAppService = scenarioAppService;
            _permissionChecker = permissionChecker;
        }

        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; } = "";
        public string ScenarioName { get; set; } = "";
        public List<UserTreatmentDto> Treatments { get; set; } = new();

        // Seed data for the create/edit modals.
        public List<UserTreatmentTypeDto> TreatmentTypes { get; set; } = new();
        public List<TreatmentOptionVm> TreatmentOptions { get; set; } = new();

        public async Task OnGetAsync(Guid id)
        {
            LibraryId = id;

            try
            {
                var library = await _candidatePoolAppService.GetAsync(id);
                LibraryName = library?.Name ?? "";
            }
            catch
            {
                LibraryName = "";
            }

            try
            {
                var scenario = await _scenarioAppService.GetScenarioByPoolId(id);
                ScenarioName = scenario?.ScenarioName ?? "";
            }
            catch
            {
                ScenarioName = "";
            }

            Treatments = await _treatmentAppService.GetByLibraryIdAsync(id);

            // Library-scoped treatment type categories (legacy TreatmentsController behavior),
            // rather than the global UserTreatmentTypes list.
            TreatmentTypes = await _treatmentAppService.GetTreatmentTypesByLibraryAsync(id);

            // Distinct treatment names (with the asset types they appear under) for the Create modal.
            TreatmentOptions = Treatments
                .Where(t => !string.IsNullOrWhiteSpace(t.Treatment))
                .GroupBy(t => t.Treatment.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new TreatmentOptionVm
                {
                    Name = g.First().Treatment.Trim(),
                    AssetTypes = string.Join(",", g
                        .Select(t => t.AssetType)
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .Distinct()
                        .OrderBy(a => a))
                })
                .OrderBy(o => o.Name)
                .ToList();
        }

        // ── Cascading lookups (District -> County -> Route -> Section) ──
        // Sourced from master reference data (tbl_pams_MaintainableAssetsSegmentation / tbl_pb_BridgeReferenceDataFromPennshare), same as the ProjectTreatments Create modal.
        public async Task<IActionResult> OnGetDistrictsAsync(string assetType)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Default))
                return Forbid();

            return new JsonResult(await _projectTreatmentAppService.GetMasterDistrictsAsync(assetType));
        }

        public async Task<IActionResult> OnGetCountiesAsync(int district, string assetType)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Default))
                return Forbid();

            var counties = await _projectTreatmentAppService.GetMasterCountiesAsync(assetType, district);
            return new JsonResult(counties.Select(c => new { countyId = c.Id, countyFullName = $"{c.Id}-{c.CountyName}" }));
        }

        public async Task<IActionResult> OnGetRoutesAsync(int district, int county, string assetType)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Default))
                return Forbid();

            return new JsonResult(await _projectTreatmentAppService.GetMasterRoutesAsync(assetType, district, county));
        }

        // Option value stays "From-To" since CreateTreatment splits Section on '-' into FromSection/ToSection.
        public async Task<IActionResult> OnGetSectionsAsync(int district, int county, int route, string assetType)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Default))
                return Forbid();

            var sections = await _projectTreatmentAppService.GetMasterSectionRangesAsync(assetType, district, county, route);
            return new JsonResult(sections.Select(s => new { value = $"{s.FromSection}-{s.ToSection}", fromSection = s.FromSection, toSection = s.ToSection }));
        }

        // Auto-populates BRKey / BMS ID in the Create modal from the master bridge reference data once the Section resolves to a bridge.
        public async Task<IActionResult> OnGetBridgeInfoAsync(int? district, int? county, int? route, string section)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Default))
                return Forbid();

            var segment = (section ?? string.Empty).Split('-')[0].Trim();
            if (!district.HasValue || !county.HasValue || !route.HasValue || !int.TryParse(segment, out _))
                return new JsonResult(Array.Empty<object>());

            var matches = await _projectTreatmentAppService.GetMasterBridgeCandidatesAsync(district.Value, county.Value, route.Value, segment);
            return new JsonResult(matches.Select(m => new { brkey = m.BrKey, bridgeId = m.BridgeId }));
        }

        public async Task<IActionResult> OnGetDefaultSlacksAsync(string assetType)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Default))
                return Forbid();

            var slack = await _treatmentAppService.GetDefaultSlacksAsync(assetType);
            return new JsonResult(new { moveBefore = slack?.MoveBefore ?? 0, moveAfter = slack?.MoveAfter ?? 0 });
        }

        public async Task<IActionResult> OnGetTreatmentAsync(Guid treatmentId)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Default))
                return Forbid();

            try
            {
                var treatment = await _treatmentAppService.GetTreatmentAsync(treatmentId);
                return new JsonResult(treatment);
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { error = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(CreateUserTreatmentDto input)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Create))
                return Forbid();

            try
            {
                var message = await _treatmentAppService.CreateTreatment(input);
                return new JsonResult(new { success = true, message });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // Used by both the Create and Edit modals' "Create New Treatment Type" flow.
        public async Task<IActionResult> OnPostCreateTreatmentTypeAsync(string name)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Create)
                && !await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Edit))
                return Forbid();

            try
            {
                var type = await _treatmentAppService.GetOrCreateTreatmentTypeAsync(name);
                return new JsonResult(new { success = true, id = type.Id, name = type.UserTreatmentName });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostEditAsync(UpdateUserTreatmentDto input)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Edit))
                return Forbid();

            try
            {
                var success = await _treatmentAppService.EditTreatmentAsync(input);
                return new JsonResult(new { success, message = success ? "Treatment was updated successfully." : "Could not update the selected treatment." });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid treatmentId)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Treatments.Delete))
                return Forbid();

            var success = await _treatmentAppService.DeleteTreatmentAsync(treatmentId);
            return new JsonResult(new { success });
        }


        public class TreatmentOptionVm
        {
            public string Name { get; set; }
            public string AssetTypes { get; set; }
        }
    }
}
