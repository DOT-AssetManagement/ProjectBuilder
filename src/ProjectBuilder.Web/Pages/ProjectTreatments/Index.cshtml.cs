using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Models.ProjectTreatment;
using ProjectBuilder.Models.ProjectTreatment.Dtos;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Treatment;
using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Repositories;
using ProjectBuilder.Web.Pages.Projects;
using ProjectBuilder.Web.Services;
using Volo.Abp;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.ProjectTreatments
{
    public class IndexModel : AnalysisPageModelBase
    {
        private readonly IProjectTreatmentAppService _projectTreatmentAppService;
        private readonly IScenarioAppService _scenarioAppService;
        private readonly IProjectRepository _projectRepository;

        public int ProjectId { get; set; }
        // Display-only friendly project number (1-based within the scenario), matching the Projects grid.
        public int? ProjectSequenceNo { get; set; }
        // True when the sequence number above came from the unfunded (UF-*) numbering rather than the funded one.
        public bool ProjectIsUnfunded { get; set; }
        public List<ProjectTreatmentDto> Treatments { get; set; } = new();
        // Districts offered by the Create modal — the scenario's own districts (matches legacy,
        // and keeps the location cascade consistent so every district has counties/routes/sections).
        public List<byte?> Districts { get; set; } = new();

        public override string ActiveMenu => "Projects";

        public IndexModel(
            IProjectTreatmentAppService projectTreatmentAppService,
            IScenarioAppService scenarioAppService,
            ICurrentUser currentUser,
            ITourGuideSeenService tourGuideSeenService,
            IProjectRepository projectRepository)
            : base(currentUser, tourGuideSeenService, projectRepository)
        {
            _projectTreatmentAppService = projectTreatmentAppService;
            _scenarioAppService = scenarioAppService;
            _projectRepository = projectRepository;
        }

        public async Task OnGetAsync(int scenid, int projectid)
        {
            ProjectId = projectid;
            await LoadScenarioAsync(_scenarioAppService, scenid);
            // GetSequenceNumbersAsync only numbers funded (Selected='Y') projects, so an unfunded
            // project's id is never in it — fall back to the unfunded numbering instead of leaving
            // the header showing the raw, non-sequential project id.
            var sequenceNumbers = await _projectRepository.GetSequenceNumbersAsync(scenid);
            if (sequenceNumbers.TryGetValue(projectid, out var seq))
            {
                ProjectSequenceNo = seq;
            }
            else
            {
                var unfundedSequenceNumbers = await _projectRepository.GetUnfundedSequenceNumbersAsync(scenid);
                if (unfundedSequenceNumbers.TryGetValue(projectid, out var ufSeq))
                {
                    ProjectSequenceNo = ufSeq;
                    ProjectIsUnfunded = true;
                }
                else
                {
                    ProjectSequenceNo = null;
                }
            }
            Treatments = (await _projectTreatmentAppService.LoadProjectTreatments(scenid, projectid)).ToList();
            Districts = await _projectTreatmentAppService.GetScenarioDistrictsAsync(scenid);
        }

        public async Task<IActionResult> OnPostEditAsync(UpdateProjectTreatmentDto input)
        {
            try
            {
                var success = await _projectTreatmentAppService.EditProjectTreatment(input);
                return new JsonResult(new { success, message = success ? "Treatment was updated successfully." : "Could not update the selected treatment." });
            }
            catch (UserFriendlyException ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(long treatmentId, int scenarioId)
        {
            var success = await _projectTreatmentAppService.DeleteProjectTreatment(
                treatmentId,
                new TreatmentDto { ScenarioId = scenarioId });
            return new JsonResult(new { success });
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
        }

        // Move-target picker — a move always crosses the funding line (matches the map's GIS
        // treatment editor), so this offers only the projects on the opposite side from the
        // treatment's current (this page's) project.
        public async Task<IActionResult> OnGetMoveTargetsAsync(int scenid, int projectId)
        {
            var funded = await _projectRepository.GetSequenceNumbersAsync(scenid);
            var unfunded = await _projectRepository.GetUnfundedSequenceNumbersAsync(scenid);

            var isCurrentFunded = funded.ContainsKey(projectId);
            var targets = isCurrentFunded
                ? unfunded.Select(kv => new { id = kv.Key, label = "UF-" + kv.Value })
                : funded.Select(kv => new { id = kv.Key, label = "PB-" + kv.Value });

            return new JsonResult(targets.OrderBy(t => t.label));
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
        }

        // Distinct treatment names already used across this scenario, scoped by asset type —
        // feeds the Create modal's "Treatment Category" pick-list, which fills in "Treatment".
        public async Task<IActionResult> OnGetTreatmentTypesAsync(int scenarioId, string assetType = null)
        {
            var categories = await _projectTreatmentAppService.GetTreatmentCategoriesAsync(scenarioId, assetType);
            return new JsonResult(categories);
        }

        // "Treatment Type" pick-list for Create/Edit — the treatment's CargoData Category values
        // for this scenario's library, scoped by asset type (matches legacy's Treatment Type field).
        public async Task<IActionResult> OnGetTreatmentTypeCategoriesAsync(int scenarioId, string assetType)
        {
            var categories = await _projectTreatmentAppService.GetTreatmentTypeCategoriesAsync(scenarioId, assetType);
            return new JsonResult(categories);
        }

        // Create-modal location cascade — scenario-scoped (the scenario's own project treatments),
        // matching legacy so each level only offers values that lead to a pickable Section.
        public async Task<IActionResult> OnGetCountiesAsync(int scenarioId, int district)
        {
            var counties = await _projectTreatmentAppService.GetScenarioCountiesAsync(scenarioId, district);
            return new JsonResult(counties.Select(c => new { countyId = c.Id, countyName = c.CountyName }));
        }

        public async Task<IActionResult> OnGetRoutesAsync(int scenarioId, int district, int county)
        {
            var routes = await _projectTreatmentAppService.GetScenarioRoutesAsync(scenarioId, district, county);
            return new JsonResult(routes);
        }

        public async Task<IActionResult> OnGetSectionsAsync(int scenarioId, int district, int county, int route)
        {
            var sections = await _projectTreatmentAppService.GetScenarioSectionsAsync(scenarioId, district, county, route);
            return new JsonResult(sections);
        }

        // Master reference-data cascade for the Create modal's Bridge/Pavement locations — same
        // source as the Map treatment editor's Add-treatment form (tbl_pams_MaintainableAssetsSegmentation
        // for Pavement, tbl_pb_BridgeReferenceDataFromPennshare for Bridge), so every location PennDOT
        // publishes is offered, not just ones already imported into this scenario. "Others" treatments
        // have no master table and keep using the scenario-scoped handlers above.
        public async Task<IActionResult> OnGetMasterDistrictsAsync(string assetType)
        {
            var districts = await _projectTreatmentAppService.GetMasterDistrictsAsync(assetType);
            return new JsonResult(districts);
        }

        public async Task<IActionResult> OnGetMasterCountiesAsync(int district, string assetType)
        {
            var counties = await _projectTreatmentAppService.GetMasterCountiesAsync(assetType, district);
            return new JsonResult(counties.Select(c => new { countyId = c.Id, countyName = c.CountyName }));
        }

        public async Task<IActionResult> OnGetMasterRoutesAsync(int district, int county, string assetType)
        {
            var routes = await _projectTreatmentAppService.GetMasterRoutesAsync(assetType, district, county);
            return new JsonResult(routes);
        }

        public async Task<IActionResult> OnGetMasterSectionRangesAsync(int district, int county, int route, string assetType)
        {
            var sections = await _projectTreatmentAppService.GetMasterSectionRangesAsync(assetType, district, county, route);
            return new JsonResult(sections.Select(s => new { section = s.Section, fromSection = s.FromSection, toSection = s.ToSection }));
        }

        // Every bridge whose District/County/Route/Segment matches the selected Section — BRKEY/BMS ID
        // auto-fill (single match) or a datalist suggestion (multiple matches). Bridge asset type only.
        public async Task<IActionResult> OnGetBridgeCandidatesAsync(int district, int county, int route, string section)
        {
            var candidates = await _projectTreatmentAppService.GetMasterBridgeCandidatesAsync(district, county, route, section);
            return new JsonResult(candidates.Select(c => new { brKey = c.BrKey, bridgeId = c.BridgeId }));
        }
    }
}
