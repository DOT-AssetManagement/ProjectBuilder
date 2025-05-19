using GisJsonHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.UI.Areas.MicrosoftIdentity.Controllers;
using Newtonsoft.Json;
using PBLogic;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.IO;

namespace ProjectBuilder.Web.MVC.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ITreatmentUnitOfWork _treatmentUnitOfWork;
        private readonly IProjectRepository _projects;
        private readonly IRepository<ScenarioModel> _scenarios;
        private readonly string _connectionString;
        public ProjectsController(ITreatmentUnitOfWork treatmentUnitOfWork,IProjectRepository projects, IRepository<ScenarioModel> scenarios, IConfiguration configuration) 
        {
            _treatmentUnitOfWork = treatmentUnitOfWork;
            _projects = projects;
            _scenarios = scenarios;
            _connectionString = configuration.GetConnectionString("Default");
        }
        public async Task<IActionResult> Index(int? scenid, string exporting, string error = "")
        {
            var projectsVm = new ProjectsViewModel();
            string scenarioName = "";
            if(scenid.HasValue)
            {
                var scenario = await _scenarios.FindAsync(scenid);
                scenarioName = scenario is null ? "" : scenario.ScenarioName;
            }
            projectsVm.ScenarioName = scenarioName;
            projectsVm.ScenarioId = scenid;
            projectsVm.IsExporting = exporting;
            projectsVm.Years = await _projects.GetYears(scenid);
            TempData["Scenid"] = scenid;
            if (!string.IsNullOrEmpty(error))
            {
                ViewData["ErrorMessage"] = error;
            }

            return View("~/Views/Outputs/Projects.cshtml",projectsVm);
        }
        [HttpPost]
        public IActionResult ApplyFilter(ProjectSearchModel filter)
        {
            TempData["Filter"] = JsonConvert.SerializeObject(filter);
            return Ok();
        }
        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
            var projects = await LoadProjects();
            var search_val = (Request.Query["sSearch"]).ToString();
            var sort_col = (Request.Query["iSortCol_0"]);
            var sort_column = (Request.Query["mDataProp_" + sort_col]).ToString();
            var sortDirection = (Request.Query["sSortDir_0"]).ToString();
            var propertyInfo = typeof(ProjectModel).GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(sort_column.ToLower()));
            if (search_val != "")
               {
                    projects = projects.Where(
                        t => t.Section.ToLower().ToString().Contains(search_val.ToLower()) ||                       
                        t.District.ToString().Contains(search_val.ToLower()) ||
                        t.County.ToLower().ToString().Contains(search_val.ToLower()) ||
                        t.Route.ToString().Contains(search_val.ToLower()) || 
                        t.Direction.ToString().Contains(search_val.ToLower()) || 
                        t.ProjectId.ToString().Contains(search_val.ToLower()) || 
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
             var displayResult = projects.Skip(param.iDisplayStart).Take(param.iDisplayLength);
             var totalRecords = projects.Count();
            return Json(new
            {
              param.sEcho,
              iTotalRecords = totalRecords,
              iTotalDisplayRecords = totalRecords,
              aaData = displayResult
            });
        }

        [HttpPost]
        public async Task ExportProjects(ExportProjectViewModel data)
        {
            var filename = "";
            string storedProcedure = "";
            string storedProcedureTreatments = "";
            string errorMessage = "";
            int scenid = data.ScenarioId;

            string currentDate = DateTime.Now.ToString("yyyyMMdd");

            switch (data.ExportType)
            {
                case "DecisionSpace":
                    storedProcedure = "";
                    errorMessage = "Invalid export type selected.";
                    filename = "";
                    break;
                case "SummaryReport":
                    storedProcedure = "";
                    filename = "";
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


            if (storedProcedure != "")
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var command = new SqlCommand();

                    command.Connection = connection;
                    command.CommandText = storedProcedure;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 24000;
                    command.Parameters.Add("ScenId", SqlDbType.Int).Value = scenid;

                    DataTable dataTable = new DataTable();
                    var dataAdapter = new SqlDataAdapter(command);
                    dataAdapter.Fill(dataTable);


                    var command1 = new SqlCommand();

                    command1.Connection = connection;
                    command1.CommandText = storedProcedureTreatments;
                    command1.CommandType = CommandType.StoredProcedure;
                    command1.CommandTimeout = 24000;
                    command1.Parameters.Add("ScenId", SqlDbType.Int).Value = scenid;

                    DataTable dataTable1 = new DataTable();
                    var dataAdapter1 = new SqlDataAdapter(command1);
                    dataAdapter1.Fill(dataTable1);

                    using (var workbook = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Projects");
                        worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                        ExcelWorksheet worksheet1 = workbook.Workbook.Worksheets.Add("Treatments");
                        worksheet1.Cells["A1"].LoadFromDataTable(dataTable1, true);

                        Response.Headers.Add("content-disposition", "attachment;filename=" + filename + "-" + currentDate + ".xlsx");

                        using (var memoryStream = new MemoryStream())
                        {
                            await workbook.SaveAsAsync(memoryStream);
                            memoryStream.Position = 0;

                            await memoryStream.CopyToAsync(Response.Body);
                        }

                        Response.Body.Close();
                    }

                    connection.Close();

                }
            }
            else
            {
                var redirectUrl = Url.Action(nameof(Index), "Projects", new { exporting = true, scenid = scenid, error = errorMessage });
                Response.Redirect(redirectUrl);
                return; 
            }

        }

        private void GetConnectionString(string v)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<ProjectModel>> LoadProjects()
        {
            var success = int.TryParse(TempData.Peek("ScenId")?.ToString(), out int scenid);
            if(!success)
                return Enumerable.Empty<ProjectModel>();
            var filterdata = TempData.Peek("Filter");
            var filter = filterdata == null ? new() : JsonConvert.DeserializeObject<ProjectSearchModel>(filterdata.ToString());
            filter.ScenarioId = scenid;
            return await _projects.FilterProjects(filter);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int? projectid,int? scenid)
        {
            bool success = false;
            if (projectid.HasValue && scenid.HasValue)
            {
               var unAssignedTreatments = await _treatmentUnitOfWork.UnAssignTreatmentsFromProject(projectid.Value);
               success = await _projects.DeleteProject(projectid.Value,scenid.Value);
               await _projects.SaveChangesAsync();
            }
            if (!success)
               return BadRequest("Could not delete the selected Project because it was not found, make sure that you have selected a scenario and a project then try again.");
            return Ok($"the project with Id: {projectid} has been successfully deleted.");
        }
    }
}
