using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Models.Project;
using ProjectBuilder.Models.ProjectTreatment;
using ProjectBuilder.Models.ProjectTreatment.Dtos;
using ProjectBuilder.Models.Treatment.Dtos;
using Volo.Abp;

namespace ProjectBuilder.Web.Pages.Map
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IProjectTreatmentAppService _projectTreatmentAppService;
        private readonly IProjectAppService _projectAppService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IProjectTreatmentAppService projectTreatmentAppService,
            IProjectAppService projectAppService,
            ILogger<IndexModel> logger)
        {
            _projectTreatmentAppService = projectTreatmentAppService;
            _projectAppService = projectAppService;
            _logger = logger;
        }

        public int? ScenarioId { get; private set; }

        public IActionResult OnGet(int? scenid)
        {
            if (!scenid.HasValue || scenid.Value <= 0)
            {
                return RedirectToPage("/Scenarios/Index");
            }

            ScenarioId = scenid.Value;
            return Page();
        }

        public async Task<IActionResult> OnPostEditTreatmentAsync([FromBody] UpdateProjectTreatmentDto input)
        {
            try
            {
                var success = await _projectTreatmentAppService.EditProjectTreatment(input);
                return new JsonResult(new { success });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit project treatment.");
                return new JsonResult(new { success = false, message = "Failed to save the treatment. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostCreateTreatmentAsync([FromBody] CreateProjectTreatmentDto input)
        {
            try
            {
                var created = await _projectTreatmentAppService.CreateProjectTreatment(input);
                return new JsonResult(new { success = true, treatmentId = created.Id });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create project treatment.");
                return new JsonResult(new { success = false, message = "Failed to create the treatment. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostDeleteTreatmentAsync([FromBody] DeleteTreatmentRequest input)
        {
            try
            {
                var success = await _projectTreatmentAppService.DeleteProjectTreatment(
                    input.TreatmentId,
                    new TreatmentDto { ScenarioId = input.ScenarioId });
                return new JsonResult(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete project treatment.");
                return new JsonResult(new { success = false, message = "Failed to delete the treatment. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostMoveTreatmentAsync([FromBody] MoveProjectTreatmentDto input)
        {
            try
            {
                var success = await _projectTreatmentAppService.MoveProjectTreatment(input);
                return new JsonResult(new { success });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move project treatment.");
                return new JsonResult(new { success = false, message = "Failed to move the treatment. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetTreatmentDetailAsync(long treatmentId)
        {
            try
            {
                var detail = await _projectTreatmentAppService.GetProjectTreatment(treatmentId);
                return new JsonResult(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load project treatment detail for treatment {TreatmentId}.", treatmentId);
                return new JsonResult(new { message = "Failed to load treatment details. Please try again." }) { StatusCode = 400 };
            }
        }

        // Full treatment rows for a project (used by the bottom editor list — includes
        // year/indirect-cost/risk/priority fields the map GIS payload does not carry).
        public async Task<IActionResult> OnGetProjectTreatmentsAsync(int scenarioId, int projectId)
        {
            var treatments = await _projectTreatmentAppService.LoadProjectTreatments(scenarioId, projectId);
            return new JsonResult(treatments);
        }

        // Distinct treatment names already used across this scenario's own project treatments
        // (not the empty tbl_pb_UserTreatments lookup table) — feeds the "Treatment" free-text
        // field's autocomplete/history.
        public async Task<IActionResult> OnGetTreatmentTypesAsync(int scenarioId, string assetType = null)
        {
            var categories = await _projectTreatmentAppService.GetTreatmentCategoriesAsync(scenarioId, assetType);
            return new JsonResult(categories);
        }

        // "Treatment Category" pick-list — the treatment's CargoData Category master list for
        // this scenario's library, scoped by asset type (matches ProjectTreatments/Index and legacy's
        // Treatment Type field). Distinct from OnGetTreatmentTypesAsync above, which returns
        // treatment *names*, not categories.
        public async Task<IActionResult> OnGetTreatmentTypeCategoriesAsync(int scenarioId, string assetType)
        {
            var categories = await _projectTreatmentAppService.GetTreatmentTypeCategoriesAsync(scenarioId, assetType);
            return new JsonResult(categories);
        }

        public async Task<IActionResult> OnPostUpdateProjectNotesAsync([FromBody] UpdateProjectNotesRequest input)
        {
            try
            {
                var success = await _projectAppService.UpdateNotesAsync(input.ScenarioId, input.ProjectId, input.UserNotes);
                return new JsonResult(new { success });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update project notes.");
                return new JsonResult(new { success = false, message = "Failed to save notes. Please try again." });
            }
        }

        // Add Treatment form's location cascade — sourced from master reference data
        // (tbl_pams_MaintainableAssetsSegmentation for Pavement, tbl_pb_BridgeReferenceDataFromPennshare
        // for Bridge) keyed by the form's own Asset Type field, instead of the scenario's own
        // imported treatments, so every location PennDOT publishes is offered, not just ones
        // already present in this scenario.
        public async Task<IActionResult> OnGetDistrictsAsync(int scenarioId, string assetType = "P")
        {
            var districts = await _projectTreatmentAppService.GetMasterDistrictsAsync(assetType);
            return new JsonResult(districts);
        }

        public async Task<IActionResult> OnGetCountiesAsync(int scenarioId, int district, string assetType = "P")
        {
            var counties = await _projectTreatmentAppService.GetMasterCountiesAsync(assetType, district);
            return new JsonResult(counties.Select(c => new { countyId = c.Id, countyName = c.CountyName }));
        }

        public async Task<IActionResult> OnGetRoutesAsync(int scenarioId, int district, int county, string assetType = "P")
        {
            var routes = await _projectTreatmentAppService.GetMasterRoutesAsync(assetType, district, county);
            return new JsonResult(routes);
        }

        public async Task<IActionResult> OnGetSectionsAsync(int scenarioId, int district, int county, int route)
        {
            var sections = await _projectTreatmentAppService.GetScenarioSectionsAsync(scenarioId, district, county, route);
            return new JsonResult(sections);
        }

        // Same cascade level as OnGetSectionsAsync, but with each option's From/To range so the
        // Section dropdown can display "10 - 10" like the project treatments table above it.
        public async Task<IActionResult> OnGetSectionRangesAsync(int scenarioId, int district, int county, int route, string assetType = "P")
        {
            var sections = await _projectTreatmentAppService.GetMasterSectionRangesAsync(assetType, district, county, route);
            return new JsonResult(sections.Select(s => new { section = s.Section, fromSection = s.FromSection, toSection = s.ToSection }));
        }

        // Every bridge matching the selected District/County/Route/Section — Segment isn't a
        // unique key in tbl_pb_BridgeReferenceDataFromPennshare, so this can return more than
        // one candidate (see BridgeCandidateModel).
        public async Task<IActionResult> OnGetBridgeCandidatesAsync(int scenarioId, int district, int county, int route, string section)
        {
            var candidates = await _projectTreatmentAppService.GetMasterBridgeCandidatesAsync(district, county, route, section);
            return new JsonResult(candidates.Select(c => new { brKey = c.BrKey, bridgeId = c.BridgeId }));
        }

        public class DeleteTreatmentRequest
        {
            public long TreatmentId { get; set; }
            public int ScenarioId { get; set; }
        }

        public class UpdateProjectNotesRequest
        {
            public int ScenarioId { get; set; }
            public int ProjectId { get; set; }
            public string UserNotes { get; set; }
        }
    }
}
