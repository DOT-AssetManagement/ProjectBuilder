using GisJsonHandler;
using log4net.Filter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
    [Authorize]
    public class ScenariosController : LibraryBaseController
    {
        private readonly IRunScenarioUnitOfWork _runScenarioUnit;
        private readonly ISession _session;
        private readonly IFilterUnitOfWork _filter;
        private readonly ICandidatePoolUnitOfWork _libraryUnitOfWork;
        private readonly IProjectTreatmentRepository _projectTreatment;



        public ScenariosController(IUserRepository users,
        IHttpContextAccessor httpContextAccessor,
        IFilterUnitOfWork filter,
         ICandidatePoolUnitOfWork libraryUnitOfWork,
            IRepository<UserRoleModel> userRoleRepository, IFilterUnitOfWork filterUnitOfWork, IProjectTreatmentRepository projectTreatment, IRunScenarioUnitOfWork runScenario)
            : base(users, filterUnitOfWork, userRoleRepository)
        {
            _runScenarioUnit = runScenario;
            _session = httpContextAccessor.HttpContext.Session;
            _filter = filter;
            _libraryUnitOfWork = libraryUnitOfWork;
            _projectTreatment = projectTreatment;

        }
        public async Task<IActionResult> Index(String SearchString = "")
        {
            if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
            {
                var user = await GetOrCreateUser();
                var userRole = await GetUserRole(user.EntityId);
                if (userRole == default)
                {
                    throw new Exception("No Role is Assigned to User Yet!");
                }
                var role = await GetRole(userRole.RoleId);
                if (role == default)
                {
                    throw new Exception("No Role is Found against RoleId: " + userRole.RoleId);
                }
                HttpContext.Session.SetString("RoleName", role.Name);
                var libraryVm = new CandidatePoolViewModel();
                ApplayUserFilter(user.EntityId);
                libraryVm.Libraries = await _filter.CandidatePoolRepo.GetAllAsync();
                var Count = libraryVm.Libraries.Where(a => a.UserId == user.EntityId).Count();
                var SharedCount = libraryVm.Libraries.Where(a => a.IsShared == true).Count();
                ViewBag.LibrariesCount = Count + SharedCount;
                var scenarioVm = new ScenarioViewModel(); 
                scenarioVm.Scenarios = await FilterUnitOfWork.ScenarioRepo.GetAllAsync();
                scenarioVm.Scenarios = scenarioVm.Scenarios.Where(a => a.CreatedBy == user.Email);
                foreach (var scenario in scenarioVm.Scenarios)
                {
                    var library = await _filter.CandidatePoolRepo.FindAsync(scenario.LibraryId);
                    scenario.CandidatePool = library?.Name ?? "";
                }
                foreach (var item in scenarioVm.Scenarios) 
                {
                    var check = await _projectTreatment.CheckStale(item.ScenarioId);
                    if (check)
                    {
                        item.Notes = "Scenario treatments have been modified. To apply the changes, please perform a scenario re-run.";
                        item.ReRun = true;
                    }
                }
                if (!string.IsNullOrEmpty(SearchString))
                    scenarioVm.Scenarios = scenarioVm.Scenarios.Where(scenario => scenario.ScenarioName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();

                scenarioVm.SearchString = SearchString;

                return View("Scenarios", scenarioVm);
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> GetScenarios()
        {            
            var user = await GetOrCreateUser();
            var userRole = await GetUserRole(user.EntityId);
            if (userRole == default)
            {
                throw new Exception("No Role is Assigned to User Yet!");
            }
            var role = await GetRole(userRole.RoleId);
            if (role == default)
            {
                throw new Exception("No Role is Found against RoleId: " + userRole.RoleId);
            }
            HttpContext.Session.SetString("RoleName", role.Name);
            var libraryVm = new CandidatePoolViewModel();
            ApplayUserFilter(user.EntityId);
            libraryVm.Libraries = await _filter.CandidatePoolRepo.GetAllAsync();
            var Count = libraryVm.Libraries.Where(a => a.UserId == user.EntityId).Count();
            ViewBag.LibrariesCount = Count;
            var scenariosVm = new ScenarioViewModel();
            scenariosVm.Scenarios = await FilterUnitOfWork.ScenarioRepo.GetAllAsync();
            scenariosVm.Scenarios = scenariosVm.Scenarios.Where(a => a.CreatedBy == user.Email);
            


            return Json(scenariosVm);            
        }


        public async Task<IActionResult> CreateScenario(string id)
        {
            var libraryId = await LoadCandidatePoolsForUser();
            var lib = ViewBag.Libraries;
            return View("CreateScenario");
        }
        [HttpPost]
        public async Task<IActionResult> CreateScenario([FromBody] ScenarioViewModel scenarioViewModel)
        {
            await CreateNewScenario(scenarioViewModel);
            await CreateScenarioParameter(scenarioViewModel);
            await CreateScenarioBudget(scenarioViewModel);
            return RedirectToAction("Index", "Scenarios");
        }
        #region MethodsForCreateNewScenario
        public async Task<IActionResult> CreateScenarioParameter(ScenarioViewModel scenarioViewModel)
        {
            foreach (var item in scenarioViewModel.parameters)
            {
                if (item.ParameterId.ToLower().Contains("yfst") || item.ParameterId.ToLower().Contains("ylst"))
                    continue;
                var paramter = new ScenarioParameterModel
                {
                    ScenarioId = scenarioViewModel.ScenarioId,
                    ParameterValue = !string.IsNullOrEmpty(item.DefaultValue) && !item.DefaultValue.ToLower().Contains("null") ? Convert.ToDouble(item.DefaultValue) : null,
                    ParameterId = item.ParameterId
                };
                _runScenarioUnit.ScenarioParametersRepo.Insert(paramter);
            }
            return Ok();
        }
        public async Task<IActionResult> CreateScenarioBudget(ScenarioViewModel scenarioViewModel)
        {
            _runScenarioUnit.ScenariosBudgetsRepo.ApplyFilter(new Dictionary<string, object> { { "ScenarioId", scenarioViewModel.ScenarioId } });
            var checkExistingScenarioBudgets = await _runScenarioUnit.ScenariosBudgetsRepo.GetAllAsync();
            try
            {
                if (checkExistingScenarioBudgets != null && checkExistingScenarioBudgets.Count() > 0)
                {
                    return Ok();
                }

                var scenBud = new ScenarioBudgetFlatModel();
                scenBud.ScenarioId = scenarioViewModel.ScenarioId;

                foreach (var item in scenarioViewModel.budgetConstraints)
                {
                        scenBud.YearWork = item.YearofWork;
                        scenBud.District = item.District;
                        scenBud.BridgeInterstateBudget = (item.bridgeInterstateBudget * 1000000);
                        scenBud.BridgeNonInterstateBudget = (item.bridgeNonInterstateBudget * 1000000);
                        scenBud.PavementInterstateBudget = (item.pavementNonInterstateBudget * 1000000);
                        scenBud.PavementNonInterstateBudget = (item.pavementNonInterstateBudget * 1000000);
                        await _runScenarioUnit.ScenariosBudgetsRepo.InsertAndSave(scenBud);
                }
                await _runScenarioUnit.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        
            return Ok();
        }
        public async Task<Guid> CreateScenLib(ScenarioViewModel scenarioViewModel)
        {
            var currentUser = await GetOrCreateUser();
            var target = await _filter.CandidatePoolRepo.FindAsync(scenarioViewModel.LibraryId);
            var newLibrary = new CandidatePoolModel();
            foreach (var item in scenarioViewModel.newLibrarySend)
            {
                newLibrary.UserId = currentUser.EntityId;
                newLibrary.Name = item.NewLibName;
                newLibrary.Description = item.librarydescription;
                newLibrary.IsActive = true;
                newLibrary.IsShared = item.IsShared;
            }

            var libid = await _libraryUnitOfWork.CreateNewCandidatePool(newLibrary);
            foreach (var item in scenarioViewModel.newLibrarySend)
            {
                if (libid != Guid.Empty && !item.IsEmptyLibrary)
                    await _libraryUnitOfWork.PopulateCandidatePool(libid, Convert.ToString(scenarioViewModel.LibraryId), string.IsNullOrEmpty(item.SelectedAsset) ? null : item.SelectedAsset == "Brdige" ? "B" : "P", item.SelectedDistrict,
                                                             item.SelectedCounty, item.SelectedRoute, item.LibMinYear, item.LibMaxYear);
            }

            return libid;
        }
        public async Task<IActionResult> CreateNewScenario(ScenarioViewModel scenarioViewModel)
        {
            var currentUser = await GetOrCreateUser();
            scenarioViewModel.CreatedBy = currentUser.Email;
            if (scenarioViewModel.SelectScen == "")
            {
                var currentLibId = scenarioViewModel.LibraryId;
                if (Guid.TryParse(currentLibId.ToString(), out Guid id))
                {
                    if (Guid.Empty != id && scenarioViewModel.FirstYear.HasValue && scenarioViewModel.LastYear.HasValue)
                    {
                        var scenarioArgs = await _runScenarioUnit.CreateScenario(scenarioViewModel.ScenarioName, id, scenarioViewModel.FirstYear.Value, scenarioViewModel.LastYear.Value, 0, scenarioViewModel.CreatedBy);
                        if (scenarioArgs.Level == LogLevel.Error)
                            return BadRequest(Json("Unexpected Error occured while creating scenario."));

                        scenarioViewModel.ScenarioId = scenarioArgs.ScenarioId;
                    }
                }
            }
            else
            {
                var libidTask = CreateScenLib(scenarioViewModel);
                var libid = await libidTask;

                if (Guid.TryParse(libid.ToString(), out Guid libid1))
                {
                    if (Guid.Empty != libid && scenarioViewModel.FirstYear.HasValue && scenarioViewModel.LastYear.HasValue)
                    {
                        var scenarioArgs = await _runScenarioUnit.CreateScenario(scenarioViewModel.ScenarioName, libid, scenarioViewModel.FirstYear.Value, scenarioViewModel.LastYear.Value, 0, scenarioViewModel.CreatedBy);
                        if (scenarioArgs.Level == LogLevel.Error)
                            return BadRequest(Json("Unexpected Error occured while creating scenario."));

                        scenarioViewModel.ScenarioId = scenarioArgs.ScenarioId;
                    }
                }
            }
            return Ok();
        }
        private async Task CreateNewLibrary(CandidatePoolViewModel libraryViewModel, long userId, string? libraraysource = null)
        {
            var newLibrary = new CandidatePoolModel
            {
                UserId = userId,
                Name = libraryViewModel.Name,
                Description = libraryViewModel.Description,
                IsActive = true,
                IsShared = libraryViewModel.IsShared
            };
            var id = await _libraryUnitOfWork.CreateNewCandidatePool(newLibrary);
            if (id != Guid.Empty && !libraryViewModel.IsEmptyLibrary)
                await _libraryUnitOfWork.PopulateCandidatePool(id, libraraysource, string.IsNullOrEmpty(libraryViewModel.SelectedAsset) ? null : libraryViewModel.SelectedAsset == "Brdige" ? "B" : "P", libraryViewModel.SelectedDistrict,
                                                         libraryViewModel.SelectedCounty, libraryViewModel.SelectedRoute, libraryViewModel.MinYear, libraryViewModel.MaxYear);
        }
        private void ApplayUserFilter(long userId)
        {
            _filter.CandidatePoolRepo.UserId = userId;
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> CheckScenarioName(string attemptedname)
        {
            FilterUnitOfWork.ScenarioRepo.ApplyFilter(new Dictionary<string, object> { { "ScenarioName", attemptedname } });
            var result = await FilterUnitOfWork.ScenarioRepo.GetAllAsync();
            if (!result.Any())
                return Json(Ok());
            return BadRequest("name already exists.");
        }

        public async Task<IActionResult> DeleteScenerio(int scenerioId)
        {
            var success = await FilterUnitOfWork.ScenarioRepo.DeleteAsync(scenerioId);
            await FilterUnitOfWork.SaveChangesAsync();
            if (!success)
                return View("Error", new ErrorViewModel() { ErrorMessage = "Could not delete the selected Scenerio because it was not found." });
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> RunScenario(int? scenarioId)
        {
            var user = await GetOrCreateUser();
            if (scenarioId.HasValue)
            {
                var scenarioEventArgs = await _runScenarioUnit.RunScenario(scenarioId.Value, user.Name, true);

                return scenarioEventArgs.Level == LogLevel.Error ? BadRequest(scenarioEventArgs.ErrorMessage) : Json(Ok());
            }
            return BadRequest("could not get the scenario id, make sure that you have selected a scenario then try again.");
        }
        [HttpPost]
        public async Task<IActionResult> CheckScenrioDeletion(int? scenId)
        {
            var check = false;
            var AllScenarios = await FilterUnitOfWork.ScenarioRepo.GetAllAsync();
            var scenario= AllScenarios.Find(a => a.ScenarioId == scenId);
            if (scenario.LibraryId.HasValue)
            {
                check = true;
            }
            return Json(check);
        }
    }
}

