using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IProjectTreatmentRepository : IRepository<ProjectTreatment>, IFilterData
    {
        Task<bool> DeleteProjectTreatments(long modelId);
        Task<bool> EditProjectTreatments(ProjectTreatment customTreatments, string category);
        Task<ProjectTreatment> CreateProjectTreatment(ProjectTreatment newTreatment, string category);
        Task<bool> CheckIsUserCreated(int? scenId);
        Task<bool?> CheckStale(int? scenId);
        Task UpdateScenarioNotes(int? scenid);
        Task<List<ProjectTreatment>> FilterProjectTreatments(TreatmentSearchModel model);
        Task<bool> DeleteAsync(long? treatmentid);

        // Null when the project isn't in the scenario at all, so callers can tell "missing" from "unfunded".
        Task<bool?> IsProjectFundedAsync(int scenarioId, int projectId);
        Task<bool> MoveTreatmentToProjectAsync(int scenarioId, long treatmentId, int targetProjectId);
        Task<ProjectTreatment> FindAsync(long? projectTreatmentId);
        Task SetImportTimeGeneratedId(long treatmentId, Guid importTimeGeneratedId);
        Task<List<string>> GetTreatmentTypes(int? scenarioId, string assetType);
        Task<List<string>> GetTreatmentCategoryOptionsAsync(int? scenarioId, string assetType);
        Task<List<SectionOptionModel>> GetSectionRanges(int? scenarioId, int? district, int? county, int? route);

        // Master reference-data cascade (tbl_pams_MaintainableAssetsSegmentation for Pavement,
        // tbl_pb_BridgeReferenceDataFromPennshare for Bridge) — independent of any scenario's own
        // imported treatments, keyed only by asset type.
        Task<List<int?>> GetMasterDistricts(string assetType);
        Task<List<CountyEntity>> GetMasterCounties(string assetType, int district);
        Task<List<int?>> GetMasterRoutes(string assetType, int district, int county);
        Task<List<SectionOptionModel>> GetMasterSectionRanges(string assetType, int district, int county, int route);

        // Every bridge whose District/County/Route/Segment matches the selected Section — see
        // BridgeCandidateModel for why this can be more than one. Bridge asset type only.
        Task<List<BridgeCandidateModel>> GetMasterBridgeCandidates(int district, int county, int route, string section);

        // Server-side paged/filtered/sorted scenario-wide treatment grid — backs /ProjectTreatments/List,
        // which used to load every treatment in the scenario up front. assetType accepts 'B', 'P',
        // or 'O' (meaning "neither B nor P", i.e. the grid's "Others" bucket).
        Task<(List<ProjectTreatment> Items, int TotalCount)> GetPagedScenarioTreatmentsAsync(
            int scenarioId, string assetType, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, bool sortDesc, int page, int pageSize);
    }
    public interface IFilterData
    {
        Task<List<byte?>> GetDistricts(int? scenarioId);
        Task<List<CountyEntity>> GetCounties(int? scenarioId, int? district);
        Task<List<int?>> GetRoutes(int? scenarioId, int? district, int? county);
        Task<List<string>> GetSections(int? scenarioId, int? district, int? county, int? route);
        Task<List<int?>> GetYears(int? scenarioId);
    }
}
