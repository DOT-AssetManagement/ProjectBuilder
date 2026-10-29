using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IProjectRepository : IRepository<Project>,IFilterData
    {
        Task<int> GetCountByLibraryIdAsync(Guid libraryId);
        Task<List<Project>> FilterProjects(ProjectSearchModel model);
        Task<List<Project>> FilterProjects(int? ScenId);
        Task<Dictionary<int, int>> GetSequenceNumbersAsync(int scenarioId);
        // Mirrors GetSequenceNumbersAsync but for unfunded (IsSelected=0) projects, which vw_pb_ui_ScenarioProjects
        // (and so _context.Projects) excludes entirely — backs the map's "UF-*" numbering.
        Task<Dictionary<int, int>> GetUnfundedSequenceNumbersAsync(int scenarioId);
        Task<bool> DeleteProject(int projectId, int scenarioid);
        Task<DataTable> GetProjectData(string storedProcedure, int scenid);
        Task<DataTable> GetTreatmentData(string storedProcedureTreatments, int scenid);
        Task<bool> UpdateNotes(int scenarioId, int projectId, string userNotes);
        Task<bool> HasProjectsAsync(int scenarioId);

        // ── Server-side pagination for the Projects grid — SequenceNo is scoped per-row, not a full scenario scan ──
        // `unfunded` swaps the grid over to the IsSelected=0 side, which every other caller keeps hidden.
        Task<(List<(Project Project, int SequenceNo)> Items, int TotalCount)> GetPagedProjectsAsync(
            int scenarioId, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, bool sortDesc, int page, int pageSize, bool unfunded = false, string committed = null);
        Task<int?> GetProjectIdBySequenceNoAsync(int scenarioId, int sequenceNo, bool unfunded = false);
        Task<List<int>> GetProjectIdsAsync(int scenarioId, byte? district, byte? countyId, int? route, string section, int? year, bool unfunded = false, string committed = null);
        // District/County/Route/Section/Year lookups already come from the inherited IFilterData.
    }
}
