
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ProjectBuilder.Entities;
using ProjectBuilder.Helpers;
using ProjectBuilder.Models.CandidatePool;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Scenario.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(TreatmentPoolLibraryPermissions.Default)]
    public class CandidatePoolAppService : ApplicationService, ICandidatePoolAppService
    {
        private readonly ICandidatePoolRepository _candidateRepository;
        private readonly IRunScenarioRepository _scenarioRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProjectBuilderMapper _mapper;
        private readonly IPermissionChecker _permissionChecker;


        public Guid Libid { get; set; }
        public CandidatePoolAppService(ICandidatePoolRepository candidateRepository,
            IRunScenarioRepository scenarioRepository,
            ITreatmentRepository treatmentRepository,
            IHttpContextAccessor httpContextAccessor, ProjectBuilderMapper mapper,
            IPermissionChecker permissionChecker)
        {
            _candidateRepository = candidateRepository;
            _scenarioRepository = scenarioRepository;
            _treatmentRepository = treatmentRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _permissionChecker = permissionChecker;
        }

        public async Task<CandidatePoolDto> GetCandidatePoolDataAsync(int? scenid, bool? importing, string searchString = "")
        {
            var candidatePoolVm = new CandidatePoolDto { Libraries = new List<CandidatePoolDto>() };
            var scenarioVm = new ScenarioDto();
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User.Identity?.IsAuthenticated != true || httpContext.Session.GetString("RoleName") == null)
            {
                return candidatePoolVm;
            }
            var candidatePools = await _candidateRepository.GetPoolsListAsync();
            var scenarios = await _scenarioRepository.GetScenariosListAsync();

            candidatePoolVm.Libraries = _mapper.Map(candidatePools);
            scenarioVm.Scenarios = _mapper.Map(scenarios);

            await GetSource(candidatePoolVm);
            await ImportingLibrary(scenid, importing, candidatePoolVm);

            if (scenid.HasValue)
            {
                searchString = candidatePoolVm.Name;
                candidatePoolVm.Libraries = candidatePoolVm.Libraries
                    .Where(library => library.Name == searchString)
                    .ToList();
            }
            else if (!string.IsNullOrEmpty(searchString))
            {
                candidatePoolVm.Libraries = candidatePoolVm.Libraries
                    .Where(library => library.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return candidatePoolVm;
        }
        public async Task<List<CandidatePoolDto>> GetPoolsListAsync() 
        {
            var libraries = await _candidateRepository.GetPoolsListAsync();
            return _mapper.Map(libraries);
        }
        public async Task GetSource(CandidatePoolDto candidatePool)
        {
            var sources = await _candidateRepository.GetSource(candidatePool.Libraries.Select(x => x.Id).ToList());

            foreach (var pool in candidatePool.Libraries)
            {
                pool.Source = sources.FirstOrDefault(x => x.Key == pool.Id).Value;
            }
        }
        public async Task ImportingLibrary(int? scenid, bool? isimporting, CandidatePoolDto candidatepoolViewModel)
        {
            if (scenid.HasValue)
            {
                var scenraio = await _scenarioRepository.FindAsync(scenid.Value);
                candidatepoolViewModel.IsImporting = isimporting.HasValue ? isimporting.Value : false;
                candidatepoolViewModel.FromScenario = !candidatepoolViewModel.IsImporting;
                if (scenraio is not null)
                {
                    var candidatepool = candidatepoolViewModel.Libraries.FirstOrDefault(l => l.Id == scenraio.LibraryId);
                    if (candidatepool != null)
                    {
                        candidatepoolViewModel.Id = candidatepool.Id;
                        candidatepoolViewModel.Name = candidatepool.Name;
                    }
                }
            }
        }
        public async Task<CandidatePoolDto> ListAsync(int? scenid, bool? importing, Guid userId = default)
        {
            var libraryVm = new CandidatePoolDto();
            var scenariosVm = new ScenarioDto();
            var scenarios = await _scenarioRepository.GetScenariosListAsync();
            scenariosVm.Scenarios = _mapper.Map(scenarios);

            _candidateRepository.UserId = userId;
            _candidateRepository.IncludeAllUsers = await _permissionChecker.IsGrantedAsync(AdministrationPermissions.Default);
            var libraries = (await _candidateRepository.GetUserLibrariesAsync())
                .OrderByDescending(lib => lib.CreatedAt ?? DateTime.MinValue).ToList();

            libraryVm.Libraries = _mapper.Map(libraries);

            await GetSource(libraryVm);

            foreach (var lib in libraryVm.Libraries)
                lib.IsDeleteable = lib.ScenarioCount == 0;

            foreach (var sce in scenariosVm.Scenarios)
            {
                foreach (var lib in libraryVm.Libraries)
                {
                    if (lib.Id == sce.LibraryId)
                        lib.SceName = sce.ScenarioName;
                    if (lib.SceName == null)
                        lib.SceName = "";
                }
            }

            return libraryVm;
        }
        [Authorize(TreatmentPoolLibraryPermissions.Create)]
        public async Task<Guid> CreateNewLibrary(CreateCandidatePoolDto libraryViewModel, Guid userId, string? libraraysource = null)
        {
            if (!HasMeaningfulName(libraryViewModel.Name))
                throw new UserFriendlyException("Treatment Pool Library name must contain at least one letter or number.");

            var newLibrary = new CandidatePool
            {
                UserId = userId,
                Name = libraryViewModel.Name?.Trim(),
                Description = libraryViewModel.Description?.Trim(),
                IsActive = true,
                IsShared = libraryViewModel.IsShared
            };

            if (!await CheckLibraryNameAsync(newLibrary.Name, null))
                throw new UserFriendlyException("Name already exists.");

            var id = await _candidateRepository.CreateNewCandidatePool(newLibrary);
            Libid = id; // Keeping the same variable reference

            if (id != Guid.Empty && !libraryViewModel.IsEmptyLibrary)
            {
                await _candidateRepository.PopulateCandidatePool(
                    id,
                    libraraysource,
                    string.IsNullOrEmpty(libraryViewModel.SelectedAsset) ? null
                    : libraryViewModel.SelectedAsset == "Brdige" ? "B" : "P",
                    libraryViewModel.SelectedDistrict,
                    libraryViewModel.SelectedCounty,
                    libraryViewModel.SelectedRoute,
                    libraryViewModel.MinYear,
                    libraryViewModel.MaxYear
                );
            }

            return id;
        }
        [Authorize(TreatmentPoolLibraryPermissions.Edit)]
        public async Task<bool> EditLibraryAsync(UpdateCandidatePoolDto libraryViewModel)
        {
            if (!HasMeaningfulName(libraryViewModel.Name))
                throw new UserFriendlyException("Treatment Pool Library name must contain at least one letter or number.");

            try
            {
                var library = await _candidateRepository.FirstOrDefaultAsync(x => x.Id == libraryViewModel.Id);

                if (library == null)
                {
                    return false;
                }

                library.Name = libraryViewModel.Name;
                library.Description = libraryViewModel.Description;
                library.IsShared = libraryViewModel.IsShared;

                await _candidateRepository.UpdateAsync(library);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the library.", ex);
            }
        }
        // Rejects names made up entirely of special characters (e.g. "%%%%", "####")
        // so the library list can't fill up with meaningless entries.
        private static bool HasMeaningfulName(string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Any(char.IsLetterOrDigit);
        }

        public async Task<bool> CheckLibraryNameAsync(string libraryName, string? libraryId)
        {
            libraryName = libraryName?.Trim();
            if (string.IsNullOrWhiteSpace(libraryName))
                return false;

            Guid? excludeId = Guid.TryParse(libraryId, out var id) ? id : null;
            return !await _candidateRepository.IsLibraryNameTakenAsync(libraryName, excludeId);
        }
        [Authorize(TreatmentPoolLibraryPermissions.Import)]
        public async Task<string?> ImportTreatmentsAsync(ImportTreatmentViewModel data)
        {
            var excelfilepath = await data.ExcelFile.SaveUploadedFile();

            var validationError = _treatmentRepository.ValidateTreatmentExcelFile(excelfilepath, data.TabName, data.ImportType);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                _treatmentRepository.DeleteExcelFile(excelfilepath);
                return validationError;
            }

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

            var errormessage = await _treatmentRepository.ImportTreatmentsFromExcelFile(
                source,
                data.ImportType,
                excelfilepath,
                data.TabName,
                data.LibraryId,
                data.fromScratch,
                data.keepAll
            );

            _treatmentRepository.DeleteExcelFile(excelfilepath);

            return errormessage;
        }
        public async Task<string?> ImportMASAndB2PAsync(ImportMASAndB2PViewModel data)
        {
            var excelfilepath = await data.ExcelFile.SaveUploadedFile();
            var errormessage = await _treatmentRepository.ImportMASAndB2PFromExcelFile(data.IsMAS, excelfilepath, data.TabName);
            _treatmentRepository.DeleteExcelFile(excelfilepath);
            return errormessage;
        }
        public async Task<CandidatePoolDto> GetAsync(Guid id)
        {
            var library = await _candidateRepository.FirstOrDefaultAsync(x => x.Id == id);
            return library == null ? null : _mapper.Map(library);
        }
        public Task<PagedResultDto<CandidatePoolDto>> GetListAsync(GetCandidatePoolInput input)
        {
            throw new NotImplementedException();
        }
        public Task<CandidatePoolDto> CreateAsync(CreateCandidatePoolDto input)
        {
            throw new NotImplementedException();
        }
        public Task<CandidatePoolDto> UpdateAsync(Guid id, UpdateCandidatePoolDto input)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
