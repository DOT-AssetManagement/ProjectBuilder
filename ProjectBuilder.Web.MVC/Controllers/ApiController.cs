using GisJsonHandler;
using log4net.Filter;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.LibraryModel;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Services;
using ProjectBuilder.Web.MVC;
using ProjectBuilder.Web.MVC.Controllers;
using ProjectBuilder.Web.MVC.Models;
using System.Text.Json;

namespace ProjectBuilder.API.Controllers;

[ApiController]
[Route("api/")]
public class ApiController : LibraryBaseController
{

    private readonly IUserRepository _userRepository;
    private readonly ITreatmentUnitOfWork _treatmentUnitOfWork;
    private readonly IFilterUnitOfWork _filterUnitOfWork;
    private readonly ITreatmentRepository _treatments;
    private readonly ProjectBuilderDbContext _context;
    private readonly IRunScenarioUnitOfWork _runScenarioUnitOfWork;

    public ApiController(IUserRepository userRepository, IFilterUnitOfWork filterUnitOfWork,
        IRepository<UserRoleModel> userRoleRepository,
        ITreatmentRepository treatments, ProjectBuilderDbContext context,
        IUserRepository users, ITreatmentUnitOfWork treatmentUnitOfWork, IRunScenarioUnitOfWork runScenarioUnitOfWork) : base(users, filterUnitOfWork, userRoleRepository)
    {
        _userRepository = userRepository;
        _filterUnitOfWork = filterUnitOfWork;
        _treatmentUnitOfWork = treatmentUnitOfWork;
        _treatments = treatments;
        _context = context;
        _runScenarioUnitOfWork = runScenarioUnitOfWork;
    }

