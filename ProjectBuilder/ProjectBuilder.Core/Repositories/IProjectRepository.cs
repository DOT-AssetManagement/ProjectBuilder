using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface IProjectRepository : IRepository<ProjectModel>,IFilterData
    {
        Task<int> GetCountByLibraryIdAsync(Guid libraryId);
        Task<List<ProjectModel>> FilterProjects(ProjectSearchModel model);
        Task<List<ProjectModel>> FilterProjects(int? ScenId);
        Task<bool> DeleteProject(int projectId,int scenarioid);
        Task<bool> UpdateUserIdAndNotes(int scenarioId, int projectId, string userId, string userNotes);
    }
}
