using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.Project.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Project
{
    public interface IProjectAppService : IApplicationService
    {
        Task<bool> DeleteProjectAsync(int projectId, int scenId);
        Task<ExportResultDto> GetExportAsync(ExportProjectDto data);
        Task<bool> UpdateNotesAsync(int scenarioId, int projectId, string userNotes);

        // ── Server-side pagination for the Projects grid ──
        Task<ProjectsPageDto> GetProjectsPageAsync(
            int scenarioId, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, string sortDir, int page, int pageSize, bool unfunded = false, string committed = null);
        Task<int?> GetProjectIdBySequenceNoAsync(int scenarioId, int sequenceNo, bool unfunded = false);
        Task<List<int>> GetProjectIdsAsync(int scenarioId, byte? district, byte? countyId, int? route, string section, int? year, bool unfunded = false, string committed = null);

        // ── Filter dropdown options ──
        Task<List<byte?>> GetDistrictsAsync(int scenarioId);
        Task<List<CountyDto>> GetCountiesAsync(int scenarioId, int? district);
        Task<List<int?>> GetRoutesAsync(int scenarioId, int? district, int? county);
        Task<List<string>> GetSectionsAsync(int scenarioId, int? district, int? county, int? route);
        Task<List<int?>> GetYearsAsync(int scenarioId);
    }
}
