using GisJsonHandler;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Identity.Web;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Web.MVC.Models;

namespace ProjectBuilder.Web.MVC.Controllers
{
	[Authorize]
	public class CandidatePoolsController : Controller
	{
		private readonly IFilterUnitOfWork _filter;
		private readonly IUserRepository _users;
		private readonly ICandidatePoolUnitOfWork _libraryUnitOfWork;
		private readonly ISession _session;
		private readonly ITreatmentUnitOfWork _treatmentUnitOfWork;
		private readonly IFilterUnitOfWork _filterUnitOfWork;
		private readonly ITreatmentRepository _treatments;
		private readonly ProjectBuilderDbContext _context;
		private readonly ICandidatePoolRepository _candidate;

		public Guid Libid { get; set; }
		public CandidatePoolsController(IFilterUnitOfWork filter, ProjectBuilderDbContext context, IHttpContextAccessor httpContextAccessor, IFilterUnitOfWork filterUnitOfWork, ITreatmentRepository treatments, IUserRepository users, ICandidatePoolUnitOfWork libraryUnitOfWork, ITreatmentUnitOfWork treatmentUnitOfWork)
		{
			_libraryUnitOfWork = libraryUnitOfWork;
			_treatmentUnitOfWork = treatmentUnitOfWork;
			_filter = filter;
			_filterUnitOfWork = filterUnitOfWork;
			_users = users;
			_session = httpContextAccessor.HttpContext.Session;
			_treatments = treatments;
			_context = context;
		}
		public async Task<IActionResult> Index(int? scenid, bool? importing, string SearchString = "")
		{
			try
			{
				if (User.Identity?.IsAuthenticated == true && HttpContext.Session.GetString("RoleName") != null)
				{

					var candidatePoolVm = new CandidatePoolViewModel();
					var scenarioVm = new ScenarioViewModel();
					var currentUser = await GetOrCreateUser();
					ViewData["CurrentUserName"] = currentUser.Name;
					ApplayUserFilter(currentUser.EntityId);
					candidatePoolVm.Libraries = await _filter.CandidatePoolRepo.GetAllAsync();
					scenarioVm.Scenarios = await _filter.ScenarioRepo.GetAllAsync();
					await GetSource(candidatePoolVm);
					await ImportingLibrary(scenid, importing, candidatePoolVm);

					if (HttpContext.Session.GetString("RoleName") == "Guest")
					{
						return View("GuestIndex", candidatePoolVm);
					}

					if (scenid.HasValue)
					{
						SearchString = candidatePoolVm.Name;
						candidatePoolVm.Libraries = candidatePoolVm.Libraries
							.Where(library => library.Name == SearchString)
							.ToList();
						candidatePoolVm.SearchString = SearchString;
					}
					else if (!string.IsNullOrEmpty(SearchString))
					{
						candidatePoolVm.Libraries = candidatePoolVm.Libraries.Where(library => library.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)).ToList();
						candidatePoolVm.SearchString = SearchString;
					}
					foreach (var library in candidatePoolVm.Libraries)
					{
						library.IsDeleteable = scenarioVm.Scenarios.Any(Scenario => Scenario.LibraryId == library.CandidatePoolId);
					}
					return View(candidatePoolVm);
				}
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public async Task<CandidatePoolViewModel> List(int? scenid, bool? importing)
		{
			var libraryVm = new CandidatePoolViewModel();
			var currentUser = await GetOrCreateUser();
			var scenariosVm = new ScenarioViewModel();
			ApplayUserFilter(currentUser.EntityId);
			scenariosVm.Scenarios = await _filter.ScenarioRepo.GetAllAsync();
			libraryVm.Libraries = (await _filter.CandidatePoolRepo.GetAllAsync())
			.OrderByDescending(lib => DateTime.TryParse(lib.CreatedAt, out var createdAt) ? createdAt : DateTime.MinValue)
			.ToList();
			foreach (var sce in scenariosVm.Scenarios)
			{
				foreach (var lib in libraryVm.Libraries)
				{
					if (lib.CandidatePoolId == sce.LibraryId)
					{
						lib.SceName = sce.ScenarioName;
					}
					if (lib.SceName == null)
					{
						lib.SceName = "";
					}
				}

			}

			ViewData["CurrentUserName"] = currentUser.Name;
			ApplayUserFilter(currentUser.EntityId);
			return libraryVm;
		}

		public async Task GetSource(CandidatePoolViewModel candidatePoolVm)
		{
			var sources = await _filter.CandidatePoolRepo.GetSource(candidatePoolVm.Libraries.Select(x => x.CandidatePoolId).ToList());

			foreach (var pool in candidatePoolVm.Libraries)
			{
				pool.Source = sources.FirstOrDefault(x => x.Key == pool.CandidatePoolId).Value;
			}
		}


		public IActionResult ImportB2P()
		{
			return View(new ImportMASAndB2PViewModel { IsMAS = false });
		}

		public IActionResult ImportMAS()
		{
			return View(new ImportMASAndB2PViewModel { IsMAS = true });
		}

		[HttpPost]
		public async Task<IActionResult> CreateLibrary(CandidatePoolViewModel libraryViewModel)
		{
			ValidateLibrary(libraryViewModel);
			if (!ModelState.IsValid)
				return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to create a library." });
			var currentUser = await GetOrCreateUser();
			await CreateNewLibrary(libraryViewModel, currentUser.EntityId);

            TempData["SuccessMessage"] = "Candidate Pool Created Successfully!";

            if (libraryViewModel.CreatScenpage == true)
			{
				return RedirectToAction("CreateScenario", "Scenarios");
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> CreateScenaLibrary([FromBody] CandidatePoolViewModel libraryViewModel)
		{
			ValidateLibrary(libraryViewModel);
			if (!ModelState.IsValid)
				return View("Error", new ErrorViewModel() { ErrorMessage = "the provided information are not valid to create a library." });
			var currentUser = await GetOrCreateUser();
			await CreateNewLibrary(libraryViewModel, currentUser.EntityId);
			return Json(Libid);
		}

		[HttpPost]
		public async Task<IActionResult> EditLibrary(CandidatePoolViewModel libraryViewModel)
		{
			try
			{
				var pairs = new Dictionary<string, object>
		{
			{ nameof(libraryViewModel.Name), libraryViewModel.Name },
			{ nameof(libraryViewModel.Description), libraryViewModel.Description },
			{ nameof(libraryViewModel.IsShared), libraryViewModel.IsShared },
		};
				await _filter.CandidatePoolRepo.UpdateAsync(libraryViewModel.Id, pairs);
				await _filter.CandidatePoolRepo.SaveChangesAsync();
				var libraryId = libraryViewModel.Id;
				return RedirectToAction(nameof(Index));
			}
			catch (Exception)
			{
				throw;
			}
		}



		[HttpPost]
		public async Task<IActionResult> CreateSharedLibrary(CandidatePoolViewModel libraryViewModel)
		{
			var currentUser = await GetOrCreateUser();
			await CreateNewLibrary(libraryViewModel, currentUser.EntityId, $"{libraryViewModel.Id}");
			return RedirectToAction(nameof(Index));
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteLibrary(CandidatePoolViewModel libraryViewModel)
		{
			var success = await _libraryUnitOfWork.DeactivateCandidatePool(libraryViewModel.Id.HasValue ? libraryViewModel.Id.Value : Guid.Empty);
			if (!success)
				return View("Error", new ErrorViewModel() { ErrorMessage = "Could not delete the selected library because it was not found." });
			return RedirectToAction(nameof(Index));
		}

		private void ApplayUserFilter(long userId)
		{
			_filter.CandidatePoolRepo.UserId = userId;
		}

		[HttpPost]
		public async Task<IActionResult> GetLibrary([FromBody] Guid? id)
		{
			var library = await _filter.CandidatePoolRepo.FindAsync(id);
			return Json(library);
		}

        [HttpPost]
        public async Task<IActionResult> GetLibraryWithScenario([FromBody] Guid? id)
        {
            var library = await _filter.CandidatePoolRepo.FindAsync(id);
           
            var AllScenarios = await _filterUnitOfWork.ScenarioRepo.GetAllAsync();
            var scenarios = AllScenarios.Find(a => a.LibraryId == id);


            var response = new
            {
                ScenarioName = scenarios?.ScenarioName ?? string.Empty,
                LibraryName = library.Name
            };

            return Json(response);

        }


        [HttpPost]
		public async Task<IActionResult> CheckLibraryName(string libraryname, string libraryid)
		{
			if (libraryid == null)
			{
				_filter.CandidatePoolRepo.ApplyFilter(new Dictionary<string, object> { { "Name", libraryname } });
				var result = await _filter.CandidatePoolRepo.GetAllAsync();
				if (!result.Any())
					return Json(Ok());
				else
					return BadRequest("name already exists");
			}
			else
			{
				if (Guid.TryParse(libraryid, out Guid id))
				{
					_filter.CandidatePoolRepo.ApplyFilter(new Dictionary<string, object> { { "Name", libraryname } });
					var resultPool = await _filter.CandidatePoolRepo.GetAllAsync();

					if (resultPool.Count() > 0)
					{
						foreach (var item in resultPool)
						{
							if (item.CandidatePoolId != id)
							{
								return BadRequest("name already exists 1");
							}
						}
					}
					else
					{
						return Json(Ok());
					}
				}
			}
			return Json(Ok());
		}
		[HttpPost]
		public async Task<IActionResult> ImportTreatments(ImportTreatmentViewModel data)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
				return BadRequest(Json(errors));
			}
			var excelfilepath = await data.ExcelFile.SaveUploadedFile();
			var source = "BAMS";
			switch (data.ImportType)
			{
				case "P":
					source = "PAMS";
					break;
				case "D":
					source = "DecisionSpace";
					break;
				default:
					source = "BAMS";
					break;
			}
			
			var errormessage = await _treatmentUnitOfWork.ImportTreatmentsFromExcelFile(source, data.ImportType, excelfilepath, data.TabName, data.LibraryId, data.fromScratch, data.keepAll);
			_treatmentUnitOfWork.DeleteExcelFile(excelfilepath);
			return string.IsNullOrEmpty(errormessage) ? Ok() : BadRequest(Json(errormessage));
		}

		[HttpPost]
		public async Task<IActionResult> ImportMASAndB2P(ImportMASAndB2PViewModel data)
		{
			if (!ModelState.IsValid)
				return BadRequest("an error occured while trying to upload file");
			var excelfilepath = await data.ExcelFile.SaveUploadedFile();
			var errormessage = await _treatmentUnitOfWork.ImportMASAndB2PFromExcelFile(data.IsMAS, excelfilepath, data.TabName);
			_treatmentUnitOfWork.DeleteExcelFile(excelfilepath);
			return string.IsNullOrEmpty(errormessage) ? data.IsMAS ? View("ImportMAS") : View("ImportB2P") : BadRequest(errormessage);
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
			Libid = id;
			if (id != Guid.Empty && !libraryViewModel.IsEmptyLibrary)
				await _libraryUnitOfWork.PopulateCandidatePool(id, libraraysource, string.IsNullOrEmpty(libraryViewModel.SelectedAsset) ? null : libraryViewModel.SelectedAsset == "Brdige" ? "B" : "P", libraryViewModel.SelectedDistrict,
														 libraryViewModel.SelectedCounty, libraryViewModel.SelectedRoute, libraryViewModel.MinYear, libraryViewModel.MaxYear);
		}
		private async Task<UserModel> GetOrCreateUser()
		{
			var currentUser = await _users.GetUserObjectIdAsync(User.GetObjectId());
			if (currentUser is not null)
				return currentUser;
			currentUser = User.ToUserModel();
			currentUser = await _users.InsertAndSave(currentUser);
			return currentUser;
		}
		private void ValidateLibrary(CandidatePoolViewModel libraryViewModel)
		{
			if (ModelState.IsValid)
				return;
			if (!ModelState.ContainsKey("assettype") || (ModelState.TryGetValue("assettype", out ModelStateEntry? modelState) && modelState?.ValidationState == ModelValidationState.Invalid))
				libraryViewModel.SelectedAsset = null;
			foreach (var key in ModelState.Keys)
			{
				if (key == "libraryname")
					continue;
				var modelstate = ModelState.GetValueOrDefault(key);
				if (modelstate is not null)
					modelstate.ValidationState = ModelValidationState.Skipped;
			}
		}
		private async Task ImportingLibrary(int? scenid, bool? isimporting, CandidatePoolViewModel candidatepoolViewModel)
		{
			if (scenid.HasValue)
			{
				var scenraio = await _filter.ScenarioRepo.FindAsync(scenid.Value);
				candidatepoolViewModel.IsImporting = isimporting.HasValue ? isimporting.Value : false;
				candidatepoolViewModel.FromScenario = !candidatepoolViewModel.IsImporting;
				if (scenraio is not null)
				{
					var candidatepool = candidatepoolViewModel.Libraries.FirstOrDefault(l => l.CandidatePoolId == scenraio.LibraryId);
					if (candidatepool != null)
					{
						candidatepoolViewModel.Id = candidatepool.CandidatePoolId;
						candidatepoolViewModel.Name = candidatepool.Name;
					}
				}
			}
		}
	}
}
