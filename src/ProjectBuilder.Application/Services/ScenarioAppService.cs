using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.Caching;
using OfficeOpenXml;
using ProjectBuilder.Entities;
using ProjectBuilder.Helpers;
using ProjectBuilder.Helpers.Exceptions;
using ProjectBuilder.Helpers.Logger;
using ProjectBuilder.Models;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Counties;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.ParameterEntity.Dtos;
using ProjectBuilder.Models.ReviseBudgetConstraints;
using ProjectBuilder.Models.Scenario;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Models.ScenarioBudget.Dtos;
using ProjectBuilder.Models.ScenarioBudgetConstraint.Dtos;
using ProjectBuilder.Models.ScenarioBudgetFlat.Dtos;
using ProjectBuilder.Models.ScenarioFilter.Dtos;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using Microsoft.AspNetCore.Authorization;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using ProjectBuilder.ScenarioBudgetConstraints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using static Volo.Abp.Identity.IdentityPermissions;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace ProjectBuilder.Services
{
    [Authorize(ScenarioPermissions.Default)]
    public class ScenarioAppService : ApplicationService, IScenarioAppService
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICandidatePoolRepository _candidateRepository;
        private readonly IRunScenarioRepository _scenarioRepository;
        private readonly IProjectTreatmentRepository _projectTreatment;
        private readonly IScenarioParametersRepository _scenarioParametersRepository;
        private readonly IScenarioBudgetFlattenedRepository _scenarioBudgetFlattenedRepository;
        private readonly IScenarioBudgetFlatRepository _scenarioBudgetFlatRepository;
        private readonly IParametersRepository _parametersRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IAnalysisMethodRepository _analysisMethodRepository;
        private readonly IBudgetConstraintsRepository _budgetConstraintsRepository;
        private readonly IdentityRoleManager _identityRoleManager;
        private readonly IdentityUserManager _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ProjectBuilderMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAppLogger<ScenarioAppService> _logger;
        private readonly IPermissionChecker _permissionChecker;
        public ScenarioAppService(ICurrentUser currentUser, ICandidatePoolRepository candidateRepository, IRunScenarioRepository scenarioRepository, IProjectTreatmentRepository projectTreatment, IdentityRoleManager identityRoleAppService, IdentityUserManager userManager, ProjectBuilderMapper mapper, IScenarioParametersRepository scenarioParametersRepository, IScenarioBudgetFlattenedRepository scenarioBudgetFlattenedRepository, IScenarioBudgetFlatRepository scenarioBudgetFlatRepository, IParametersRepository parametersRepository, ITreatmentRepository treatmentRepository, IAnalysisMethodRepository analysisMethodRepository, IBudgetConstraintsRepository budgetConstraintsRepository, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, IAppLogger<ScenarioAppService> logger, IPermissionChecker permissionChecker)
        {
            _currentUser = currentUser;
            _candidateRepository = candidateRepository;
            _scenarioRepository = scenarioRepository;
            _projectTreatment = projectTreatment;
            _identityRoleManager = identityRoleAppService;
            _userManager = userManager;
            _mapper = mapper;
            _scenarioParametersRepository = scenarioParametersRepository;
            _scenarioBudgetFlattenedRepository = scenarioBudgetFlattenedRepository;
            _scenarioBudgetFlatRepository = scenarioBudgetFlatRepository;
            _parametersRepository = parametersRepository;
            _treatmentRepository = treatmentRepository;
            _analysisMethodRepository = analysisMethodRepository;
            _budgetConstraintsRepository = budgetConstraintsRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _permissionChecker = permissionChecker;
        }

        [HttpGet("Get-all-scenarios")]
        public async Task<ScenarioDto> GetScenariosAsync(string searchString = "")
        {
            var user = await GetOrCreateUser();
            if (user == null)
                throw new AppException(ExceptionCodes.NotFound,
                    "User not found.",
                    "Could not find or create the current user.");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null || userRoles.Count == 0)
                throw new AppException(ExceptionCodes.Forbidden,
                    "No role assigned.",
                    $"User '{user.Id}' has no role assigned. Contact your administrator.");

            _logger.Info("[SCENARIOS] Role found | Role: {Role} | UserId: {UserId}",
                userRoles.First(), user.Id);

            ApplayUserFilter(user.Id);
            var candidatePools = await _candidateRepository.GetListAsync();
            if (candidatePools == null)
                throw new AppException(ExceptionCodes.NotFound,
                    "No libraries found.",
                    $"No candidate pools available for user '{user.Id}'.");

            var userLibrariesCount = candidatePools.Count(pool => pool.UserId == user.Id);
            var sharedLibrariesCount = candidatePools.Count(pool => pool.IsShared);
            var totalLibrariesCount = userLibrariesCount + sharedLibrariesCount;

            _logger.Info("[SCENARIOS] Libraries | User: {UserCount} | Shared: {SharedCount} | Total: {Total}",
                userLibrariesCount, sharedLibrariesCount, totalLibrariesCount);

            var scenarios = await _scenarioRepository.GetScenariosListAsync();
            if (scenarios == null)
                throw new AppException(ExceptionCodes.NotFound,
                    "No scenarios found.",
                    $"No scenarios exist for user '{user.Email}'.");

            var canViewAllScenarios = await _permissionChecker.IsGrantedAsync(AdministrationPermissions.Default);
            // Legacy visibility rules: admins see every scenario; regular users see
            // their own scenarios plus any scenario marked as shared.
            var scenarioDtos = _mapper.Map(scenarios)
                .Where(scenario => canViewAllScenarios
                    || scenario.IsShared
                    || string.Equals(scenario.CreatedBy, user.Email, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var scenario in scenarioDtos)
            {
                // A scenario may have no library, or its library may have been deleted.
                // Match the MVC behaviour and tolerate this instead of throwing.
                var library = scenario.LibraryId.HasValue
                    ? await _candidateRepository.FirstOrDefaultAsync(x => x.Id == scenario.LibraryId.Value)
                    : null;

                scenario.CandidatePool = library?.Name ?? string.Empty;

                var isStale = await _projectTreatment.CheckStale(scenario.Id);
                if (isStale == true)
                {
                    scenario.Notes = "Scenario treatments have been modified. Please perform a scenario re-run.";
                    scenario.ReRun = true;
                }
            }

            await PopulateScenarioCreatorDisplayAsync(scenarioDtos);

            if (!string.IsNullOrEmpty(searchString))
            {
                // No matches simply yields an empty table (as in the MVC version),
                // rather than surfacing an error to the user.
                scenarioDtos = scenarioDtos
                    .Where(s => s.ScenarioName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Show the most recently created scenarios first.
            scenarioDtos = scenarioDtos
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return new ScenarioDto
            {
                Scenarios = scenarioDtos,
                SearchString = searchString,
                LibrariesCount = totalLibrariesCount
            };
        }

        // Resolves each scenario's CreatedBy (an email) to a display name, caching
        // by email so scenarios sharing a creator only look the user up once.
        private async Task PopulateScenarioCreatorDisplayAsync(IEnumerable<ScenarioDto> scenarios)
        {
            var creatorByEmail = new Dictionary<string, Volo.Abp.Identity.IdentityUser>(StringComparer.OrdinalIgnoreCase);

            foreach (var scenario in scenarios)
            {
                var createdByEmail = scenario.CreatedBy?.Trim();
                if (string.IsNullOrWhiteSpace(createdByEmail))
                {
                    scenario.CreatorName = string.Empty;
                    scenario.CreatorEmail = string.Empty;
                    continue;
                }

                if (!creatorByEmail.TryGetValue(createdByEmail, out var creator))
                {
                    creator = await _userManager.FindByEmailAsync(createdByEmail);
                    creatorByEmail[createdByEmail] = creator;
                }

                scenario.CreatorName = creator?.Name ?? string.Empty;
                scenario.CreatorEmail = createdByEmail;
            }
        }

        public async Task<ScenarioDto> GetScenariosAsync()
        {
            var user = await GetOrCreateUser();

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null || userRoles.Count == 0)
            {
                throw new InvalidOperationException("No role is assigned to the user.");
            }


            ApplayUserFilter(user.Id);
            var candidatePools = await _candidateRepository.GetListAsync();

            var scenarios = await _scenarioRepository.GetListAsync();

            var scenarioDtos = _mapper.Map(scenarios);

            var canViewAllScenarios = await _permissionChecker.IsGrantedAsync(AdministrationPermissions.Default);
            var scenariosVm = new ScenarioDto
            {
                Scenarios = scenarioDtos.Where(a => canViewAllScenarios || a.CreatedBy == user.Email).ToList()
            };

            return scenariosVm;
        }
        public async Task<CreateScenarioDto> CreateScenarioAsync(string id)
        {
            var candidatePools = await LoadCandidatePoolsForUser();

            return new CreateScenarioDto
            {
                CandidatePools = candidatePools
            };
        }
        public async Task<List<CandidatePoolDto>> LoadCandidatePoolsForUser()
        {
            var user = await GetOrCreateUser();
            return await GetCurrentUserCandidatePools(user.Id);

        }
        public async Task<List<CandidatePoolDto>> GetCurrentUserCandidatePools(Guid userId)
        {
            var candidatePools = await _candidateRepository.GetListAsync(cp => cp.UserId == userId && cp.IsActive == true);
            return _mapper.Map(candidatePools);
        }
        public async Task<ScenarioDto> GetScenarioByPoolId(Guid poolId)
        {
            var Scenario = await _scenarioRepository.GetScenarioByPoolId(poolId);
            if (Scenario == null)
            {
                return null;
            }
            return _mapper.Map(Scenario);
        }

        public async Task UpdateScenarioByPoolIdAsync(Guid poolId, string name, string description)
        {
            await _scenarioRepository.UpdateScenarioByPoolIdAsync(poolId, name, description);
        }
        public async Task<ScenarioDto> CreateNewScenarioAsync(CreateScenarioDto scenarioDto)
        {
            if (scenarioDto == null)
                throw new UserFriendlyException("Scenario request is invalid.");

            var yearValidationErrors = GetScenarioYearValidationErrors(scenarioDto.FirstYear, scenarioDto.LastYear);
            if (yearValidationErrors.Any())
                throw new UserFriendlyException(string.Join(" ", yearValidationErrors));

            if (string.IsNullOrWhiteSpace(scenarioDto.ScenarioName))
                throw new UserFriendlyException("Scenario name is required.");

            scenarioDto.ScenarioName = scenarioDto.ScenarioName.Trim();

            if (!scenarioDto.LibraryId.HasValue || scenarioDto.LibraryId == Guid.Empty)
                throw new UserFriendlyException("A treatment pool library must be selected.");

            var currentUser = await GetOrCreateUser();
            scenarioDto.CreatedBy = currentUser.Email;

            var hasExistingScenarioLibrary = !string.IsNullOrWhiteSpace(scenarioDto.SelectScen)
                && !string.Equals(scenarioDto.SelectScen, "null", StringComparison.OrdinalIgnoreCase);

            Guid libraryId;

            if (!hasExistingScenarioLibrary)
            {
                libraryId = scenarioDto.LibraryId.Value;
            }
            else
            {
                if (scenarioDto.newLibrarySend == null || !scenarioDto.newLibrarySend.Any())
                    throw new UserFriendlyException("Selected treatment pool is already linked to a scenario. Create a new pool copy to continue.");

                libraryId = await CreateScenLib(scenarioDto);
            }

            var scenarioArgs = await _scenarioRepository.CreateScenarioAsync(
                scenarioDto.ScenarioName,
                libraryId,
                scenarioDto.FirstYear!.Value,
                scenarioDto.LastYear!.Value,
                1,
                scenarioDto.CreatedBy,
                scenarioDto.IsScenarioShared);

            if (scenarioArgs.Level == LogLevel.Error)
                throw new UserFriendlyException(string.IsNullOrWhiteSpace(scenarioArgs.ErrorMessage)
                    ? "Unexpected error occurred while creating the scenario."
                    : scenarioArgs.ErrorMessage);

            if (scenarioArgs.ScenarioId <= 0)
                throw new UserFriendlyException("Scenario could not be created. Please verify the database stored procedure dbo.sp_pb_CreateNewScenario.");

            scenarioDto.ScenarioId = scenarioArgs.ScenarioId;
            scenarioDto.LibraryId = libraryId;

            return new ScenarioDto
            {
                Id = scenarioDto.ScenarioId,
                LibraryId = scenarioDto.LibraryId,
                ScenarioName = scenarioDto.ScenarioName,
                CreatedBy = scenarioDto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                LastRunBy = scenarioDto.LastRunBy,
                LastRunAt = scenarioDto.LastRunAt,
                LastStarted = scenarioDto.LastStarted,
                LastFinished = scenarioDto.LastFinished,
                Locked = scenarioDto.Locked,
                Notes = scenarioDto.Notes,
                Stale = scenarioDto.Stale,
                CandidatePool = string.Empty,
                LibrariesCount = 1
            };
        }

        private const int MinimumScenarioFirstYear = 2021;

        private static List<string> GetScenarioYearValidationErrors(int? firstYear, int? lastYear)
        {
            var validationErrors = new List<string>();

            if (!firstYear.HasValue)
                validationErrors.Add("First year is required.");
            else if (firstYear.Value < MinimumScenarioFirstYear)
                validationErrors.Add($"First year must be {MinimumScenarioFirstYear} or later.");

            if (!lastYear.HasValue)
                validationErrors.Add("Last year is required.");

            if (firstYear.HasValue && lastYear.HasValue && firstYear.Value > lastYear.Value)
                validationErrors.Add("Last year must be greater than or equal to first year.");

            return validationErrors;
        }
        public async Task<Guid> CreateScenLib(CreateScenarioDto scenarioViewModel)
        {
            if (!await _permissionChecker.IsGrantedAsync(TreatmentPoolLibraryPermissions.Create))
                throw new UserFriendlyException("You do not have permission to create treatment pool libraries.");

            if (scenarioViewModel.newLibrarySend == null || !scenarioViewModel.newLibrarySend.Any())
                throw new UserFriendlyException("A new treatment pool payload is required to copy an existing scenario library.");

            var currentUser = await GetOrCreateUser();
            var target = await _candidateRepository.FindAsync(cp => cp.Id == scenarioViewModel.LibraryId);

            var newLibrary = new CandidatePool();
            foreach (var item in scenarioViewModel.newLibrarySend)
            {
                newLibrary.UserId = currentUser.Id;
                newLibrary.Name = item.NewLibName;
                newLibrary.Description = item.librarydescription;
                newLibrary.IsActive = true;
                newLibrary.IsShared = item.IsShared;
            }

            var libid = await _candidateRepository.CreateNewCandidatePool(newLibrary);
            foreach (var item in scenarioViewModel.newLibrarySend)
            {
                if (libid != Guid.Empty && !item.IsEmptyLibrary)
                    await _candidateRepository.PopulateCandidatePool(libid, Convert.ToString(scenarioViewModel.LibraryId), string.IsNullOrEmpty(item.SelectedAsset) ? null : item.SelectedAsset == "Brdige" ? "B" : "P", item.SelectedDistrict,
                                                             item.SelectedCounty, item.SelectedRoute, item.LibMinYear, item.LibMaxYear);
            }

            return libid;
        }
        public async Task<List<ScenarioParameterDto>> CreateScenarioParameter(CreateScenarioParameterDto scenarioViewModel)
        {
            var scenarioParameters = new List<ScenarioParamater>();

            foreach (var item in scenarioViewModel.parameters)
            {
                if (item.Id.ToLower().Contains("yfst") || item.Id.ToLower().Contains("ylst"))
                    continue;

                var parameterEntity = new ScenarioParamater
                {
                    ScenarioId = scenarioViewModel.ScenarioId,
                    ParameterValue = scenarioViewModel.ParameterValue,
                    Id = item.Id
                };

                scenarioParameters.Add(parameterEntity);
            }

            if (scenarioParameters.Any())
            {
                await _scenarioParametersRepository.InsertManyAsync(scenarioParameters);
            }
            var response = _mapper.Map(scenarioParameters);

            return response;
        }
        public async Task<ScenarioBudgetFlatDto> CreateScenarioBudget(CreateScenarioBudgetFlatDto scenarioViewModel)
        {
            //  _runScenarioUnit.ScenariosBudgetsRepo.ApplyFilter(new Dictionary<string, object> { { "ScenarioId", scenarioViewModel.ScenarioId } });
            var scenarioBudgets = await _scenarioBudgetFlattenedRepository.GetListAsync(sb => sb.ScenarioId == scenarioViewModel.ScenarioId);

            var checkExistingScenarioBudgets = await _scenarioBudgetFlattenedRepository.GetAllAsync();

            if (checkExistingScenarioBudgets != null && checkExistingScenarioBudgets.Count() > 0)
            {
                throw new UserFriendlyException("A budget for this scenario already exists.");
            }

            var scenBudDto = new ScenarioBudgetFlatDto();
            scenBudDto.ScenarioId = scenarioViewModel.ScenarioId;

            foreach (var item in scenarioViewModel.budgetConstraints)
            {
                scenBudDto.YearWork = item.YearofWork;
                scenBudDto.District = item.District;
                scenBudDto.BridgeInterstateBudget = (item.bridgeInterstateBudget * 1000000);
                scenBudDto.BridgeNonInterstateBudget = (item.bridgeNonInterstateBudget * 1000000);
                scenBudDto.PavementInterstateBudget = (item.pavementNonInterstateBudget * 1000000);
                scenBudDto.PavementNonInterstateBudget = (item.pavementNonInterstateBudget * 1000000);
                var scenBudEntity = ObjectMapper.Map<ScenarioBudgetFlatDto, ScenarioBudgetFlat>(scenBudDto);
                await _scenarioBudgetFlatRepository.InsertAsync(scenBudEntity);
            }

            return scenBudDto;
        }
        private void ApplayUserFilter(Guid userId)
        {
            _candidateRepository.UserId = userId;
        }
        public async Task<Volo.Abp.Identity.IdentityUser> GetOrCreateUser()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal == null || claimsPrincipal.Identity == null || !claimsPrincipal.Identity.IsAuthenticated)
                throw new BusinessException("User is not authenticated.");

            // Resolve the current ABP identity user (AbpUsers) by id, falling back to email.
            Volo.Abp.Identity.IdentityUser user = null;
            if (_currentUser.Id.HasValue)
            {
                user = await _userManager.FindByIdAsync(_currentUser.Id.Value.ToString());
            }

            if (user == null)
            {
                var email = _currentUser.Email ?? claimsPrincipal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    user = await _userManager.FindByEmailAsync(email);
                }
            }

            if (user == null)
                throw new BusinessException("Authenticated user was not found in the identity store.");

            return user;
        }
        public async Task<bool> DeleteScenarioAsync(int scenarioId)
        {
            var result = await _scenarioRepository.DeleteScenarioAsync(scenarioId);
            return result.Level != LogLevel.Error;
        }
        public async Task<string> RunScenarioAsync(int? scenarioId)
        {
            if (!scenarioId.HasValue)
            {
                return "Could not get the scenario ID. Make sure that you have selected a scenario and try again.";
            }

            var user = await GetOrCreateUser();
            // Match "Creator" (CreatedBy == user.Email) so both identity columns show the same format.
            var scenarioEventArgs = await _scenarioRepository.RunScenarioAsync(scenarioId.Value, user.Email, true);

            return scenarioEventArgs.Level == LogLevel.Error
                ? scenarioEventArgs.ErrorMessage
                : "Scenario executed successfully.";
        }
        public async Task<bool> CheckScenarioDeletionAsync(int? scenId)
        {
            if (!scenId.HasValue)
            {
                return false;
            }

            var allScenarios = await _scenarioRepository.GetListAsync();
            var scenario = allScenarios.Find(a => a.Id == scenId);

            return scenario?.LibraryId.HasValue ?? false;
        }
        public async Task<bool> CheckScenarioNameAsync(string attemptedName)
        {
            var scenarios = await _scenarioRepository.GetScenariosListAsync();

            if (scenarios.Any(s => s.ScenarioName == attemptedName))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public async Task<List<ParameterDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var parameter = await _parametersRepository.GetListAsync();
            var displayResult = parameter.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).Select(p => new ParameterDto
                {
                    Id = p.Id,
                    Parmfamily = p.Parmfamily,
                    ParmName = p.ParmName,
                    ParmDescription = p.ParmDescription,

                }).ToList();

            return displayResult;
        }
        public async Task<List<ScenarioBudgetFlatDto>> PBudgetConstraints(int? scenid)
        {
            string scenarioName = "";
            if (scenid.HasValue)
            {
                var scenario = await _scenarioRepository.FindScenarioByIdAsync(scenid);
                _scenarioRepository.ScenarioId = scenid.Value;
                scenarioName = scenario is null ? "" : scenario.ScenarioName;
            }

            var scenarioBudgets = await _scenarioBudgetFlattenedRepository.GetAllAsync();

            var mapped = _mapper.Map(scenarioBudgets);
            var uniqueScenarioBudgets = mapped
                .GroupBy(sb => sb.YearWork)
                .Select(group => group.First())
                .ToList();

            return uniqueScenarioBudgets;
        }
        public async Task<List<ReviseBudgetConstraintsDto>> ReviseBudgetConstarints(int? firstYear, int? lastYear)
        {
            var counties = await _treatmentRepository.GetAllCounties();
            var districts = counties.Select(a => a.District).OrderBy(d => d).Distinct().ToList();
            var model = new List<ReviseBudgetConstraintsDto>();
            int x = 1;

            foreach (var district in districts)
            {
                for (int year = firstYear.GetValueOrDefault(); year <= lastYear.GetValueOrDefault(); year++)
                {
                    var item = new ReviseBudgetConstraintsDto();
                    item.District = district;
                    item.YearOfWork = year;
                    item.BridgeInterstate = 1000000;
                    item.BridgeNonInterstate = 1000000;
                    item.PavementInterstate = 1000000;
                    item.PavementNonInterstate = 1000000;
                    model.Add(item);
                }
            }
            return model;
        }
        public async Task<ScenarioBudgetConstraintsDto> BudgetConstraints(int? scenid)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.Roles.IsNullOrEmpty())
            {
                throw new BusinessException("Unauthorized access.");
            }
            {
                string scenarioName = "";
                if (scenid.HasValue)
                {
                    var scenario = await _scenarioRepository.FindScenarioByIdAsync(scenid);
                    var scenarioDto = _mapper.Map(scenario);
                    _scenarioRepository.ScenarioId = scenid.Value;
                    scenarioName = scenario is null ? "" : scenarioDto.ScenarioFullName;
                }
                var scenarioBudgets = await _scenarioBudgetFlattenedRepository.GetAllAsync();
                var scenarioBudgetDtos = _mapper.Map(scenarioBudgets);

                return new ScenarioBudgetConstraintsDto
                {
                    BudgetConstraints = scenarioBudgetDtos,
                    SelectedScenario = scenarioName,
                    ScenarioId = scenid
                };
            }
        }
        public async Task<ScenarioParameterDto> UpdateScenarioParameter(int? scenid, UpdateScenarioParameterDto scenparam)
        {
            var target = await _scenarioParametersRepository.FindScenarioParameter(scenid, scenparam.Id);
            if (target is null)
            {
                throw new UserFriendlyException("Unexpected error occurred");
            }
            target.ParameterValue = scenparam.ParameterValue;
            await _scenarioParametersRepository.UpdateAsync(target, nameof(target.ParameterValue));
            var targetDto = _mapper.Map(target);

            return targetDto;

        }
        public async Task<ScenarioBudgetFlatDto> UpdateBudgetConstraints(int? scenid, UpdateScenarioBudgetFlatDto scenarioBudget)
        {
            if (!scenid.HasValue)
            {
                throw new UserFriendlyException("Unexpected error occurred");
            }

            if (scenarioBudget == null)
            {
                throw new BusinessException("Budget constraints information is missing. Please fill all required fields.");
            }

            var result = _mapper.Map(scenarioBudget);
            result.ScenarioId = scenid.Value;

            await _scenarioBudgetFlatRepository.UpdateFieldsAsync(result,
                nameof(result.BridgeInterstateBudget),
                nameof(result.BridgeNonInterstateBudget),
                nameof(result.PavementInterstateBudget),
                nameof(result.PavementNonInterstateBudget)
            );

            var updatedEntity = await _scenarioBudgetFlatRepository.GetAsync(x => x.ScenarioId == scenid.Value);

            return _mapper.Map(updatedEntity);
        }
        public async Task<bool> DeleteScenarioParameterAsync(int scenid, string scenparamid)
        {
            if (scenid <= 0 || string.IsNullOrEmpty(scenparamid))
            {
                throw new BusinessException("Invalid scenario or parameter ID. Make sure you have selected a scenario.");
            }

            var success = await _scenarioParametersRepository.DeleteScenarioParameterAsync(scenid, scenparamid);

            if (!success)
            {
                throw new BusinessException($"Couldn't delete the scenario parameter with ID {scenparamid}. Please try again.");
            }

            return success;
        }
        public async Task<int> DeleteScenarioBudgetAsync(int? scenid, int? yearofwork, int? district)
        {
            if (scenid == null || yearofwork == null || district == null)
            {
                throw new BusinessException("Invalid scenario budget details. Make sure you have selected a scenario.");
            }

            var result = new ScenarioBudgetFlatDto()
            {
                ScenarioId = scenid.Value,
                YearWork = yearofwork.Value,
                District = district.Value
            };

            var mappedData = _mapper.Map(result);
            var rowsAffected = await _scenarioBudgetFlattenedRepository.DeleteAsync(mappedData);

            if (rowsAffected == 0)
            {
                throw new BusinessException("Couldn't delete the selected scenario budget. Please try again.");
            }

            return rowsAffected;
        }

        public async Task<List<ScenarioDto>> GetAllAsync(string searchString = "")
        {
            var result = await GetScenariosAsync(searchString);
            return result?.Scenarios?.ToList() ?? new List<ScenarioDto>();
        }

        private const string ScenarioSettingsModifiedNote =
            "Scenario treatments have been modified. To apply the changes, please perform a scenario re-run.";

        public async Task<ScenarioSettingsDto> GetScenarioSettingsAsync(int scenid)
        {
            var scenario = await _scenarioRepository.FindScenarioByIdAsync(scenid);
            if (scenario == null)
                throw new UserFriendlyException("Scenario not found.");

            var firstYear = await _scenarioParametersRepository.FindScenarioParameter(scenid, "YFST");
            var lastYear = await _scenarioParametersRepository.FindScenarioParameter(scenid, "YLST");

            return new ScenarioSettingsDto
            {
                ScenarioId = scenid,
                ScenarioName = scenario.ScenarioName ?? string.Empty,
                ScenarioNotes = scenario.Notes ?? string.Empty,
                Locked = scenario.Locked ?? false,
                Stale = scenario.Stale,
                IsShared = scenario.IsShared,
                FirstYear = firstYear?.ParameterValue.HasValue == true ? Convert.ToInt32(firstYear.ParameterValue.Value) : (int?)null,
                LastYear = lastYear?.ParameterValue.HasValue == true ? Convert.ToInt32(lastYear.ParameterValue.Value) : (int?)null
            };
        }

        public async Task UpdateScenarioSettingsAsync(int scenid, ScenarioSettingsDto input)
        {
            if (scenid <= 0)
                throw new UserFriendlyException("Scenario ID not provided. Please select a scenario before updating settings.");

            if (input == null)
                throw new UserFriendlyException("Scenario settings are missing.");

            var scenario = await _scenarioRepository.FindScenarioByIdAsync(scenid);
            if (scenario == null)
                throw new UserFriendlyException("Scenario not found.");

            if (string.IsNullOrWhiteSpace(input.ScenarioName))
                throw new UserFriendlyException("Scenario name is required.");

            var yearValidationErrors = GetScenarioYearValidationErrors(input.FirstYear, input.LastYear);
            if (yearValidationErrors.Any())
                throw new UserFriendlyException(string.Join(" ", yearValidationErrors));

            scenario.ScenarioName = input.ScenarioName.Trim();
            scenario.IsShared = input.IsShared;
            scenario.Stale = true;
            scenario.Notes = ScenarioSettingsModifiedNote;

            // No autoSave here: flushing mid-method inside the transactional unit of
            // work would hold write locks on tbl_Scenarios for the rest of the method
            // and block the parameter reads below. Let the UoW commit everything once.
            await _scenarioRepository.UpdateAsync(scenario);

            await UpsertScenarioYearParameterAsync(scenid, "YFST", input.FirstYear!.Value);
            await UpsertScenarioYearParameterAsync(scenid, "YLST", input.LastYear!.Value);
        }

        private async Task UpsertScenarioYearParameterAsync(int scenid, string parameterId, int value)
        {
            var target = await _scenarioParametersRepository.FindScenarioParameter(scenid, parameterId);
            if (target == null)
            {
                await _scenarioParametersRepository.InsertAsync(new ScenarioParamater
                {
                    Id = parameterId,
                    ScenarioId = scenid,
                    ParameterValue = value
                });
                return;
            }

            // The entity is tracked by the unit-of-work DbContext, so mutating it is
            // enough — the UoW persists the change on completion.
            target.ParameterValue = value;
        }

        // ──────────────────────────── Parameters page ────────────────────────────
        private const string ParametersModifiedNote = "Scenario parameters have been modified. Please perform a scenario re-run.";

        public async Task<List<ScenarioParameterDto>> GetScenarioParametersAsync(int scenid)
        {
            var list = await _scenarioParametersRepository.GetByScenarioAsync(scenid);
            return list.Select(p => new ScenarioParameterDto
            {
                Id = p.Id,
                ParameterId = p.Id,
                ParameterName = p.ParmName,
                ParameterDescription = p.ParmDescription,
                ScenarioId = p.ScenarioId,
                ParameterValue = p.ParameterValue
            }).ToList();
        }

        public async Task<List<TreatmentShiftDto>> GetTreatmentShiftsAsync(int scenid)
        {
            var rows = await _scenarioParametersRepository.GetTreatmentShiftsAsync(scenid);
            return rows.Select(r => new TreatmentShiftDto { Treatment = r.Treatment, MaxAccelerate = r.MaxAccelerate, MaxDelay = r.MaxDelay }).ToList();
        }

        public async Task SaveParametersAsync(int scenid, SaveParametersInputDto input)
        {
            var parameterValues = input?.ParameterValues ?? new Dictionary<string, double?>();
            var shifts = input?.Shifts ?? new List<TreatmentShiftDto>();
            var rows = shifts
                .Select(s => new TreatmentShiftRow { Treatment = s.Treatment, MaxAccelerate = s.MaxAccelerate, MaxDelay = s.MaxDelay })
                .ToList();
            await _scenarioParametersRepository.SaveParametersPageAsync(scenid, parameterValues, rows, ParametersModifiedNote);
        }

        // ──────────────────────────── Analysis Method page ────────────────────────────
        public async Task<AnalysisMethodDto> GetAnalysisMethodAsync(int scenid)
        {
            var scenario = await _scenarioRepository.FindScenarioByIdAsync(scenid);
            var libraryId = scenario?.LibraryId ?? Guid.Empty;

            var attrs = libraryId == Guid.Empty
                ? new List<FilteringAttribute>()
                : await _analysisMethodRepository.GetFilteringAttributesAsync(libraryId);
            var filters = await _analysisMethodRepository.GetScenarioFiltersAsync(scenid);

            return new AnalysisMethodDto
            {
                ScenarioId = scenid,
                ScenarioName = scenario?.ScenarioName ?? string.Empty,
                Notes = scenario?.CriteriaNotes ?? string.Empty,
                FilteringAttributes = attrs.Select(a => new FilteringAttributeDto { AssetType = a.AssetType, AttributeName = a.AttributeName, AttributeType = a.AttributeType }).ToList(),
                SavedFilters = filters.Select(f => new ScenarioFilterDto { EntityId = f.ScenId, FilterNo = f.FilterNo, AssetType = f.AssetType, AttributeName = f.AttributeName, Operation = f.Operation, TextValue = f.TextValue }).ToList()
            };
        }

        public Task<List<string>> GetAttributeValuesAsync(int scenid, string assetType, string attributeName)
            => _analysisMethodRepository.GetAttributeValuesAsync(scenid, assetType, attributeName);

        public async Task SaveCriteriaAsync(int scenid, List<ScenarioFilterDto> filters, string notes)
        {
            var entities = (filters ?? new List<ScenarioFilterDto>())
                .Where(f => !string.IsNullOrWhiteSpace(f.AttributeName))
                .Select(f => new ScenarioFilter
                {
                    ScenId = scenid,
                    FilterNo = f.FilterNo,
                    AssetType = string.IsNullOrWhiteSpace(f.AssetType) ? "*" : f.AssetType,
                    AttributeName = f.AttributeName,
                    Operation = f.Operation,
                    TextValue = f.TextValue ?? string.Empty
                })
                .ToList();
            await _analysisMethodRepository.SaveCriteriaAsync(scenid, entities, notes);
        }

        public Task<int> GetMatchedTreatmentCountAsync(int scenid, List<ScenarioFilterDto> filters)
        {
            var entities = (filters ?? new List<ScenarioFilterDto>())
                .Where(f => !string.IsNullOrWhiteSpace(f.AttributeName))
                .Select(f => new ScenarioFilter
                {
                    ScenId = scenid,
                    FilterNo = f.FilterNo,
                    AssetType = string.IsNullOrWhiteSpace(f.AssetType) ? "*" : f.AssetType,
                    AttributeName = f.AttributeName,
                    Operation = f.Operation,
                    TextValue = f.TextValue ?? string.Empty
                })
                .ToList();
            return _analysisMethodRepository.GetMatchedTreatmentCountAsync(scenid, entities);
        }

        // ──────────────────────────── Budget Constraints page ────────────────────────────
        // The grid always renders/treats the first column as the row's read-only "Year" label
        // (GetBudgetConstraintsAsync/BudgetConstraints.cshtml), matching the "Year"/"YearWork"
        // column Import already requires by name (ImportBudgetConstraintsAsync). Identifying it by
        // name here (falling back to plain column order only when no such name exists, e.g. older
        // data) keeps that column first regardless of the order columns were actually created in --
        // otherwise, a real criteria column created before the Year column exists could end up
        // being mistaken for it.
        private static List<BudgetConstraintColumn> OrderColumnsWithYearFirst(List<BudgetConstraintColumn> columns)
        {
            var yearColumn = columns.FirstOrDefault(c =>
                c.ColumnName.Equals("Year", StringComparison.OrdinalIgnoreCase) || c.ColumnName.Equals("YearWork", StringComparison.OrdinalIgnoreCase));
            if (yearColumn == null) return columns;

            var rest = columns.Where(c => c.Id != yearColumn.Id);
            return new List<BudgetConstraintColumn> { yearColumn }.Concat(rest).ToList();
        }

        public async Task<BudgetConstraintsGridDto> GetBudgetConstraintsAsync(int scenid)
        {
            var scenario = await _scenarioRepository.FindScenarioByIdAsync(scenid);
            var libraryId = scenario?.LibraryId ?? Guid.Empty;

            var columns = OrderColumnsWithYearFirst(await _budgetConstraintsRepository.GetColumnsAsync(scenid));
            var constraints = await _budgetConstraintsRepository.GetConstraintsAsync(scenid);
            var criteriaRows = await _budgetConstraintsRepository.GetCriteriaListAsync(scenid);
            var filteringAttributes = libraryId == Guid.Empty ? new List<FilteringAttribute>() : await _budgetConstraintsRepository.GetFilteringAttributesAsync(libraryId);
            var counts = libraryId == Guid.Empty ? (0, 0) : await _budgetConstraintsRepository.GetTreatmentCountsAsync(libraryId);

            var dto = new BudgetConstraintsGridDto
            {
                ScenarioId = scenid,
                ScenarioName = scenario?.ScenarioName ?? string.Empty,
                ScenarioNotes = scenario?.Notes ?? string.Empty,
                Locked = scenario?.Locked ?? false,
                Headers = columns.Select(c => c.ColumnName).ToList(),
                BridgesCount = counts.Item1,
                PavementsCount = counts.Item2,
                FilteringAttributes = filteringAttributes.Select(a => new FilteringAttributeDto { AssetType = a.AssetType, AttributeName = a.AttributeName, AttributeType = a.AttributeType }).ToList(),
                CriteriaList = (from crit in criteriaRows
                                join col in columns on crit.BCCId equals col.Id
                                select new BudgetCriteriaDto { ColumnId = col.Id, ColumnName = col.ColumnName, Criteria = crit.Criteria, CriteriaId = crit.Id }).ToList()
            };

            var yearColumnId = columns.FirstOrDefault()?.Id;
            var grouped = constraints
                .GroupBy(c => c.RowNumber)
                .OrderBy(g => g.FirstOrDefault(c => c.BCCId == yearColumnId)?.BudgetValue ?? 0);

            foreach (var rowGroup in grouped)
            {
                var cells = columns.Select(col =>
                {
                    var cell = rowGroup.FirstOrDefault(c => c.BCCId == col.Id);
                    return new BudgetCellViewDto { RowNumber = cell?.RowNumber, BCCId = cell?.BCCId, Value = cell?.BudgetValue };
                }).ToList();
                dto.Rows.Add(cells);
            }

            return dto;
        }

        public async Task UpdateBudgetConstraintsAsync(int scenid, List<BudgetCellDto> cells)
        {
            var inputs = (cells ?? new List<BudgetCellDto>())
                .Select(c => new BudgetCellInput { Row = c.Row, Col = c.Col, Value = c.Value })
                .ToList();
            await _budgetConstraintsRepository.UpdateBudgetCellsAsync(scenid, inputs);
        }

        public async Task<int> CreateBudgetCriteriaAsync(int scenid, string columnName, string criteria)
        {
            columnName = columnName?.Trim();
            criteria = BudgetCriteriaParser.NormalizeFieldNames(criteria?.Trim());
            if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(criteria))
                throw new UserFriendlyException("Column name and criteria are required.");
            if (!BudgetCriteriaParser.TryValidate(criteria, out var error))
                throw new UserFriendlyException(error);

            return await _budgetConstraintsRepository.CreateCriteriaAsync(scenid, columnName, criteria);
        }

        public async Task UpdateBudgetCriteriaAsync(int columnId, int scenid, string columnName, string criteria)
        {
            if (columnId <= 0)
                throw new UserFriendlyException("Invalid request data.");
            columnName = columnName?.Trim();
            criteria = BudgetCriteriaParser.NormalizeFieldNames(criteria?.Trim());
            if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(criteria))
                throw new UserFriendlyException("Column name and criteria are required.");
            if (!BudgetCriteriaParser.TryValidate(criteria, out var error))
                throw new UserFriendlyException(error);

            await _budgetConstraintsRepository.UpdateCriteriaAsync(columnId, scenid, columnName, criteria);
        }

        public Task DeleteBudgetCriteriaAsync(int id) => _budgetConstraintsRepository.DeleteCriteriaAsync(id);

        public Task DeleteBudgetRowAsync(int scenid, int yearWork, int rowNumber) => _budgetConstraintsRepository.DeleteRowAsync(scenid, yearWork, rowNumber);

        public async Task AddBudgetRowAsync(int scenid, int year)
        {
            if (year < 1000 || year > 9999)
                throw new UserFriendlyException("Year must be a 4-digit number (for example 2027).");

            var (firstYear, lastYear) = await _budgetConstraintsRepository.GetScenarioYearsAsync(scenid);
            if (firstYear.HasValue && lastYear.HasValue && (year < firstYear.Value || year > lastYear.Value))
                throw new UserFriendlyException($"Year must be between {firstYear} and {lastYear}.");

            // AddRowAsync creates the Year/YearWork column automatically if this is the very first
            // row -- only check for a duplicate when that column (and therefore a prior row) can
            // actually already exist.
            var columns = await _budgetConstraintsRepository.GetColumnsAsync(scenid);
            var yearColumn = columns.FirstOrDefault(c =>
                c.ColumnName.Equals("Year", StringComparison.OrdinalIgnoreCase) || c.ColumnName.Equals("YearWork", StringComparison.OrdinalIgnoreCase));
            if (yearColumn != null)
            {
                var constraints = await _budgetConstraintsRepository.GetConstraintsAsync(scenid);
                if (constraints.Any(c => c.BCCId == yearColumn.Id && c.BudgetValue == year))
                    throw new UserFriendlyException($"A budget row for year {year} already exists.");
            }

            await _budgetConstraintsRepository.AddRowAsync(scenid, year);
        }

        public Task ClearBudgetConstraintsAsync(int scenid) => _budgetConstraintsRepository.ClearBudgetsAsync(scenid);

        public async Task<(byte[] Content, string FileName)> ExportBudgetConstraintsAsync(int scenid)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var columns = await _budgetConstraintsRepository.GetColumnsAsync(scenid);
            var constraints = await _budgetConstraintsRepository.GetConstraintsAsync(scenid);
            var criteriaList = await _budgetConstraintsRepository.GetCriteriaListAsync(scenid);

            using var package = new ExcelPackage();
            var budgetSheet = package.Workbook.Worksheets.Add("Budget");
            for (int col = 0; col < columns.Count; col++)
            {
                budgetSheet.Cells[1, col + 1].Value = columns[col].ColumnName;
            }
            int rowIdx = 2;
            foreach (var rowGroup in constraints.GroupBy(c => c.RowNumber).OrderBy(g => g.Key))
            {
                foreach (var bc in rowGroup)
                {
                    var colIndex = columns.FindIndex(c => c.Id == bc.BCCId);
                    if (colIndex >= 0) budgetSheet.Cells[rowIdx, colIndex + 1].Value = bc.BudgetValue;
                }
                rowIdx++;
            }

            var criteriaSheet = package.Workbook.Worksheets.Add("Criteria");
            criteriaSheet.Cells[1, 1].Value = "BUDGET NAME";
            criteriaSheet.Cells[1, 2].Value = "CRITERIA";
            int critRow = 2;
            foreach (var crit in criteriaList)
            {
                var colName = columns.FirstOrDefault(c => c.Id == crit.BCCId)?.ColumnName;
                if (!string.IsNullOrEmpty(colName))
                {
                    criteriaSheet.Cells[critRow, 1].Value = colName;
                    criteriaSheet.Cells[critRow, 2].Value = crit.Criteria;
                    critRow++;
                }
            }

            return (package.GetAsByteArray(), $"BudgetConstraints_Scenario_{scenid}.xlsx");
        }

        public async Task<(byte[] Content, string FileName)> DownloadBudgetTemplateAsync(int scenid)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var templateColumns = await _budgetConstraintsRepository.GetTemplateColumnsAsync(scenid);

            using var package = new ExcelPackage();
            var budgetSheet = package.Workbook.Worksheets.Add("Budget");

            var headers = new List<string> { "Year" };
            headers.AddRange(templateColumns.Select(c => c.ColumnName));
            for (int i = 0; i < headers.Count; i++)
            {
                budgetSheet.Cells[1, i + 1].Value = headers[i];
                budgetSheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            int row = 2;
            for (int year = 2026; year <= 2036; year++)
            {
                budgetSheet.Cells[row, 1].Value = year;
                for (int columnIndex = 2; columnIndex <= headers.Count; columnIndex++)
                {
                    budgetSheet.Cells[row, columnIndex].Value = 100000000d;
                }
                row++;
            }
            if (headers.Count > 1)
            {
                budgetSheet.Cells[2, 2, row - 1, headers.Count].Style.Numberformat.Format = "$#,##0.00";
            }
            budgetSheet.Cells.AutoFitColumns();

            var criteriaSheet = package.Workbook.Worksheets.Add("Criteria");
            criteriaSheet.Cells[1, 1].Value = "BUDGET NAME";
            criteriaSheet.Cells[1, 2].Value = "CRITERIA";
            criteriaSheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;
            int critRow = 2;
            foreach (var c in templateColumns)
            {
                criteriaSheet.Cells[critRow, 1].Value = c.ColumnName;
                criteriaSheet.Cells[critRow, 2].Value = $"DISTRICT='{c.District}' AND {c.AttributeName}='{c.AttributeValue}' AND INTERSTATE='{c.IsInterstate}' AND ASSETTYPE='{c.AssetType}'";
                critRow++;
            }
            criteriaSheet.Cells.AutoFitColumns();

            return (package.GetAsByteArray(), "BudgetConstraintsTemplate.xlsx");
        }

        public async Task ImportBudgetConstraintsAsync(int scenid, byte[] xlsxBytes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var xlsxStream = new MemoryStream(xlsxBytes);
            using var package = new ExcelPackage(xlsxStream);

            if (package.Workbook.Worksheets.Count == 0)
                throw new UserFriendlyException("Excel file is empty or corrupted.");

            var budgetSheet = package.Workbook.Worksheets["Budget"];
            var criteriaSheet = package.Workbook.Worksheets["Criteria"];
            if (budgetSheet == null || criteriaSheet == null)
                throw new UserFriendlyException("Required worksheets ('Budget' and 'Criteria') not found.");
            if (budgetSheet.Dimension == null || budgetSheet.Dimension.Rows < 2)
                throw new UserFriendlyException("Budget sheet has no data.");

            int rowCount = budgetSheet.Dimension.Rows;
            int colCount = budgetSheet.Dimension.Columns;
            if (colCount == 0)
                throw new UserFriendlyException("Budget sheet has no columns.");

            var budgetColumnNames = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var name = budgetSheet.Cells[1, col].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(name)) budgetColumnNames.Add(name);
            }
            if (!budgetColumnNames.Any())
                throw new UserFriendlyException("Budget sheet has no valid column names in header row.");
            if (budgetColumnNames.Count != budgetColumnNames.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                throw new UserFriendlyException("Budget sheet contains duplicate column names. Please fix and try again.");
            if (!budgetColumnNames.Any(c => c.Equals("Year", StringComparison.OrdinalIgnoreCase) || c.Equals("YearWork", StringComparison.OrdinalIgnoreCase)))
                throw new UserFriendlyException("Budget sheet must include a 'Year' or 'YearWork' column.");

            var constraintsRaw = new List<BudgetImportConstraint>();
            var budgetRowData = new Dictionary<int, Dictionary<string, decimal>>();
            for (int r = 2; r <= rowCount; r++)
            {
                var rowDict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                for (int col = 1; col <= colCount; col++)
                {
                    var columnName = budgetSheet.Cells[1, col].Text?.Trim();
                    var cellValue = budgetSheet.Cells[r, col].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(cellValue)) continue;

                    var cleaned = cellValue.Replace("$", "").Replace(",", "").Trim();
                    if (!decimal.TryParse(cleaned, out var budgetValue))
                        throw new UserFriendlyException($"Invalid numeric value '{cellValue}' in Budget sheet at row {r}, column '{columnName}'.");

                    constraintsRaw.Add(new BudgetImportConstraint { ColumnName = columnName, BudgetValue = budgetValue, RowNumber = r });
                    rowDict[columnName] = budgetValue;
                }
                if (rowDict.Any()) budgetRowData[r] = rowDict;
            }

            // Validate years against the scenario's first/last year.
            var (firstYear, lastYear) = await _budgetConstraintsRepository.GetScenarioYearsAsync(scenid);
            if (firstYear.HasValue && lastYear.HasValue)
            {
                var yearColName = budgetColumnNames.FirstOrDefault(c => c.Equals("Year", StringComparison.OrdinalIgnoreCase) || c.Equals("YearWork", StringComparison.OrdinalIgnoreCase));
                foreach (var rowEntry in budgetRowData)
                {
                    if (yearColName != null && rowEntry.Value.TryGetValue(yearColName, out var yearVal))
                    {
                        int year = (int)yearVal;
                        if (year < firstYear.Value || year > lastYear.Value)
                            throw new UserFriendlyException($"Year {year} is outside the scenario's allowed range ({firstYear}–{lastYear}). All budget years must be within the scenario's start and end year.");
                    }
                }
            }

            int criteriaRowCount = criteriaSheet.Dimension?.Rows ?? 0;
            if (criteriaRowCount < 2)
                throw new UserFriendlyException("Criteria sheet has no data.");

            var parsedCriteria = new List<(string ColumnName, string CriteriaValue, string BudgetDistrict, string IsInterstate, string AssetType, string AttributeName, string AttributeValue)>();
            for (int r = 2; r <= criteriaRowCount; r++)
            {
                var columnName = criteriaSheet.Cells[r, 1].Text?.Trim();
                var criteriaValue = criteriaSheet.Cells[r, 2].Text?.Trim();
                if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(criteriaValue)) continue;

                if (!budgetColumnNames.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                    throw new UserFriendlyException($"Criteria row {r} references unknown budget column '{columnName}'.");
                if (!BudgetCriteriaParser.TryValidate(criteriaValue, out var criteriaError))
                    throw new UserFriendlyException($"Criteria row {r} {criteriaError}");

                var parsed = BudgetCriteriaParser.Parse(criteriaValue, BudgetCriteriaParser.CategoryAttributeName);
                var normalizedAttributeName = parsed.AttributeName?.Trim().ToUpperInvariant() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedAttributeName))
                    throw new UserFriendlyException($"Criteria row {r} must include {BudgetCriteriaParser.CategoryAttributeName} as the attribute name.");

                parsedCriteria.Add((columnName, BudgetCriteriaParser.NormalizeFieldNames(criteriaValue), parsed.BudgetDistrict, parsed.IsInterstate, parsed.AssetType, normalizedAttributeName, parsed.AttributeValue));
            }

            // Build the expanded with-attributes rows.
            var extendedRows = new List<ScenarioBudgetWithAttribute>();
            foreach (var criterion in parsedCriteria)
            {
                foreach (var rowData in budgetRowData.Values)
                {
                    int year = rowData.ContainsKey("Year") ? Convert.ToInt32(rowData["Year"])
                        : rowData.ContainsKey("YearWork") ? Convert.ToInt32(rowData["YearWork"]) : 0;

                    foreach (var kvp in rowData)
                    {
                        if (kvp.Key.Equals("Year", StringComparison.OrdinalIgnoreCase) || kvp.Key.Equals("YearWork", StringComparison.OrdinalIgnoreCase)) continue;
                        if (!criterion.ColumnName.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase)) continue;

                        if (!extendedRows.Any(e => e.YearWork == year && e.ScenId == scenid && e.District == criterion.BudgetDistrict
                            && e.IsInterstate == criterion.IsInterstate && e.AssetType == criterion.AssetType
                            && e.AttributeName == criterion.AttributeName && e.AttributeValue == criterion.AttributeValue))
                        {
                            extendedRows.Add(new ScenarioBudgetWithAttribute
                            {
                                YearWork = year,
                                ScenId = scenid,
                                District = criterion.BudgetDistrict,
                                IsInterstate = criterion.IsInterstate,
                                AssetType = criterion.AssetType,
                                AttributeName = criterion.AttributeName,
                                AttributeValue = criterion.AttributeValue,
                                Budget = kvp.Value
                            });
                        }
                    }
                }
            }

            var criteriaToSave = parsedCriteria.Select(c => new BudgetImportCriteria { ColumnName = c.ColumnName, CriteriaValue = c.CriteriaValue }).ToList();
            await _budgetConstraintsRepository.SaveImportAsync(scenid, budgetColumnNames, constraintsRaw, criteriaToSave, extendedRows);
        }

        public Task<ScenarioDto> GetAsync(Guid id)
        {
            return Task.FromResult<ScenarioDto>(null);
        }
    }
}
