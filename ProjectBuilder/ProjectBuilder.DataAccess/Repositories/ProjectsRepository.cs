using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class ProjectsRepository : ProjectBuilderRepository<Project, ProjectModel, int>, IProjectRepository
    {
        public ProjectsRepository(ProjectBuilderDbContext projectBuilderDbContext, IMapper mapper, ILogger<ProjectBuilderRepository<Project, ProjectModel, int>> logger) :
                             base(projectBuilderDbContext, mapper, logger)
        {
           
        }

        public async Task<int> GetCountByLibraryIdAsync(Guid libraryId)
        {
            var scenarioIds = await ProjectBuilderDbContext.Scenarios
                .Where(x => x.LibraryId == libraryId)
                .Select(x => x.EntityId).ToListAsync();
            var projectCount = await ProjectBuilderDbContext.Projects
                .Where(x => scenarioIds.Contains(x.ScenarioId)).CountAsync();
            return projectCount;
        }

        public async Task<bool> DeleteProject(int projectId,int scenarioid)
        {
            var project = await ProjectBuilderDbContext.Projects.FindAsync(scenarioid,projectId);
            if(project is null)
                return false;
            ProjectBuilderDbContext.Projects.Remove(project);
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserIdAndNotes(int scenarioId, int projectId, string userId, string userNotes)
        {
            var project = await ProjectBuilderDbContext.Projects.FindAsync(scenarioId, projectId);
            if (project is null)
                return false;

            project.UserId = userId;
            project.UserNotes = userNotes;
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }


        public async Task<List<ProjectModel>> FilterProjects(int? ScenId)
        {
            var result = ProjectBuilderDbContext.Projects.Where(a => a.ScenarioId == ScenId).AsNoTracking();
            var projects = await result.ProjectTo<ProjectModel>(Mapper.ConfigurationProvider).ToListAsync();
            return projects;
        }
        public async Task<List<ProjectModel>> FilterProjects(ProjectSearchModel model)
        {
            try
            {
              var result=  ProjectBuilderDbContext.Projects.Where(x => (model.ScenarioId == x.ScenarioId)
                                                         && (!model.County.HasValue || x.CountyId.Equals(model.County))
                                                         && (!model.District.HasValue || x.District.Equals(model.District))
                                                         && (!model.Route.HasValue || x.Route.Equals(model.Route))
                                                         && (string.IsNullOrEmpty(model.Section) || x.Section.Contains(model.Section))
                                                         && (!model.Year.HasValue || x.SelectedFirstYear.Equals(model.Year)))
                                                   .OrderBy(e => e.EntityId)
                                                   .AsNoTracking();
                var projects = await  result.ProjectTo<ProjectModel>(Mapper.ConfigurationProvider).ToListAsync();
                return projects;
            }
            catch (Exception)
            {
               return Enumerable.Empty<ProjectModel>().ToList();
            }        
        }
        public  Task<List<byte?>> GetDistricts(int? scenarioId)
        {
            return  ProjectBuilderDbContext.Projects.Where(p => p.ScenarioId == scenarioId)
                                                    .Select(p=> p.District)
                                                    .Distinct()
                                                    .OrderBy(d => d)
                                                    .ToListAsync();
        }
        public  Task<List<CountyModel>> GetCounties(int? scenarioId, int? district)
        {
                return ProjectBuilderDbContext.Projects
                                              .Where(p => p.ScenarioId == scenarioId && p.District == district)
                                              .Join(ProjectBuilderDbContext.Counties, p => p.CountyId, c => c.EntityId, (p, c) => c)
                                              .Distinct()
                                              .OrderBy(c => c.EntityId)
                                              .ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                                              .ToListAsync();   
        }
        public Task<List<int?>> GetRoutes(int? scenarioId, int? district, int? county)
        {
            return ProjectBuilderDbContext.Projects.Where(p => p.ScenarioId == scenarioId && p.District == district && p.CountyId == county)
                                                   .Select(p => p.Route)
                                                   .Distinct()
                                                   .OrderBy(d => d)
                                                   .ToListAsync();
        }
        public Task<List<string>> GetSections(int? scenarioId, int? district, int? county, int? route)
        {
            return ProjectBuilderDbContext.Projects.Where(p => p.ScenarioId == scenarioId && p.District == district && p.CountyId == county && p.Route == route)
                                                   .Select(p => p.Section)
                                                   .Distinct()
                                                   .ToListAsync();
        }
        public  Task<List<int?>> GetYears(int? scenarioId)
        {
           return  ProjectBuilderDbContext.Projects.Where(p => p.ScenarioId == scenarioId && p.SelectedFirstYear.HasValue)
                               .Select(p => p.SelectedFirstYear)
                               .Distinct()
                               .OrderBy(d => d)
                               .ToListAsync();
        }
    }
}