    [HttpGet("GetUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
        var userDtos = users
        .Where(user => user.IsMapActive)
        .Select(user => new
        {
            Email = user.Email,
            UserId = user.EntityId,
            Name = user.Name
        })
        .ToList();

        return Ok(userDtos);
    }

    [HttpGet("GetScenarios")]
    public async Task<IActionResult> GetScenarios(long UserId)
    {
        var candidatePoolVm = new CandidatePoolViewModel();
        var scenarioVm = new ScenarioViewModel();

        _filterUnitOfWork.CandidatePoolRepo.UserId = UserId;
        candidatePoolVm.Libraries = await _filterUnitOfWork.CandidatePoolRepo.GetAllAsync();
        candidatePoolVm.Libraries = candidatePoolVm.Libraries.Where(a => a.UserId == UserId && a.IsShared);

        scenarioVm.Scenarios = await _filterUnitOfWork.ScenarioRepo.GetAllAsync();

        var scenarios = new List<object>();

        foreach (var library in candidatePoolVm.Libraries)
        {
            scenarios.AddRange(
                scenarioVm.Scenarios
                    .Where(a => a.LibraryId == library.CandidatePoolId)
                    .Select(a => new
                    {
                        a.ScenarioId,
                        a.ScenarioName
                    }) 
            );

        }

        return Ok(scenarios);

    }

    [HttpPost("CreateTreatment")]
    public async Task<IActionResult> CreateTreatment()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var inputJson = await reader.ReadToEndAsync();

            var treatmentViewModel = JsonConvert.DeserializeObject<TreatmentViewModel>(inputJson);

            if (treatmentViewModel == null)
            {
                return BadRequest(new { success = false, message = "Invalid JSON format" });
            }

            var newTreatment = UpdateOrCreateTreatment(treatmentViewModel);
            _ = Guid.TryParse(HttpContext.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryid);
            newTreatment.LibraryId = libraryid;
            var treatmentId = await _treatmentUnitOfWork.CreateUserTreatment(newTreatment);

            return Ok(new { success = true, message = "Data received successfully", data = treatmentViewModel });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while processing the request." });
        }
    }

    public static TreatmentViewModel ConvertToViewModel(GisJsonHandler.Treatment treatment)
    {
        return new TreatmentViewModel
        {
            AssetType = "Bridge", // Example static value
            Treatment = treatment.AppliedTreatment,
            IsCommitted = false, // Example default value
            District = (byte?)treatment.District,
            PreferredYear = treatment.Year,
            County = (byte?)treatment.Cnty, // Adjust based on logic
            Route = (int?)treatment.Route,
            Benefit = treatment.Benefit,
            Cost = treatment.Cost,
            BridgeId = treatment.BRIDGE_ID,
            Brkey = treatment.BRKEY,
            ProjectId = treatment.ProjectId.ToString(),
            TreatmentId = Guid.NewGuid() // Example for generating new GUID
        };
    }


    private async Task<(bool Success, string Message, GisOutput Gis)> ProcessGisPayload()
    {
        using var reader = new StreamReader(Request.Body);
        var inputJson = await reader.ReadToEndAsync();

        GisOutput gis;
        try
        {
            gis = JsonConvert.DeserializeObject<GisOutput>(inputJson);
        }
        catch (Exception ex)
        {
            return (false, "Invalid JSON: " + ex.Message, null);
        }

        if (gis?.Treatments == null)
            return (true, "No treatments to process.", gis); // still a success, just no work

        foreach (var treatment in gis.Treatments)
        {
            var updatedTreatment = new UserTreatmentModel
            {
                ImportTimeGeneratedId = treatment.ImportTimeGeneratedId,
                Cost = treatment.Cost,
                Benefit = treatment.Benefit,
                PreferredYear = treatment.PreferredYear,
                MinYear = treatment.MinYear,
                MaxYear = treatment.MaxYear,
                PriorityOrder = (byte?)treatment.PriorityOrder,
                Risk = treatment.Risk,
                IndirectCostDesign = treatment.IndirectCostDesign,
                IndirectCostOther = treatment.IndirectCostOther,
                IndirectCostRow = treatment.IndirectCostROW,
                IndirectCostUtilities = treatment.IndirectCostUtilities,
                IsCommitted = treatment.IsCommitted,
                LibraryId = gis.ScenHeader.LibraryId
            };

            await FilterUnitOfWork.TreatmentRepo.EditTreatments(updatedTreatment);

            FilterUnitOfWork.CargoAttributesRepo.ApplyFilter(new Dictionary<string, object>
        {
            { "AttributeName", "PROJECTSOURCEID" },
            { "AssetType", treatment.TreatmentType.ToString().FirstOrDefault() }
        });

            var cargoAttributesList = await FilterUnitOfWork.CargoAttributesRepo.GetAllAsync();
            if (cargoAttributesList == null || !cargoAttributesList.Any())
            {
                return (false, "No matching CargoAttributes records found.", null);
            }

            foreach (var cargoAttribute in cargoAttributesList)
            {
                var attributeNo = cargoAttribute.AttributeNo;
                await FilterUnitOfWork.TreatmentRepo.UpdateMPMSID(treatment.ImportTimeGeneratedId, attributeNo, treatment.MPMSID);
            }
        }

        if (gis?.Projects != null)
        {
            foreach (var project in gis.Projects)
            {
                await FilterUnitOfWork.ProjectRepo.UpdateUserIdAndNotes(
                    gis.ScenHeader.ScenId,
                    project.ProjectId,
                    project.UserId,
                    project.UserNotes
                );
            }
        }

        return (true, "Success", gis);
    }


    [HttpPost("UpdateScenario")]
    public async Task<IActionResult> UpdateScenario()
    {
        try
        {
            var (success, message, gis) = await ProcessGisPayload();
            if (!success)
                return StatusCode(500, new { success = false, message });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        }
    }

    [HttpPost("RunScenario")]
    public async Task<IActionResult> RunScenario()
    {
        try
        {
            var (success, message, gis) = await ProcessGisPayload();
            if (!success)
                return StatusCode(500, new { success = false, message });

            if (gis?.ScenHeader != null)
            {
                var scenarioEventArgs = _runScenarioUnitOfWork.RunScenario(
                    gis.ScenHeader.ScenId,
                    gis.ScenHeader.LastRunBy,
                    true
                );
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        }
    }




    [HttpGet("GetMapJSON")]
    public async Task<IActionResult> GetMapJSON([FromQuery] int ScenID)
    {
        var mapresult = await _runScenarioUnitOfWork.ExportScenarioResultsToJson(ScenID, false, null, null, null, null, null);

        if (string.IsNullOrWhiteSpace(mapresult.Result))
        {
            return BadRequest(new { error = "No data available for the given scenario ID." });
        }

        return Content(mapresult.Result, "application/json");
    }


    [HttpGet("HasScenarioRunCompleted")]
    public async Task<IActionResult> HasScenarioRunCompleted([FromQuery] int ScenID)
    {

        var scenario = await _filterUnitOfWork.ScenarioRepo.FindAsync(ScenID);

        return Ok(new { success = true, notes = scenario.Notes });
    }



    private UserTreatmentModel UpdateOrCreateTreatment(TreatmentViewModel treatmentViewModel)
    {
        var sections = treatmentViewModel.Section.DecomposeString<int>('-');
        var result = new UserTreatmentModel
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
            Direction = (byte?)(treatmentViewModel.Direction ? 1 : 0)
        };
        return result;
    }


}
