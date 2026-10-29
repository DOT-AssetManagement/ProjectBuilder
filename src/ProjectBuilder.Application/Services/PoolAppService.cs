using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.CandidatePool.Dtos;
using ProjectBuilder.Models.Pool;
using ProjectBuilder.Models.Pool.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Users;

namespace ProjectBuilder.Services
{
    [Authorize(SupplementalCandidatesPermissions.PoolsPageActions.Default)]
    public class PoolAppService : ApplicationService, IPoolAppService
    {
        private readonly IPoolRepository _poolRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProjectBuilderMapper _mapper;
        private readonly IDssScenarioRepository _dssScenarioRepository;
        private readonly IPermissionChecker _permissionChecker;

        public PoolAppService(IPoolRepository poolRepository, ICurrentUser currentUser, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, ProjectBuilderMapper mapper, IDssScenarioRepository dssScenarioRepository, IPermissionChecker permissionChecker)
        {
            _poolRepository = poolRepository;
            _currentUser = currentUser;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _dssScenarioRepository = dssScenarioRepository;
            _permissionChecker = permissionChecker;
        }


        public async Task<PoolDto> CheckName(string poolName)
        {
            var pool = await _poolRepository.CheckName(poolName);
            if (pool == null) return null;
            return new PoolDto { Id = pool.Id, Name = pool.Name, Description = pool.Description, IsActive = pool.IsActive ?? false, IsSharedLibrary = pool.IsSharedLibrary ?? false };
        }

        public async Task<PoolDto> CreateAsync(CreatePoolDto input)
        {
            var userId = _currentUser.Id ?? Guid.Empty;
            var id = await _poolRepository.CreatePool(input.Name, input.Description, userId, _currentUser.UserName ?? "system", input.IsSharedLibrary);
            return id.HasValue ? new PoolDto { Id = id.Value, Name = input.Name, Description = input.Description, IsSharedLibrary = input.IsSharedLibrary } : null;
        }

        [HttpPost("api/app/pool/create")]
        public async Task<Guid?> Create(CreatePoolDto input)
        {
            var userId = _currentUser.Id ?? Guid.Empty;
            return await _poolRepository.CreatePool(input.Name, input.Description, userId, _currentUser.UserName ?? "system", input.IsSharedLibrary);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _poolRepository.DeleteAsync(id);
        }

        public async Task ClearAsync(Guid id)
        {
            _poolRepository.ClearPool(id);
        }

        [HttpGet("api/app/Pool/getAll")]
        public async Task<List<CandidatePoolDto>> GetAllAsync(string name, string poolId)
        {
            var userId = _currentUser.Id ?? Guid.Empty;
            // Legacy visibility rules (mirrors ScenarioAppService.GetScenariosAsync): admins see
            // every pool; regular users see their own pools plus any pool marked as shared.
            var canViewAll = await _permissionChecker.IsGrantedAsync(AdministrationPermissions.Default);
            var pools = _poolRepository.GetPoolsByUserId(userId, name, poolId, canViewAll);
            var dtos = _mapper.Map(pools);

            // DSS pools run through the DEA engine (tbl_dss_Scenarios), not the PB treatment
            // optimizer (tbl_pb_Scenarios) — see OnPostRunDssScenarioAsync in Pools/Index.cshtml.cs.
            var poolIds = dtos.Select(p => p.Id).ToList();
            var dssScenarios = await _dssScenarioRepository.GetByPoolIdsAsync(poolIds);
            var latestDssScenarioByPool = dssScenarios
                .Where(s => s.PoolId.HasValue)
                .GroupBy(s => s.PoolId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.CreatedAt).First());

            foreach (var dto in dtos)
            {
                if (latestDssScenarioByPool.TryGetValue(dto.Id, out var dssScenario))
                {
                    dto.SceName = dssScenario.ScenName;
                }
            }

            return dtos;
        }

        [HttpPut("api/app/Pool/import-work-candidates")]
        public bool ImportWorkCandidates(string excelFilePath, string importSourceName, string worksheetName, Guid poolId, bool forgiving = false, bool deleteAll = false)
        {
            ValidateWorksheetSelection(excelFilePath, worksheetName);
            ValidateCosts(excelFilePath, worksheetName);
            return _poolRepository.Import(excelFilePath, importSourceName, worksheetName, poolId, forgiving, deleteAll);
        }

        private static void ValidateCosts(string excelFilePath, string? worksheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(excelFilePath));
            var worksheet = package.Workbook.Worksheets.Count > 1 && !string.IsNullOrWhiteSpace(worksheetName)
                ? package.Workbook.Worksheets.First(w => string.Equals(w.Name, worksheetName, StringComparison.OrdinalIgnoreCase))
                : package.Workbook.Worksheets[0];

            var lastColumn = worksheet.Dimension?.End.Column ?? 0;
            var costColumn = Enumerable.Range(1, lastColumn)
                .FirstOrDefault(col => string.Equals(worksheet.Cells[1, col].Text?.Trim(), "Cost", StringComparison.OrdinalIgnoreCase));
            if (costColumn == 0)
            {
                return; // No Cost column to validate — BulkInserter's forgiving import will just leave it null.
            }

