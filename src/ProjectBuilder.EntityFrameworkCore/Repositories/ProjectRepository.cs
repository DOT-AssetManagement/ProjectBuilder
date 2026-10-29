using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder.Repositories
{
    public class ProjectsRepository : EfCoreRepository<ProjectBuilderDbContext, Project, int>, IProjectRepository
    {
        private readonly ProjectBuilderDbContext _context;
        private readonly string _connectionString;
        public ProjectsRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext, IConfiguration configuration) :
                             base(projectBuilderDbContext)
        {
            _context = dbContext;
            _connectionString = configuration.GetConnectionString("Default");
        }

        public async Task<bool> HasProjectsAsync(int scenarioId)
        {
            // IgnoreQueryFilters: the entity's default filter is funded-only (Selected == "Y"),
            // but the Maps nav should stay enabled for a scenario with only unfunded projects too.
            return await _context.Projects.AsNoTracking().IgnoreQueryFilters()
                .AnyAsync(p => p.ScenarioId == scenarioId);
        }

        public async Task<int> GetCountByLibraryIdAsync(Guid libraryId)
        {
            var scenarioIds = await _context.Scenarios
                .Where(x => x.LibraryId == libraryId)
                .Select(x => x.Id).ToListAsync();
            var projectCount = await _context.Projects
                .Where(x => scenarioIds.Contains(x.ScenarioId)).CountAsync();
            return projectCount;
        }

        public async Task<bool> DeleteProject(int projectId, int scenarioid)
        {
            var project = await _context.Projects.IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ScenarioId == scenarioid && p.Id == projectId);
            if (project is null)
                return false;
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateNotes(int scenarioId, int projectId, string userNotes)
        {
            var project = await _context.Projects.IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.ScenarioId == scenarioId && p.Id == projectId);
            if (project is null)
                return false;
            project.UserNotes = userNotes;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Project>> FilterProjects(int? ScenId)
        {
            var ctx = await GetDbContextAsync();
            return await ctx.Projects.Where(a => a.ScenarioId == ScenId).OrderBy(a => a.Id).AsNoTracking().ToListAsync();
        }

        // Maps each real ProjectId to its 1-based position within the scenario (ordered by Id), so the grid and map agree on the friendly number.
        public async Task<Dictionary<int, int>> GetSequenceNumbersAsync(int scenarioId)
        {
            var ids = await _context.Projects
                .Where(p => p.ScenarioId == scenarioId)
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .ToListAsync();

            var sequenceNumbers = new Dictionary<int, int>();
            for (var i = 0; i < ids.Count; i++)
                sequenceNumbers[ids[i]] = i + 1;
            return sequenceNumbers;
        }

        // Same idea as GetSequenceNumbersAsync, but numbers the unfunded projects the funded-only
        // global query filter hides — a separate 1-based run, so they read as UF-1, UF-2, …
        public async Task<Dictionary<int, int>> GetUnfundedSequenceNumbersAsync(int scenarioId)
        {
            var ids = await _context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.ScenarioId == scenarioId && p.Selected == "N")
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .ToListAsync();

            var sequenceNumbers = new Dictionary<int, int>();
            for (var i = 0; i < ids.Count; i++)
                sequenceNumbers[ids[i]] = i + 1;
            return sequenceNumbers;
        }

        // The Projects grid is the only place unfunded rows are shown, so it opts out of the
        // funded-only global query filter and names the side it wants.
        private IQueryable<Project> ProjectsFor(bool unfunded)
        {
            var selected = unfunded ? "N" : "Y";
            return _context.Projects.AsNoTracking().IgnoreQueryFilters().Where(p => p.Selected == selected);
        }
        // Single source of truth for the district/county/route/section/year predicate — also used by FilterProjects(ProjectSearchModel) below.
        private static IQueryable<Project> ApplySearchFilter(IQueryable<Project> query, ProjectSearchModel model)
        {
            return query.Where(x => (model.ScenarioId == x.ScenarioId)
                && (!model.County.HasValue || x.CountyId.Equals(model.County))
                && (!model.District.HasValue || x.District.Equals(model.District))
                && (!model.Route.HasValue || x.Route.Equals(model.Route))
                && (string.IsNullOrEmpty(model.Section) || x.Section == model.Section)
                && (!model.Year.HasValue || x.SelectedFirstYear.Equals(model.Year)));
        }

        // committed is "Y"/"N" against CommitmentStatus (the view's HasCommitted); anything else leaves it unconstrained.
        private static IQueryable<Project> ApplyGridFilters(
            IQueryable<Project> query, int scenarioId, byte? district, byte? countyId, int? route, string section, int? year, string committed)
        {
            query = ApplySearchFilter(query, new ProjectSearchModel
            {
                ScenarioId = scenarioId, District = district, County = countyId, Route = route, Section = section, Year = year
            });
            if (committed == "Y" || committed == "N")
                query = query.Where(p => p.CommitmentStatus == committed);
            return query;
        }

        // Whitelisted switch, not string-built SQL — sequenceNo falls through to Id since it's monotonic with it.
        // Always breaks ties on Id so Skip/Take paging stays stable across requests on low-cardinality columns.
        private static IQueryable<Project> ApplySort(IQueryable<Project> query, string sortField, bool desc)
        {
            IOrderedQueryable<Project> ordered = (sortField ?? "").ToLowerInvariant() switch
            {
                "assettype" => desc ? query.OrderByDescending(p => p.AssetType) : query.OrderBy(p => p.AssetType),
                "district" => desc ? query.OrderByDescending(p => p.District) : query.OrderBy(p => p.District),
                "county" => desc ? query.OrderByDescending(p => p.County) : query.OrderBy(p => p.County),
                "route" => desc ? query.OrderByDescending(p => p.Route) : query.OrderBy(p => p.Route),
                "section" => desc ? query.OrderByDescending(p => p.Section) : query.OrderBy(p => p.Section),
                "direction" => desc ? query.OrderByDescending(p => p.Direction) : query.OrderBy(p => p.Direction),
                "firstyear" => desc ? query.OrderByDescending(p => p.SelectedFirstYear) : query.OrderBy(p => p.SelectedFirstYear),
                "totalcost" => desc ? query.OrderByDescending(p => p.TotalCost) : query.OrderBy(p => p.TotalCost),
                "benefit" => desc ? query.OrderByDescending(p => p.Benefit) : query.OrderBy(p => p.Benefit),
                "treatments" => desc ? query.OrderByDescending(p => p.NumberOfTreatments) : query.OrderBy(p => p.NumberOfTreatments),
                "hascommitted" => desc ? query.OrderByDescending(p => p.CommitmentStatus) : query.OrderBy(p => p.CommitmentStatus),
                "selected" => desc ? query.OrderByDescending(p => p.Selected) : query.OrderBy(p => p.Selected),
                "preferredyear" => desc ? query.OrderByDescending(p => p.PreferredStartingYear) : query.OrderBy(p => p.PreferredStartingYear),
                "usercreated" => desc ? query.OrderByDescending(p => p.UserCreated) : query.OrderBy(p => p.UserCreated),
                _ => desc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id) // "sequenceno" and unrecognized fields
            };
            return ordered.ThenBy(p => p.Id);
        }

        public async Task<(List<(Project Project, int SequenceNo)> Items, int TotalCount)> GetPagedProjectsAsync(
            int scenarioId, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, bool sortDesc, int page, int pageSize, bool unfunded = false, string committed = null)
        {
            pageSize = Math.Clamp(pageSize, 1, 200);

            var filtered = ApplyGridFilters(ProjectsFor(unfunded), scenarioId, district, countyId, route, section, year, committed);
            var totalCount = await filtered.CountAsync();

            var maxPage = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            page = Math.Clamp(page, 1, maxPage);

            var sorted = ApplySort(filtered, sortField, sortDesc);
            var pageItems = await sorted.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // A correlated COUNT subquery per row here produced wrong, page-relative numbers (EF Core query-translation issue), so this reuses the known-correct full-scenario lookup instead.
            var sequenceNumbers = unfunded
                ? await GetUnfundedSequenceNumbersAsync(scenarioId)
                : await GetSequenceNumbersAsync(scenarioId);
            var items = pageItems.Select(p => (p, sequenceNumbers.GetValueOrDefault(p.Id))).ToList();

            return (items, totalCount);
        }

        public async Task<int?> GetProjectIdBySequenceNoAsync(int scenarioId, int sequenceNo, bool unfunded = false)
        {
            if (sequenceNo < 1) return null;
            return await ProjectsFor(unfunded)
                .Where(p => p.ScenarioId == scenarioId)
                .OrderBy(p => p.Id)
                .Skip(sequenceNo - 1)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetProjectIdsAsync(
            int scenarioId, byte? district, byte? countyId, int? route, string section, int? year, bool unfunded = false, string committed = null)
        {
            var filtered = ApplyGridFilters(ProjectsFor(unfunded), scenarioId, district, countyId, route, section, year, committed);
            return await filtered.Select(p => p.Id).ToListAsync();
        }

        public async Task<List<Project>> FilterProjects(ProjectSearchModel model)
        {
            try
            {
                var result = await ApplySearchFilter(_context.Projects, model)
                                                     .OrderBy(e => e.Id)
                                                     .AsNoTracking().ToListAsync();
                //var projects = await result.ProjectTo<ProjectModel>(Mapper.ConfigurationProvider).ToListAsync();
                return result;
            }
            catch (Exception)
            {
                return Enumerable.Empty<Project>().ToList();
            }
        }
        public Task<List<byte?>> GetDistricts(int? scenarioId)
        {
            return _context.Projects.Where(p => p.ScenarioId == scenarioId)
                                                    .Select(p => p.District)
                                                    .Distinct()
                                                    .OrderBy(d => d)
                                                    .ToListAsync();
        }
        // district/county/route null means "not yet narrowed", not "match a literal NULL column".
        public Task<List<CountyEntity>> GetCounties(int? scenarioId, int? district)
        {
            return _context.Projects.Where(p => p.ScenarioId == scenarioId && (!district.HasValue || p.District == district))
                                          .Join(_context.Counties, p => p.CountyId, c => c.Id, (p, c) => c)
                                          .Distinct()
                                          .OrderBy(c => c.Id)
                                          //.ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                                          .ToListAsync();
        }
        public Task<List<int?>> GetRoutes(int? scenarioId, int? district, int? county)
        {
            return _context.Projects.Where(p => p.ScenarioId == scenarioId
                                                   && (!district.HasValue || p.District == district)
                                                   && (!county.HasValue || p.CountyId == county))
                                                   .Select(p => p.Route)
                                                   .Distinct()
                                                   .OrderBy(d => d)
                                                   .ToListAsync();
        }
        public Task<List<string>> GetSections(int? scenarioId, int? district, int? county, int? route)
        {
            return _context.Projects.Where(p => p.ScenarioId == scenarioId
                                                   && (!district.HasValue || p.District == district)
                                                   && (!county.HasValue || p.CountyId == county)
                                                   && (!route.HasValue || p.Route == route))
                                                   .Select(p => p.Section)
                                                   .Distinct()
                                                   .ToListAsync();
        }
        public Task<List<int?>> GetYears(int? scenarioId)
        {
            return _context.Projects.Where(p => p.ScenarioId == scenarioId && p.SelectedFirstYear.HasValue)
                                .Select(p => p.SelectedFirstYear)
                                .Distinct()
                                .OrderBy(d => d)
                                .ToListAsync();
        }
        public async Task<DataTable> GetProjectData(string storedProcedure, int scenid)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(storedProcedure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 24000;
                    command.Parameters.Add("ScenId", SqlDbType.Int).Value = scenid;

                    var dataAdapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    return dataTable;
                }
            }
        }

        public async Task<DataTable> GetTreatmentData(string storedProcedureTreatments, int scenid)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(storedProcedureTreatments, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 24000;
                    command.Parameters.Add("ScenId", SqlDbType.Int).Value = scenid;

                    var dataAdapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    return dataTable;
                }
            }
        }
    }
}
