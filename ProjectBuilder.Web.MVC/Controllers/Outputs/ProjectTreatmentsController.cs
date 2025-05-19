using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;
using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace ProjectBuilder.Web.MVC.Controllers
{
    [Authorize]
    public class ProjectTreatmentsController : Controller
    {
        private readonly ITreatmentUnitOfWork _treatmentUnitOfWork;
        private readonly IProjectRepository _projects;
        private readonly IFilterUnitOfWork _filterUnitOfWork;
        private readonly IProjectTreatmentRepository _projectTreatment;
		private ProjectBuilderDbContext _context;

		public ProjectTreatmentsController(ITreatmentUnitOfWork treatmentUnitOfWork, IProjectTreatmentRepository projectTreatment, IProjectRepository project,IFilterUnitOfWork filterUnitOfWork, ProjectBuilderDbContext context)
        {
            _treatmentUnitOfWork = treatmentUnitOfWork;
            _filterUnitOfWork = filterUnitOfWork;
            _projects = project;
            _projectTreatment = projectTreatment;
            _context = context;
        }
        public async Task<IActionResult> Index(int? scenid,int? projectid)
        {
            var scenName = "";
            var projectTreatments = new ProjectTreatmentsViewModel();
            if(scenid.HasValue)
            {
                var scenario = await _filterUnitOfWork.ScenarioRepo.FindAsync(scenid.Value);
                scenName = scenario is null ? "" : scenario.ScenarioName;
                projectTreatments.Years = await _filterUnitOfWork.ProjectTreatmentRepo.GetYears(scenid.Value);
            }
            projectTreatments.ScenarioName = scenName;
            projectTreatments.ProjectId = projectid;
            projectTreatments.ScenarioId = scenid;
            TempData["ScenId"] = scenid;
            TempData["ProjectId"] = projectid;
            return View("~/Views/Outputs/ProjectTreatments.cshtml",projectTreatments);
        }

        [HttpPost]
        public IActionResult ApplyFilter(TreatmentSearchModel filter)
        {
            TempData["Filter"] = JsonConvert.SerializeObject(filter);
            return Ok();
        }
        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
               var treatments = await LoadProjectTreatments();
               var search_val = (Request.Query["sSearch"]).ToString();
               var sort_col = (Request.Query["iSortCol_0"]);
               var sort_column = (Request.Query["mDataProp_" + sort_col]).ToString();
               var sortDirection = (Request.Query["sSortDir_0"]).ToString();
               var propertyInfo = typeof(ProjectTreatmentModel).GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(sort_column.ToLower()));
               if (search_val != "")
                    {
                        treatments = treatments.Where(
                            t => t.Section.ToLower().ToString().Contains(search_val.ToLower()) ||
                            (t.AssetType == "P" && "Pavement".ToLower().Contains(search_val.ToLower())) ||
                            (t.AssetType == "B" && "Bridge".ToLower().Contains(search_val.ToLower())) ||
                            t.District.ToString().Contains(search_val.ToLower()) ||
                            t.County.ToLower().ToString().Contains(search_val.ToLower()) ||
                            t.TreatmentType.ToLower().ToString().Contains(search_val.ToLower()) ||
                            t.BridgeId.ToString().Contains(search_val.ToLower()) ||
                            t.Route.ToString().Contains(search_val.ToLower()) ||
                            t.Interstate.ToString().Contains(search_val.ToLower()) ||
                            t.Direction.ToString().Contains(search_val.ToLower()) ||
                            Math.Round((decimal)t.TotalCost / 1000000, 2).ToString().Contains(search_val.ToLower()) ||
                            Math.Round((decimal)t.Benefit / 1000000, 2).ToString().Contains(search_val.ToLower()) ||
                            Math.Round((decimal)t.Risk, 2).ToString().Contains(search_val.ToLower()) ||
                            t.PreferredYear.ToString().Contains(search_val.ToLower()) ||
                            t.MinYear.ToString().Contains(search_val.ToLower()) ||
                            t.MaxYear.ToString().Contains(search_val.ToLower()) ||  
                            t.IndirectCostDesign.ToString().Contains(search_val.ToLower()) ||
                            t.IndirectCostRow.ToString().Contains(search_val.ToLower()) ||
                            t.IndirectCostUtilities.ToString().Contains(search_val.ToLower()) ||
                            t.IndirectCostOther.ToString().Contains(search_val.ToLower())
                            ).ToList();
                    }
                    treatments = sortDirection == "asc" && propertyInfo != default
                        ? treatments.OrderBy(t => propertyInfo.GetValue(t, null))
                        : treatments.OrderByDescending(t => propertyInfo.GetValue(t, null));
                    var displayResult = treatments.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                    var totalRecords = treatments.Count();
                    return Json(new
                    {
                        param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = totalRecords,
                        aaData = displayResult
                    });
        }
        private async Task<IEnumerable<ProjectTreatmentModel>> LoadProjectTreatments()
        {
            var hasscenid = int.TryParse(TempData.Peek("ScenId")?.ToString(),out int scenid);
            var hasprojectid = int.TryParse(TempData.Peek("ProjectId")?.ToString(), out int projectid);
            if(!hasscenid || !hasprojectid)
                return Enumerable.Empty<ProjectTreatmentModel>();
            var filterdata = TempData.Peek("Filter");
            var filter = filterdata == null ? new() : JsonConvert.DeserializeObject<TreatmentSearchModel>(filterdata.ToString());
            filter.ScenarioId = scenid;
            filter.ProjectId = projectid;
           return await _filterUnitOfWork.ProjectTreatmentRepo.FilterProjectTreatments(filter);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProjectTreatment(long? treatmentid, TreatmentViewModel treatmentViewModel)
        {
            var success = await _filterUnitOfWork.ProjectTreatmentRepo.DeleteAsync(treatmentid);
            await _filterUnitOfWork.SaveChangesAsync();
            if (success)
            {
                var scenario = await _context.Scenarios.FindAsync(treatmentViewModel.ScenarioId);

                if (scenario != null)
                {
                    scenario.Stale = true;
                    scenario.Notes = "Scenario Needs to be Run Again";
                    _context.Scenarios.Update(scenario);
                    _context.SaveChanges();
                }

            }
            if (!success)
            {
                return BadRequest("Could not delete the selected Project Treatment because it was not found.");
            }

            return Ok($"Treatment with id: {treatmentid} has been successfully deleted");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProjectTreatment(TreatmentViewModel treatmentViewModel)
        {
            ValidateTreatmentOnEditing(treatmentViewModel);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(Json(errors));
            }

            var success = await _filterUnitOfWork.ProjectTreatmentRepo.EditProjectTreatments(new ProjectTreatmentModel
            {
                ProjectTreatmentId = treatmentViewModel.ProjectTreatmentId,
                Benefit = treatmentViewModel.Benefit,
                DirectCost = treatmentViewModel.Cost,
                IsCommitted = treatmentViewModel.IsCommitted,
                UserTreatmentTypeNo = treatmentViewModel.TreatmentType,
                Risk = treatmentViewModel.Risk,
                PreferredYear = treatmentViewModel.PreferredYear,
                PriorityOrder = treatmentViewModel.PriorityOrder,
                MaxYear = treatmentViewModel.MaxYear,
                MinYear = treatmentViewModel.MinYear,
                IndirectCostDesign = treatmentViewModel.IndirectCostDesign,
                IndirectCostRow = treatmentViewModel.IndirectCostRow,
                IsUserCreated = true,
                IndirectCostOther = treatmentViewModel.IndirectCostOthers,
                IndirectCostUtilities = treatmentViewModel.IndirectCostUtilities
            });
			//var stale = await _filterUnitOfWork.ProjectTreatmentRepo.CheckStale(treatmentViewModel.ScenarioId);

			//if (!stale)
			//{
			//	var scenario = await _filterUnitOfWork.ScenarioRepo.FindAsync(treatmentViewModel.ScenarioId);
			//	scenario.Notes = "Scenario Needs to be Run Again";
			//	scenario.Stale = true;
			//	await _filterUnitOfWork.ScenarioRepo.UpdateAsync(scenario);
			//}
			//return Ok("Treatment was updated successfully.");
			//if (success)
			//{
			//    var scenario = await _filterUnitOfWork.ScenarioRepo.FindAsync(treatmentViewModel.ScenarioId);
			//    scenario.Notes = "Scenario Needs to be Run Again";
			//    await _filterUnitOfWork.ScenarioRepo.UpdateAsync(scenario);
			//}

			//if (!success)
			//    return BadRequest(Json("Could not upadate the selected treatment please try again."));
			//return Ok("Treatment was updated successfully.");
			if (success)
			{
				var scenario = await _context.Scenarios.FindAsync(treatmentViewModel.ScenarioId);

				if (scenario != null)
				{
					scenario.Stale = true;
					//await _filterUnitOfWork.ScenarioRepo.UpdateAsync(scenario);
					scenario.Notes = "Scenario Needs to be Run Again";
					 _context.Scenarios.Update(scenario);
					 _context.SaveChanges();

				}
			}
			return Ok("Treatment was updated successfully.");
		}
       
        public async Task<IActionResult> CheckRunAvailAbilty(int? scenarioId)
        {
            try
            {
                var check = await _projectTreatment.CheckIsUserCreated(scenarioId);
                return Json(check);
            }
            catch(Exception)
            {
                throw;
            }
        }
        
        
        
        
        public async Task<IActionResult> GetProjectTreatment(long? projectTreatmentId)
        {
            var target = await _filterUnitOfWork.ProjectTreatmentRepo.FindAsync(projectTreatmentId);
            if (target == null)
                return BadRequest("could not find the selected treatment");
            return Json(target);
        }
        private void ValidateTreatmentOnEditing(TreatmentViewModel treatment)
        {
            var propertiestoIgnore = new string[] { "assettype" , "treatment" , "district" , "county" , "route" , "section", "interstate", "direction",
               "bridgeid","brkey","projectId","treatmentid","scenarioid"};
            foreach (var key in propertiestoIgnore)
            {
                SetValidationState(key);
            }
            ValidateGeneratedValues(treatment);
        }
        private void ValidateGeneratedValues(TreatmentViewModel treatmentViewModel)
        {
            if (treatmentViewModel.IsCommitted)
            {
                treatmentViewModel.MinYear = treatmentViewModel.PreferredYear;
                treatmentViewModel.MaxYear = treatmentViewModel.PreferredYear;
                treatmentViewModel.PriorityOrder = 0;
                SetValidationState("minyear");
                SetValidationState("maxyear");
                SetValidationState("pariorityorder");
            }
            else if (treatmentViewModel.PriorityOrder is null)
            {
                treatmentViewModel.PriorityOrder = 4;
                SetValidationState("pariorityorder");
            }
            if (treatmentViewModel.AssetType != "B")
            {
                SetValidationState("bridgeid");
                SetValidationState("brkey");
            }
        }
        private void SetValidationState(string key, ModelValidationState state = ModelValidationState.Valid)
        {
            var modelstate = ModelState.GetValueOrDefault(key);
            if (modelstate != null)
                modelstate.ValidationState = ModelValidationState.Valid;
        }
    }
}