            var lastRow = worksheet.Dimension?.End.Row ?? 1;
            var badRows = new List<int>();
            for (int row = 2; row <= lastRow; row++)
            {
                var text = worksheet.Cells[row, costColumn].Text;
                if (!string.IsNullOrWhiteSpace(text) && double.TryParse(text, out var cost) && cost < 1)
                {
                    badRows.Add(row);
                }
            }

            if (badRows.Count > 0)
            {
                throw new UserFriendlyException($"Row(s) {string.Join(", ", badRows)} have a Cost under $1. Every work candidate needs a Cost of at least $1 — a $0 candidate fails the scenario Run for the entire pool.");
            }
        }

        // Confirms which sheet the import will actually read from before it ever reaches the
        // legacy ExcelHandler.ImportDSSWorkCandidatesFromExcel pipeline, which otherwise silently
        // falls back to the first sheet whenever Tab Name is blank or doesn't match — risking a
        // wrong-sheet import going unnoticed for a multi-sheet file.
        private void ValidateWorksheetSelection(string excelFilePath, string? worksheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                using var package = new ExcelPackage(new FileInfo(excelFilePath));
                var worksheets = package.Workbook.Worksheets;

                if (worksheets.Count > 1)
                {
                    if (string.IsNullOrWhiteSpace(worksheetName))
                    {
                        var names = string.Join(", ", worksheets.Select(w => w.Name));
                        throw new UserFriendlyException($"This file has multiple sheets ({names}). Enter the exact sheet name to import from in \"Tab Name\".");
                    }

                    if (!worksheets.Any(w => string.Equals(w.Name, worksheetName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var names = string.Join(", ", worksheets.Select(w => w.Name));
                        throw new UserFriendlyException($"No sheet named \"{worksheetName}\" was found. Available sheets: {names}.");
                    }
                }
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read uploaded Excel file for worksheet validation.");
                throw new UserFriendlyException("The uploaded file could not be read. Make sure it's a valid Excel (.xlsx) file.");
            }
        }

        public bool ImportBridgeToPavements(string excelFilePath)
        {
            var result = _poolRepository.ImportBridgeToPavements(excelFilePath);
            if (!result)
                throw new Exception("An error occurred during the import process.");
            return true;
        }

        public async Task<bool> ImportAssetSegmentationAsync(string excelFilePath, string worksheetName)
        {
            var result = _poolRepository.ImportAssetSegmentation(excelFilePath, worksheetName);
            if (!result)
                throw new Exception("An error occurred during the import process.");
            return true;
        }

        public async Task<bool> ImportSuperAssetsAsync(string excelFilePath, string worksheetName)
        {
            var result = _poolRepository.ImportSuperAssets(excelFilePath, worksheetName);
            if (!result)
                throw new Exception("An error occurred during the import process.");
            return true;
        }

        public async Task<PoolDto> GetAsync(Guid id)
        {
            var pool = await _poolRepository.GetAsync(id);
            return ToDssPoolDto(pool);
        }

        public async Task<PagedResultDto<PoolDto>> GetListAsync(GetPoolInput input)
        {
            var pools = await _poolRepository.GetListAsync();
            var paged = pools.Skip(input.SkipCount).Take(input.MaxResultCount).Select(ToDssPoolDto).ToList();
            return new PagedResultDto<PoolDto>(pools.Count, paged);
        }

        public async Task<PoolDto> UpdateAsync(Guid id, UpdatePoolDto input)
        {
            var pool = await _poolRepository.GetAsync(id);
            pool.Name = input.Name;
            pool.Description = input.Description;
            pool.IsSharedLibrary = input.IsSharedLibrary;
            pool.UpdatedBy = _currentUser.UserName ?? "system";
            pool.UpdatedAt = DateTime.Now;
            var response = await _poolRepository.UpdateAsync(pool);
            return ToDssPoolDto(response);
        }

        private static PoolDto ToDssPoolDto(UserPoolEntity p) => new PoolDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedBy = p.CreatedBy,
            CreatedAt = p.CreatedAt,
            IsActive = p.IsActive ?? false,
            IsSharedLibrary = p.IsSharedLibrary ?? false,
            UpdatedBy = p.UpdatedBy,
            UpdatedAt = p.UpdatedAt,
            SourcePoolId = p.SourcePoolId
        };

        public async Task<string> GetDssScenarioNameByPoolIdAsync(Guid poolId)
        {
            var scenario = await _dssScenarioRepository.GetByPoolIdAsync(poolId);
            return scenario?.ScenName;
        }

        public async Task CreateDssScenarioAsync(Guid poolId, string name, string description, string createdBy)
        {
            await _dssScenarioRepository.CreateForPoolAsync(poolId, name, description, createdBy);
        }

        public async Task UpdateDssScenarioAsync(Guid poolId, string name, string description)
        {
            await _dssScenarioRepository.UpdateByPoolIdAsync(poolId, name, description);
        }
    }
}
