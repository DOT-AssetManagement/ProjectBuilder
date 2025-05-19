using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using NuGet.LibraryModel;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ProjectBuilder.Web.MVC.Controllers
{
    [Authorize]
    public class TreatmentsController : LibraryBaseController
    {
        private readonly ITreatmentUnitOfWork _treatmentUnitOfWork;
        private readonly ITreatmentRepository _treatments;
        private readonly ProjectBuilderDbContext _context;
        public TreatmentsController(IFilterUnitOfWork filterUnitOfWork,
            IRepository<UserRoleModel> userRoleRepository,
            ITreatmentRepository treatments, ProjectBuilderDbContext context,
            IUserRepository users,ITreatmentUnitOfWork treatmentUnitOfWork) 
            :base(users, filterUnitOfWork, userRoleRepository)
        {
           _treatmentUnitOfWork = treatmentUnitOfWork;
            _treatments = treatments;
            _context= context;

        }
        public async Task<IActionResult> Index(string id, TreatmentViewModel searchModel)
        {
            if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {

                Guid libraryId;
                if (id != null)
                {
                    HttpContext.Session.SetString(Constents.LIBRARYIDKEY, id.ToString());
                }
                else
                {
                    libraryId = Guid.Empty;
                }
                _ = Guid.TryParse(HttpContext.Session.GetString(Constents.LIBRARYIDKEY), out libraryId);
                var treatments = await _treatments.GetByLibraryIdAsync(libraryId);
                ViewBag.Districts = treatments.Select(a => a.District).OrderBy(d => d).Distinct().ToList();
                if (searchModel.District != null)
                {
                    ViewBag.County = treatments.Where(a => a.District == searchModel.District).Select(a => a.County).OrderBy(a => a).Distinct().ToList();
                }

                if (searchModel.County != null)
                {
                    ViewBag.Route = treatments.Where(a => a.District == searchModel.District && a.CountyId == searchModel.County).Select(a => a.Route).OrderBy(a => a).Distinct().ToList();
                }

                if (searchModel.Route != null)
                {
                    ViewBag.Section = treatments.Where(a => a.District == searchModel.District &&
                a.CountyId == searchModel.County &&
                a.Route == searchModel.Route).Select(a => a.Section).OrderBy(a => a).Distinct().ToList();
                }

                ViewBag.id = id;
                var treatmentViewModel = new TreatmentViewModel { };
                var model = LoadSearchModel(libraryId, searchModel);
                TempData["TreartmentFilter"] = JsonConvert.SerializeObject(model);
                return View("Treatments", treatmentViewModel);
            }
            return RedirectToAction("Index", "Scenarios");
        }
        private TreatmentSearchModel LoadSearchModel(Guid libraryId, TreatmentViewModel searchModel)
        {
            TreatmentSearchModel model = new TreatmentSearchModel
            {
                AssetType = searchModel.AssetType,
                Cnty = searchModel.County,
                District = searchModel.District,
                Route = searchModel.Route,
                LibraryId = libraryId,     
                Direction = searchModel.FilterDirection,
                Year = searchModel.Year
            };
            var sections = searchModel.Section.DecomposeString<int>('-');
            if(sections.Length == 2)
            {
                model.FromSection = sections[0];
                model.ToSection = sections[1];
            }
            return model;
        }

        public async Task<ActionResult> ApplyPagination(JqueryDatatableParam param)
        {
            if (TempData["TreartmentFilter"] != null)
            {
                var treatmentfilter = JsonConvert.DeserializeObject<TreatmentSearchModel>(TempData["TreartmentFilter"].ToString());
                var treatments = await FilterUnitOfWork.TreatmentRepo.FilterTreatments(treatmentfilter);
                var search_val = (Request.Query["sSearch"]).ToString();
                var sort_col = (Request.Query["iSortCol_0"]);
                var sort_column = (Request.Query["mDataProp_" + sort_col]).ToString();
                var sortDirection = (Request.Query["sSortDir_0"]).ToString();

                var propertyInfo = typeof(UserTreatmentModel).GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(sort_column.ToLower()));

                if (search_val != "")
                {
                    treatments = treatments.Where(
                        t => t.Section.ToLower().Contains(search_val.ToLower()) ||
                        (t.AssetType == "P" && "Pavement".ToLower().Contains(search_val.ToLower())) ||
                        (t.AssetType == "B" && "Bridge".ToLower().Contains(search_val.ToLower())) ||
                        (t.District.HasValue && t.District.ToString().Contains(search_val.ToLower())) ||
                        t.County.ToLower().Contains(search_val.ToLower()) ||
                        t.Treatment.ToLower().Contains(search_val.ToLower()) ||
                        (t.BridgeId.HasValue && t.BridgeId.ToString().Contains(search_val.ToLower())) ||
                        (t.Route.HasValue && t.Route.ToString().Contains(search_val.ToLower())) ||
                        (t.Interstate.HasValue && t.Interstate.ToString().Contains(search_val.ToLower())) ||
                        (t.Direction.HasValue && t.Direction.ToString().Contains(search_val.ToLower())) ||
                        (t.Cost.HasValue && Math.Round((decimal)t.Cost / 1000000, 2).ToString().Contains(search_val.ToLower())) ||
                        (t.Benefit.HasValue && Math.Round((decimal)t.Benefit / 1000000, 2).ToString().Contains(search_val.ToLower())) ||
                        (t.Risk.HasValue && Math.Round((decimal)t.Risk, 2).ToString().Contains(search_val.ToLower())) ||
                        (t.PreferredYear.HasValue && t.PreferredYear.ToString().Contains(search_val.ToLower())) ||
                        (t.MinYear.HasValue && t.MinYear.ToString().Contains(search_val.ToLower())) ||
                        (t.MaxYear.HasValue && t.MaxYear.ToString().Contains(search_val.ToLower())) ||
                        $"{t.UserTreatmentName}".Contains(search_val.ToLower()) ||
                        (t.IndirectCostDesign.HasValue && t.IndirectCostDesign.ToString().Contains(search_val.ToLower())) ||
                        (t.IndirectCostRow.HasValue && t.IndirectCostRow.ToString().Contains(search_val.ToLower())) ||
                        (t.IndirectCostUtilities.HasValue && t.IndirectCostUtilities.ToString().Contains(search_val.ToLower())) ||
                        (t.IndirectCostOther.HasValue && t.IndirectCostOther.ToString().Contains(search_val.ToLower()))
                        ).ToList();
                }


                treatments = sortDirection == "asc" && propertyInfo != default
                    ? treatments.OrderBy(t => propertyInfo.GetValue(t, null)).ToList()
                    : treatments.OrderByDescending(t => propertyInfo.GetValue(t, null)).ToList();

                var districts = treatments.Select(a => a.District).Distinct().ToList();

                var displayResult = treatments.Skip(param.iDisplayStart)
                   .Take(param.iDisplayLength).ToList();
                var totalRecords = treatments.Count;
                TempData["TreartmentFilter"] = JsonConvert.SerializeObject(treatmentfilter);
                return Json(new
                {
                    param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = displayResult,
                    search_val = search_val
                });
            }
            return Json(new
            {
                param.sEcho,
                iTotalRecords = 0,
                iTotalDisplayRecords = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTreatment(Guid treatmentid)
         {
            try
            {
                _ = Guid.TryParse(HttpContext.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryId);

                var success = await FilterUnitOfWork.TreatmentRepo.DeleteAsync(libraryId, treatmentid);
                await FilterUnitOfWork.SaveChangesAsync();

                if (!success)
                {
                    return View("Error", new ErrorViewModel() { ErrorMessage = "Could not delete the selected Treatment because it was not found." });
                }
                var findScenario = _context.Scenarios.Where(a => a.LibraryId == libraryId).FirstOrDefault();
                if (findScenario != null)
                {
                    findScenario.Notes = "Candidate Pool has been modified. Please perform a scenario re-run";
                    findScenario.Stale = true;
                    _context.Scenarios.Update(findScenario);
                    await _context.SaveChangesAsync();

                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTreatment(TreatmentViewModel treatmentViewModel)
        {
            ValidateTreatmentOnEditing(treatmentViewModel);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(Json(errors));   
            }
            var updatedTreatment = new UserTreatmentModel
            {
                ImportTimeGeneratedId = treatmentViewModel.TreatmentId,
                Risk = treatmentViewModel.Risk,
                Cost  = treatmentViewModel.Cost,
                Benefit = treatmentViewModel.Benefit,
                PreferredYear = treatmentViewModel.PreferredYear,
                MinYear = treatmentViewModel.MinYear,
                MaxYear = treatmentViewModel.MaxYear,
                UserTreatmentTypeNo = treatmentViewModel.UserTreatmentTypeNo,
                IsCommitted = treatmentViewModel.IsCommitted,
                PriorityOrder = treatmentViewModel.PriorityOrder,
                IndirectCostDesign = treatmentViewModel.IndirectCostDesign,
                IndirectCostRow = treatmentViewModel.IndirectCostRow,
                IndirectCostOther = treatmentViewModel.IndirectCostOthers,
                IndirectCostUtilities = treatmentViewModel.IndirectCostUtilities,
                LibraryId = Guid.TryParse(HttpContext.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryid) ? libraryid : Guid.Empty,
            };

            var success = await FilterUnitOfWork.TreatmentRepo.EditTreatments(updatedTreatment);
            if (!success)
                return BadRequest(Json("Could not upadate the selected treatment please try again."));
            var findScenario = _context.Scenarios.Where(a => a.LibraryId == updatedTreatment.LibraryId).FirstOrDefault();
            if (findScenario != null)
            {
                findScenario.Notes = "Candidate Pool has been modified. Please perform a scenario re-run";
                findScenario.Stale = true;
                _context.Scenarios.Update(findScenario);
                await _context.SaveChangesAsync();

            }
            return Ok("Treatment was updated successfully.");
        }
        public async Task<IActionResult> GetTreatment(Guid treatmentid)
        {
            _ = Guid.TryParse(HttpContext.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryId);
            FilterUnitOfWork.TreatmentRepo.ApplyFilter(new Dictionary<string, object> { { "EntityId", treatmentid }, { "LibraryId", libraryId } });
            var result = await FilterUnitOfWork.TreatmentRepo.GetAllAsync();
            if (!result.Any())
                return BadRequest("could not find the selected treatment");
             return Json(result.FirstOrDefault());
        }
        [HttpPost]
        public async Task<IActionResult> CreateTreatment(TreatmentViewModel treatmentViewModel)
        {
            ValidateTreatmentOnCreation(treatmentViewModel);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(Json(errors));
            }    
            var newTreatment = UpdateOrCreateTreatment(treatmentViewModel);
            _ = Guid.TryParse(HttpContext.Session.GetString(Constents.LIBRARYIDKEY),out Guid libraryid);
            newTreatment.LibraryId = libraryid;
            var treatmentId =  await _treatmentUnitOfWork.CreateUserTreatment(newTreatment);
            return treatmentId == null ? BadRequest(Json("Could not create new treatment please try again.")) : Ok("Treatment was created successfully");
        }
        private void ValidateTreatmentOnEditing(TreatmentViewModel treatment)
        {
            var propertiestoIgnore = new string[] { "assettype" , "treatment" , "district" , "county" , "route" , "section", "interstate", "direction","bridgeid","brkey","projecttreatmentid", "scenarioid" };             
            foreach (var key in propertiestoIgnore)
            {
                SetValidationState(key);
            }
            ValidateTreatmentOnCreation(treatment);
        }
        private UserTreatmentModel UpdateOrCreateTreatment(TreatmentViewModel treatmentViewModel)
        {
            var sections = treatmentViewModel.Section.DecomposeString<int>('-');
            var result  = new UserTreatmentModel
            {
                Benefit = treatmentViewModel.Benefit,
                AssetType = treatmentViewModel.AssetType,
                BridgeId = treatmentViewModel.BridgeId,
                Brkey = treatmentViewModel.Brkey,
                Cost = treatmentViewModel.Cost,
                CountyId = treatmentViewModel.County,
                District = treatmentViewModel.District,
                IndirectCostDesign = treatmentViewModel.IndirectCostDesign,
                IndirectCostRow = treatmentViewModel.IndirectCostRow,
                IndirectCostOther = treatmentViewModel.IndirectCostOthers,
                IndirectCostUtilities = treatmentViewModel.IndirectCostUtilities,
                IsCommitted = treatmentViewModel.IsCommitted,
                IsUserTreatment = true,
                UserTreatmentTypeNo = treatmentViewModel.TreatmentType,
                Treatment = treatmentViewModel.Treatment,
                Route = treatmentViewModel.Route,
                Risk = treatmentViewModel.Risk,
                PreferredYear = treatmentViewModel.PreferredYear,
                PriorityOrder = treatmentViewModel.PriorityOrder,
                MaxYear = treatmentViewModel.MaxYear,
                MinYear = treatmentViewModel.MinYear,
                FromSection = sections.Length == 2 ? sections[0] : null,
                ToSection = sections.Length == 2 ? sections[1] : null,
                Interstate = treatmentViewModel.Interstate,
                Direction = (byte?)(treatmentViewModel.Direction ?1 : 0)
            };
            return result;
        }
        private void ValidateTreatmentOnCreation(TreatmentViewModel treatmentViewModel)
        {
            if (treatmentViewModel.IsCommitted)
            {
                treatmentViewModel.MinYear = treatmentViewModel.PreferredYear;
                treatmentViewModel.MaxYear = treatmentViewModel.PreferredYear;
                treatmentViewModel.PriorityOrder = 0;
                SetValidationState("minyear");
                SetValidationState("maxyear");
                SetValidationState("pariorityorder");
            } else if(treatmentViewModel.PriorityOrder is null)
            {
                treatmentViewModel.PriorityOrder = 4;
                SetValidationState("pariorityorder");
            }
            if(treatmentViewModel.AssetType != "B")
            {
                SetValidationState("bridgeid");
                SetValidationState("brkey");
            }
            SetValidationState("projecttreatmentid");
            SetValidationState("projectid");
        }
        private void SetValidationState(string key, ModelValidationState state = ModelValidationState.Valid)
        {
            var modelstate = ModelState.GetValueOrDefault(key);
            if (modelstate != null)
                modelstate.ValidationState =state;
        }
    }
}
