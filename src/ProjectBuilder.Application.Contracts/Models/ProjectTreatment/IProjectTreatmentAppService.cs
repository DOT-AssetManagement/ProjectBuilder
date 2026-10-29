using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.Project.Dtos;
using ProjectBuilder.Models.ProjectTreatment.Dtos;
using ProjectBuilder.Models.Treatment.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.ProjectTreatment
{
    public interface IProjectTreatmentAppService : ICrudAppService<ProjectTreatmentDto, Guid, GetProjectTreatmentInput, CreateProjectTreatmentDto, UpdateProjectTreatmentDto>
    {
        Task<IEnumerable<ProjectTreatmentDto>> LoadProjectTreatments(int scenarioId, int projectId, string filterJson = null);

        // Scenario-wide treatment list (all projects) — backs /ProjectTreatments/List.
        Task<IEnumerable<ProjectTreatmentDto>> LoadScenarioTreatments(int scenarioId, string filterJson = null);

        // Server-paginated version of the above, backing the /ProjectTreatments/List grid's remote
        // Tabulator pagination. assetType accepts 'B', 'P', or 'O' ("Others" — neither B nor P).
        Task<ScenarioTreatmentsPageDto> GetScenarioTreatmentsPageAsync(
            int scenarioId, string assetType, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, string sortDir, int page, int pageSize);
        Task<bool> DeleteProjectTreatment(long? treatmentid, TreatmentDto treatmentViewModel);

        // Reassigns a treatment to another project in the same scenario, leaving the source project in place.
        Task<bool> MoveProjectTreatment(MoveProjectTreatmentDto input);
        Task<bool> EditProjectTreatment(UpdateProjectTreatmentDto treatmentViewModel);
        Task<bool> CheckRunAvailAbilty(int? scenarioId);
        Task<ProjectTreatmentDto> GetProjectTreatment(long? projectTreatmentId);
        Task<ProjectTreatmentDto> CreateProjectTreatment(CreateProjectTreatmentDto input);

        // Distinct treatment names already used across this scenario's own project treatments
        // (not the empty tbl_pb_UserTreatments lookup table) — backs the "Treatment Category"
        // pick-list on the Map editor and the ProjectTreatments page.
        Task<List<string>> GetTreatmentCategoriesAsync(int scenarioId, string assetType = null);

        // "Treatment Type" field options on the ProjectTreatments Create/Edit modals — the
        // treatment's CargoData "Category" attribute values for the scenario's library + asset
        // type (e.g. "Preservation", "Applied"), matching legacy's Treatment Type dropdown.
        Task<List<string>> GetTreatmentTypeCategoriesAsync(int scenarioId, string assetType);

        // Scenario-scoped location cascade for the Create modal (District → County → Route → Section),
        // matching legacy's /ProjectTreatments create form which sources these from the scenario's own
        // project treatments so every offered location has a valid section to pick.
        Task<List<byte?>> GetScenarioDistrictsAsync(int scenarioId);
        Task<List<CountyDto>> GetScenarioCountiesAsync(int scenarioId, int district);
        Task<List<int?>> GetScenarioRoutesAsync(int scenarioId, int district, int county);
        Task<List<string>> GetScenarioSectionsAsync(int scenarioId, int district, int county, int route);

        // Preferred-year options for the /ProjectTreatments/List filter bar.
        Task<List<int?>> GetScenarioYearsAsync(int scenarioId);

        // Same cascade level as GetScenarioSectionsAsync, but carries each option's From/To
        // range — backs the Map treatment editor's Section dropdown, which (unlike the
        // ProjectTreatments create form) needs to display "10 - 10" like its own table does.
        Task<List<SectionRangeDto>> GetScenarioSectionRangesAsync(int scenarioId, int district, int county, int route);

        // Master reference-data cascade for the Map treatment editor's Add-treatment form —
        // District/County/Route/Section sourced from tbl_pams_MaintainableAssetsSegmentation
        // (Pavement) or tbl_pb_BridgeReferenceDataFromPennshare (Bridge) instead of the
        // scenario's own imported treatments, so options aren't limited to what's already there.
        Task<List<int?>> GetMasterDistrictsAsync(string assetType);
        Task<List<CountyDto>> GetMasterCountiesAsync(string assetType, int district);
        Task<List<int?>> GetMasterRoutesAsync(string assetType, int district, int county);
        Task<List<SectionRangeDto>> GetMasterSectionRangesAsync(string assetType, int district, int county, int route);

        // Every bridge whose District/County/Route/Segment matches the selected Section — the
        // Add-treatment form's BRKEY/BMS ID auto-fill (single match) or extra Bridge picker
        // (multiple matches) is driven off this. Bridge asset type only.
        Task<List<BridgeCandidateDto>> GetMasterBridgeCandidatesAsync(int district, int county, int route, string section);
    }
}
