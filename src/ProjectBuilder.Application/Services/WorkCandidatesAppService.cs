using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.WorkCandidates;
using ProjectBuilder.Models.WorkCandidates.Dtos;
using ProjectBuilder.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Users;

namespace ProjectBuilder.Services
{
    [Authorize(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Default)]
    public class WorkCandidatesAppService : ApplicationService, IWorkCandidatesAppService
    {
        private readonly IWorkCandidatesRepository _workCandidatesRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ProjectBuilderMapper _mapper;

        public WorkCandidatesAppService(IWorkCandidatesRepository workCandidatesRepository, ICurrentUser currentUser, ProjectBuilderMapper mapper)
        {
            _workCandidatesRepository = workCandidatesRepository;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<List<string>> GetDistinctProjectNamesAsync(Guid poolId)
        {
            var names = await _workCandidatesRepository.GetDistinctProjectNames(poolId);
            return names
                .OrderBy(n => int.TryParse(new string(n.Where(char.IsDigit).ToArray()), out var num) ? num : int.MaxValue)
                .ToList();
        }

        public async Task<WorkCandidatesDto> GetAsync(Guid id)
        {
            var entity = await _workCandidatesRepository.GetAsync(id);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map(entity);
        }

        public async Task<List<WorkCandidatesDto>> GetAllByPoolAndWorkCandidateIds(Guid libraryId, List<Guid> workCandidateIds)
        {
            var entities = await _workCandidatesRepository.GetAllByPoolAndWorkCandidateIds(libraryId, workCandidateIds);
            if (entities == null)
            {
                return null;
            }
            return _mapper.Map(entities);
        }

        public Task<PagedResultDto<WorkCandidatesDto>> GetListAsync(GetWorkCandidatesInput input)
        {
            throw new NotImplementedException();
        }

        public async Task<List<WorkCandidatesDto>> GetAllByPoolOrProjectIdAsync(Guid libraryId, Guid projectId)
        {
            var entities = await _workCandidatesRepository.GetListByPoolOrProjectId(libraryId, projectId);
            if (entities == null)
            {
                return null;
            }
            return _mapper.Map(entities);
        }

        // Every column of tbl_dss_WorkCandidates (all ~90 fields, not just the grid's display
        // subset), with headers matching the entity's field names — the same underscored names
        // the Excel import maps by, so this doubles as a full data dump and a re-importable file.
        public async Task<(byte[] Content, string FileName)> ExportAsync(Guid poolId, string poolName)
        {
            var entities = await _workCandidatesRepository.GetListByPoolOrProjectId(poolId, Guid.Empty);
            var properties = typeof(WorkCandidateEntity).GetProperties();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("WorkCandidates");

            for (int c = 0; c < properties.Length; c++)
            {
                sheet.Cells[1, c + 1].Value = properties[c].Name;
                sheet.Cells[1, c + 1].Style.Font.Bold = true;
            }

            for (int r = 0; r < entities.Count; r++)
            {
                int row = r + 2;
                for (int c = 0; c < properties.Length; c++)
                {
                    sheet.Cells[row, c + 1].Value = ToCellValue(properties[c].GetValue(entities[r]));
                }
            }
            sheet.Cells.AutoFitColumns();

            return (package.GetAsByteArray(), $"WorkCandidates_{SanitizeFileNameSegment(poolName)}.xlsx");
        }

        private static object ToCellValue(object value) => value switch
        {
            null => null,
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            Guid g => g.ToString(),
            _ => value
        };

        private static string SanitizeFileNameSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Pool";
            return string.Concat(value.Select(c => System.IO.Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();
        }

        public async Task<WorkCandidatesDto> GetByProjectId(Guid projectId)
        {
            var entity = await _workCandidatesRepository.GetByProjectId(projectId);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map(entity);
        }

        [Authorize(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await _workCandidatesRepository.DeleteAsync(id);
        }

        public Task<WorkCandidatesDto> CreateAsync(CreateWorkCandidatesDto input)
        {
            throw new NotImplementedException();
        }

        [Authorize(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Create)]
        // Bridge candidates need at least one way to identify the bridge, the analysis years must
        // be present, and Min./Max. Year must bracket the preferred Year — none of this is enforced
        // anywhere else on the save path, so it's checked once here for both create and update.
        private static void ValidateWorkCandidate(string assetType, string brkey, string bridgeId, int? year, int? minYear, int? maxYear, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new UserFriendlyException("Description cannot be blank or only spaces.");

            if (string.Equals(assetType, "B", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(brkey) && string.IsNullOrWhiteSpace(bridgeId))
                throw new UserFriendlyException("BRKEY or Bridge ID is required for Bridge asset type.");

            if (!year.HasValue || !minYear.HasValue || !maxYear.HasValue)
                throw new UserFriendlyException("Year, Min. Year, and Max. Year are required.");

            if (minYear > maxYear)
                throw new UserFriendlyException("Min. Year cannot be greater than Max. Year.");
            if (year < minYear || year > maxYear)
                throw new UserFriendlyException("Year must be between Min. Year and Max. Year.");
        }

        public async Task<bool> CreateWorkCandidateAsync(CreateWorkCandidatesDto input, Guid libraryId)
        {
            ValidateWorkCandidate(input.AssetType, input.BRKEY, input.BRIDGE_ID, input.Year, input.MinYear, input.MaxYear, input.Description);
            input.Description = input.Description.Trim();

            var entity = _mapper.Map(input);
            var response = await _workCandidatesRepository.CreateWorkCandidate(entity);
            await _workCandidatesRepository.UpdatePool(libraryId, false);
            return response;
        }

        public Task<WorkCandidatesDto> UpdateAsync(Guid id, UpdateWorkCandidatesDto input)
        {
            throw new NotImplementedException();
        }

        [Authorize(SupplementalCandidatesPermissions.PoolsPageActions.WorkCandidates.Edit)]
        public async Task<WorkCandidatesDto> UpdateWorkCandidateAsync(Guid id, UpdateWorkCandidatesDto input, Guid libraryId)
        {
            ValidateWorkCandidate(input.AssetType, input.BRKEY, input.BRIDGE_ID, input.Year, input.MinYear, input.MaxYear, input.Description);

            var entity = await _workCandidatesRepository.GetAsync(id);

            entity.District = input.District;
            entity.Cnty = input.Cnty;
            entity.Route = input.Route;
            entity.AssetType = input.AssetType;
            entity.FromSection = input.FromSection;
            entity.ToSection = input.ToSection;
            entity.Description = input.Description.Trim();
            entity.ProjectName = input.ProjectName;
            entity.BRKEY = input.BRKEY;
            entity.BRIDGE_ID = input.BRIDGE_ID;
            entity.Cost = input.Cost;
            entity.IsIsolatedBridge = input.IsIsolatedBridge;
            entity.HSIP_Eligible = input.HSIP_Eligible;
            entity.Excess_Safety_Value = input.Excess_Safety_Value;
            entity.Supported_by_RSA_Safety_Study = input.Supported_by_RSA_Safety_Study;
            entity.Degree_of_Safety_Improvement = input.Degree_of_Safety_Improvement;
            entity.Supports_Hazard_Mitigation = input.Supports_Hazard_Mitigation;
            entity.Improves_Access_to_Emergency_Services = input.Improves_Access_to_Emergency_Services;
            entity.Travel_Time_Savings = input.Travel_Time_Savings;
            entity.Operating_Cost_Savings = input.Operating_Cost_Savings;
            entity.Degree_of_Mobility_Improvement = input.Degree_of_Mobility_Improvement;
            entity.Freight_Improvement = input.Freight_Improvement;
            entity.Facilitates_Transit_Use = input.Facilitates_Transit_Use;
            entity.Facilitates_Active_Transportation = input.Facilitates_Active_Transportation;
            entity.Bike_Lanes_Constructed = input.Bike_Lanes_Constructed;
            entity.New_Cyclists = input.New_Cyclists;
            entity.Support_for_Job_Growth = input.Support_for_Job_Growth;
            entity.Support_for_Tourism = input.Support_for_Tourism;
            entity.Support_for_Economic_Goals = input.Support_for_Economic_Goals;
            entity.Support_for_Recreation = input.Support_for_Recreation;
            entity.Consistency_with_Existing_Plans = input.Consistency_with_Existing_Plans;
            entity.Sidewalks_and_Curb_Ramps_Constructed_or_Improved = input.Sidewalks_and_Curb_Ramps_Constructed_or_Improved;
            entity.Change_in_Accessible_Destinations = input.Change_in_Accessible_Destinations;
            entity.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations = input.Percent_of_Acessibilty_Improvement_for_Disadvantaged_Populations;
            entity.Support_for_Environmental_Justice = input.Support_for_Environmental_Justice;
            entity.Reduced_Flood_Closure_Risk = input.Reduced_Flood_Closure_Risk;
            entity.Wetlands_Improved = input.Wetlands_Improved;
            entity.Wildlife_Crossings = input.Wildlife_Crossings;
            entity.Reduced_Fuel_Consumption = input.Reduced_Fuel_Consumption;
            entity.Degree_of_Environmental_Improvement = input.Degree_of_Environmental_Improvement;
            entity.Consistency_with_Land_Use_Plans = input.Consistency_with_Land_Use_Plans;
            entity.Pavement_Rehabilitated_or_Reconstructed = input.Pavement_Rehabilitated_or_Reconstructed;
            entity.Bridges_Rehabilitated_or_Replaced = input.Bridges_Rehabilitated_or_Replaced;
            entity.Culverts_Rehabilitated_or_Replaced = input.Culverts_Rehabilitated_or_Replaced;
            entity.Guardrail_Rehabilitated_or_Replaced = input.Guardrail_Rehabilitated_or_Replaced;
            entity.Geotechnical_Assets_Rehabilitated_or_Replaced = input.Geotechnical_Assets_Rehabilitated_or_Replaced;
            entity.Facilities_Rehabilitated_or_Reconstructed = input.Facilities_Rehabilitated_or_Reconstructed;
            entity.Sidewalks_Rehabilitated_or_Reconstructed = input.Sidewalks_Rehabilitated_or_Reconstructed;
            entity.BPN = input.BPN;
            entity.ADT = input.ADT;
            entity.Percent_Trucks = input.Percent_Trucks;
            entity.Population_Density = input.Population_Density;
            entity.Bike_Commute_Share = input.Bike_Commute_Share;
            entity.Length = input.Length;
            entity.Speed_Limit = input.Speed_Limit;
            entity.Detour_Distance = input.Detour_Distance;
            entity.Functional_Classification = input.Functional_Classification;
            entity.National_Highway_System = input.National_Highway_System;
            entity.Pennsylvania_Byway = input.Pennsylvania_Byway;
            entity.Bicycle_PA_Route = input.Bicycle_PA_Route;
            entity.Interstate_Emergency_Detour = input.Interstate_Emergency_Detour;
            entity.LFAR_BOF_Eligible = input.LFAR_BOF_Eligible;
            entity.Treatment = input.Treatment;
            entity.BenefitsComputedAt = input.BenefitsComputedAt;
            entity.Year = input.Year;
            entity.MinYear = input.MinYear;
            entity.MaxYear = input.MaxYear;

            var response = await _workCandidatesRepository.UpdateAsync(entity);
            await _workCandidatesRepository.UpdateWorkCandidate(id);
            await _workCandidatesRepository.UpdatePool(libraryId, false);

            return _mapper.Map(response);
        }
    }
}
