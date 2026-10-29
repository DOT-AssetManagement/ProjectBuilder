using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ProjectBuilder;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using ProjectBuilder.Models;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.Project;
using ProjectBuilder.Models.Project.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using Scriban;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Http;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(ScenarioPermissions.Analysis.Projects)]
    public class ProjectAppService : ApplicationService, IProjectAppService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IObjectMapper _ObjectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public ProjectAppService(IProjectRepository projectRepository, ITreatmentRepository treatmentRepository, IHttpContextAccessor httpContextAccessor, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _projectRepository = projectRepository;
            _treatmentRepository = treatmentRepository;
            _httpContextAccessor = httpContextAccessor;
            _ObjectMapper = objectMapper;
            _mapper = mapper;
        }

        public async Task<ProjectsPageDto> GetProjectsPageAsync(
            int scenarioId, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, string sortDir, int page, int pageSize, bool unfunded = false, string committed = null)
        {
            var (items, totalCount) = await _projectRepository.GetPagedProjectsAsync(
                scenarioId, district, countyId, route, section, year,
                sortField, string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase),
                page, pageSize, unfunded, committed);

            var dtos = _mapper.Map(items.Select(i => i.Project).ToList());
            for (var i = 0; i < dtos.Count; i++)
                dtos[i].SequenceNo = items[i].SequenceNo;

            return new ProjectsPageDto { Data = dtos, TotalCount = totalCount };
        }

        public Task<int?> GetProjectIdBySequenceNoAsync(int scenarioId, int sequenceNo, bool unfunded = false)
            => _projectRepository.GetProjectIdBySequenceNoAsync(scenarioId, sequenceNo, unfunded);

        public Task<List<int>> GetProjectIdsAsync(int scenarioId, byte? district, byte? countyId, int? route, string section, int? year, bool unfunded = false, string committed = null)
            => _projectRepository.GetProjectIdsAsync(scenarioId, district, countyId, route, section, year, unfunded, committed);

        public Task<List<byte?>> GetDistrictsAsync(int scenarioId)
            => _projectRepository.GetDistricts(scenarioId);

        public async Task<List<CountyDto>> GetCountiesAsync(int scenarioId, int? district)
        {
            var counties = await _projectRepository.GetCounties(scenarioId, district);
            return counties.ConvertAll(c => _mapper.Map(c));
        }

        public Task<List<int?>> GetRoutesAsync(int scenarioId, int? district, int? county)
            => _projectRepository.GetRoutes(scenarioId, district, county);

        public Task<List<string>> GetSectionsAsync(int scenarioId, int? district, int? county, int? route)
            => _projectRepository.GetSections(scenarioId, district, county, route);

        public Task<List<int?>> GetYearsAsync(int scenarioId)
            => _projectRepository.GetYears(scenarioId);

        public async Task<PaginatedDto<ProjectDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var projects = await LoadProjects();
            var search_val = (httpContext.Request.Query["sSearch"]).ToString();
            var sort_col = (httpContext.Request.Query["iSortCol_0"]);
            var sort_column = (httpContext.Request.Query["mDataProp_" + sort_col]).ToString();
            var sortDirection = (httpContext.Request.Query["sSortDir_0"]).ToString();
            var propertyInfo = typeof(ProjectModel).GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(sort_column.ToLower()));
            if (search_val != "")
            {
                projects = projects.Where(
                    t => t.Section.ToLower().ToString().Contains(search_val.ToLower()) ||
                    t.District.ToString().Contains(search_val.ToLower()) ||
                    t.County.ToLower().ToString().Contains(search_val.ToLower()) ||
                    t.Route.ToString().Contains(search_val.ToLower()) ||
                    t.Direction.ToString().Contains(search_val.ToLower()) ||
                    t.Id.ToString().Contains(search_val.ToLower()) ||
                    Math.Round((decimal)t.Benefit, 2).ToString().Contains(search_val.ToLower()) ||
                    Math.Round((decimal)t.TotalCost, 2).ToString().Contains(search_val.ToLower()) ||
                    t.PreferredStartingYear.ToString().Contains(search_val.ToLower()) ||
                    t.NumberOfTreatments.ToString().Contains(search_val.ToLower()) ||
                    t.CommitmentStatus.ToString().Contains(search_val.ToLower()) ||
                    t.Selected.ToString().Contains(search_val.ToLower()) ||
                    t.SelectedFirstYear.ToString().Contains(search_val.ToLower())
                    );
            }
            projects = sortDirection == "asc" && propertyInfo != default
                   ? projects.OrderBy(t => propertyInfo.GetValue(t, null))
                   : projects.OrderByDescending(t => propertyInfo.GetValue(t, null));
            var displayResult = projects.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            var totalRecords = projects.Count();
            return new PaginatedDto<ProjectDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = displayResult
            };
        }
        public async Task<IEnumerable<ProjectDto>> LoadProjects()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Enumerable.Empty<ProjectDto>();

            var success = int.TryParse(httpContext.Session.GetString("ScenId"), out int scenid);
            if (!success)
                return Enumerable.Empty<ProjectDto>();

            var filterData = httpContext.Session.GetString("Filter");
            var filter = string.IsNullOrEmpty(filterData)
                ? new ProjectSearchModel()
                : JsonConvert.DeserializeObject<ProjectSearchModel>(filterData);

            filter.ScenarioId = scenid;
            var projects = await _projectRepository.FilterProjects(filter);
            return _ObjectMapper.Map<List<Entities.Project>, List<ProjectDto>>(projects);

        }
        public async Task ExportProjects(ExportProjectDto data)
        {
            var response = _httpContextAccessor.HttpContext?.Response;

            string filename = "";
            string storedProcedure = "";
            string storedProcedureTreatments = "";
            string errorMessage = "";
            int scenid = data.ScenarioId;
            string currentDate = DateTime.Now.ToString("yyyyMMdd");

            switch (data.ExportType)
            {
                case "DecisionSpace":
                    errorMessage = "Invalid export type selected.";
                    break;
                case "SummaryReport":
                    errorMessage = "Invalid export type selected.";
                    break;
                case "BAMS":
                    storedProcedure = "sp_pb_ExportBridgeProjects";
                    storedProcedureTreatments = "sp_pb_ExportNarrowBridgeTreatments";
                    filename = "BAMS";
                    break;
                case "PAMS":
                    storedProcedure = "sp_pb_ExportPavementProjects";
                    storedProcedureTreatments = "sp_pb_ExportNarrowPavementTreatments";
                    filename = "PAMS";
                    break;
                default:
                    storedProcedure = "sp_pb_ExportBridgeProjects";
                    storedProcedureTreatments = "sp_pb_ExportNarrowBridgeTreatments";
                    filename = "BAMS";
                    break;
            }

            if (string.IsNullOrEmpty(storedProcedure))
            {
                throw new BusinessException(errorMessage);
            }

            DataTable projectData = await _projectRepository.GetProjectData(storedProcedure, scenid);
            DataTable treatmentData = await _projectRepository.GetTreatmentData(storedProcedureTreatments, scenid);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var workbook = new ExcelPackage())
            {
                ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Projects");
                worksheet.Cells["A1"].LoadFromDataTable(projectData, true);

                ExcelWorksheet worksheet1 = workbook.Workbook.Worksheets.Add("Treatments");
                worksheet1.Cells["A1"].LoadFromDataTable(treatmentData, true);
                response.Headers.Add("content-disposition", "attachment;filename=" + filename + "-" + currentDate + ".xlsx");

                using (var memoryStream = new MemoryStream())
                {
                    await workbook.SaveAsAsync(memoryStream);
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(response.Body);
                }

                response.Body.Close();
            }
        }
        public async Task<bool> DeleteProject(int? projectid, int? scenid)
        {
            bool success = false;
            if (projectid.HasValue && scenid.HasValue)
            {
                var unAssignedTreatments = await _treatmentRepository.UnAssignTreatmentsFromProject(projectid.Value);
                success = await _projectRepository.DeleteProject(projectid.Value, scenid.Value);
            }
            if (!success)
            {
                throw new Exception("Could not delete the selected Project because it was not found, make sure that you have selected a scenario and a project then try again.");
            }
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int projectId, int scenId)
        {
            await _treatmentRepository.UnAssignTreatmentsFromProject(projectId);
            bool success = await _projectRepository.DeleteProject(projectId, scenId);
            if (!success)
                throw new UserFriendlyException("Could not delete the selected project.");
            return true;
        }

        public async Task<bool> UpdateNotesAsync(int scenarioId, int projectId, string userNotes)
        {
            return await _projectRepository.UpdateNotes(scenarioId, projectId, userNotes);
        }

        public async Task<ExportResultDto> GetExportAsync(ExportProjectDto data)
        {
            string sproc, sprocTreatments, name;
            if (data.ExportType == "PAMS")
            {
                sproc           = "sp_pb_ExportPavementProjects";
                sprocTreatments = "sp_pb_ExportNarrowPavementTreatments";
                name            = "PAMS";
            }
            else
            {
                sproc           = "sp_pb_ExportBridgeProjects";
                sprocTreatments = "sp_pb_ExportNarrowBridgeTreatments";
                name            = "BAMS";
            }

            var projectData   = await _projectRepository.GetProjectData(sproc, data.ScenarioId);
            var treatmentData = await _projectRepository.GetTreatmentData(sprocTreatments, data.ScenarioId);
            projectData = FilterSelectedProjectRows(projectData, data.Unfunded);
            projectData = FilterByProjectIds(projectData, data.ProjectIdsCsv);
            treatmentData = FilterTreatmentsByProjectIds(treatmentData, data.ProjectIdsCsv);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var workbook = new ExcelPackage();
            workbook.Workbook.Worksheets.Add("Projects").Cells["A1"].LoadFromDataTable(projectData, true);
            workbook.Workbook.Worksheets.Add("Treatments").Cells["A1"].LoadFromDataTable(treatmentData, true);

            using var ms = new MemoryStream();
            await workbook.SaveAsAsync(ms);
            return new ExportResultDto
            {
                Bytes    = ms.ToArray(),
                Filename = $"{name}-{DateTime.Now:yyyyMMdd}.xlsx"
            };
        }

        private static DataTable FilterSelectedProjectRows(DataTable projectsTable, bool unfunded)
        {
            if (!projectsTable.Columns.Contains("Selected"))
            {
                return projectsTable;
            }

            var filteredTable = projectsTable.Clone();
            foreach (DataRow row in projectsTable.Rows)
            {
                var selectedValue = row["Selected"]?.ToString();
                var isSelected = string.Equals(selectedValue, "True", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(selectedValue, "Y", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(selectedValue, "1", StringComparison.OrdinalIgnoreCase);
                if (isSelected != unfunded)
                {
                    filteredTable.ImportRow(row);
                }
            }

            return filteredTable;
        }

        private static HashSet<int> ParseProjectIds(string projectIdsCsv)
        {
            if (string.IsNullOrWhiteSpace(projectIdsCsv))
            {
                return new HashSet<int>();
            }

            return new HashSet<int>(projectIdsCsv.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value));
        }

        // The Projects sheet's ID column is aliased "ID" for PAMS and left as "ProjectId" for
        // BAMS (see sp_pb_ExportPavementProjects / sp_pb_ExportBridgeProjects) — check both.
        private static DataTable FilterByProjectIds(DataTable projectsTable, string projectIdsCsv)
        {
            var ids = ParseProjectIds(projectIdsCsv);
            if (ids.Count == 0)
            {
                return projectsTable;
            }

            var idColumn = projectsTable.Columns.Contains("ID") ? "ID"
                : projectsTable.Columns.Contains("ProjectId") ? "ProjectId"
                : null;
            if (idColumn == null)
            {
                return projectsTable;
            }

            var filteredTable = projectsTable.Clone();
            foreach (DataRow row in projectsTable.Rows)
            {
                if (row[idColumn] is int rowId && ids.Contains(rowId))
                {
                    filteredTable.ImportRow(row);
                }
            }

            return filteredTable;
        }

        // sp_pb_ExportNarrowPavementTreatments / sp_pb_ExportNarrowBridgeTreatments carry the
        // owning project as "ProjectId" (see script 38_add_projectid_to_export_treatment_sprocs.sql)
        // purely so the Treatments sheet can be scoped to the same filtered projects as the
        // Projects sheet — it isn't part of the legacy sheet schema, so it's dropped again
        // before the workbook is written, filtered or not.
        private static DataTable FilterTreatmentsByProjectIds(DataTable treatmentsTable, string projectIdsCsv)
        {
            if (!treatmentsTable.Columns.Contains("ProjectId"))
            {
                return treatmentsTable;
            }

            var ids = ParseProjectIds(projectIdsCsv);
            DataTable result;
            if (ids.Count == 0)
            {
                result = treatmentsTable;
            }
            else
            {
                result = treatmentsTable.Clone();
                foreach (DataRow row in treatmentsTable.Rows)
                {
                    if (row["ProjectId"] is int rowId && ids.Contains(rowId))
                    {
                        result.ImportRow(row);
                    }
                }
            }

            result.Columns.Remove("ProjectId");
            return result;
        }
    }
}
