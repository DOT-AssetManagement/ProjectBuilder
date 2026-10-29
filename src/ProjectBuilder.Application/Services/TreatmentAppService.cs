using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Polly;
using ProjectBuilder.Entities;
using ProjectBuilder.Helpers;
using ProjectBuilder.Models;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.Treatment;
using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using ProjectBuilder.Models.UserTreatment.Dtos;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(TreatmentPoolLibraryPermissions.Treatments.Default)]
    public class TreatmentAppService : ApplicationService, ITreatmentAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITreatmentRepository _treatmentRepository;
        // Use ABP default repositories (registered via includeAllEntities:true). The custom
        // IUserTreatmentTypeRepository/IBridgeToPavementsRepository require an unregistered
        // AutoMapper.IMapper in their ctors and fail to activate.
        private readonly IRepository<UserTreatmentType, int> _userTreatmentTypeRepository;
        private readonly IRepository<BridgeToPavements, string> _bridgeToPavementsRepository;
        private readonly IRepository<CargoData> _cargoDataRepository;
        private readonly IRepository<CargoAttribute> _cargoAttributeRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public TreatmentAppService(IHttpContextAccessor httpContextAccessor, ITreatmentRepository treatmentRepository, IRepository<UserTreatmentType, int> userTreatmentTypeRepository, IRepository<BridgeToPavements, string> bridgeToPavementsRepository, IRepository<CargoData> cargoDataRepository, IRepository<CargoAttribute> cargoAttributeRepository, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _treatmentRepository = treatmentRepository;
            _userTreatmentTypeRepository = userTreatmentTypeRepository;
            _bridgeToPavementsRepository = bridgeToPavementsRepository;
            _cargoDataRepository = cargoDataRepository;
            _cargoAttributeRepository = cargoAttributeRepository;
            _objectMapper = objectMapper;
            _mapper = mapper;
        }

        public async Task<List<UserTreatmentDto>> GetByLibraryIdAsync(Guid libraryId, string assetType = null)
        {
            // Remember the active library so subsequent per-row actions (edit/delete),
            // which resolve the library from session, target the right pool — same as
            // the legacy MVC TreatmentsController.Index.
            _httpContextAccessor.HttpContext?.Session.SetString(Constents.LIBRARYIDKEY, libraryId.ToString());

            var model = new TreatmentSearchModel { LibraryId = libraryId };
            if (!string.IsNullOrWhiteSpace(assetType))
            {
                model.AssetType = assetType;
            }

            var treatments = await _treatmentRepository.FilterTreatments(model);
            await EnrichCategoriesAsync(libraryId, treatments);

            // County is a [NotMapped] navigation on the entity, so resolve the name from
            // a county lookup keyed by Cnty id.
            var counties = await _treatmentRepository.GetAllCounties();
            var countyById = counties
                .Where(c => c.Id.HasValue)
                .GroupBy(c => c.Id.Value)
                .ToDictionary(g => g.Key, g => g.First().CountyFullName);

            // Use the Mapperly mapper directly: the generic IObjectMapper doesn't route
            // to ProjectBuilderMapper for this pair, and there's no List -> List overload.
            return treatments
                .Select(t =>
                {
                    var dto = _mapper.Map(t);
                    if (t.CountyId.HasValue && countyById.TryGetValue(t.CountyId.Value, out var countyName))
                    {
                        dto.County = countyName;
                    }
                    return dto;
                })
                .ToList();
        }

        public async Task<List<UserTreatmentDto>> GetByProjectAsync(int scenId, int projectId)
        {
            var model = new TreatmentSearchModel { ScenarioId = scenId, ProjectId = projectId };
            var treatments = await _treatmentRepository.FilterTreatments(model);

            var counties = await _treatmentRepository.GetAllCounties();
            var countyById = counties
                .Where(c => c.Id.HasValue)
                .GroupBy(c => c.Id.Value)
                .ToDictionary(g => g.Key, g => g.First().CountyFullName);

            return treatments
                .Select(t =>
                {
                    var dto = _mapper.Map(t);
                    if (t.CountyId.HasValue && countyById.TryGetValue(t.CountyId.Value, out var countyName))
                        dto.County = countyName;
                    return dto;
                })
                .ToList();
        }

        public async Task<TreatmentSearchModel> LoadSearchModel(Guid libraryId, TreatmentDto searchModel)
        {
            var model = new TreatmentSearchModel
            {
                AssetType = searchModel.AssetType,
                Cnty = searchModel.Id,
                District = searchModel.District,
                Route = searchModel.Route,
                LibraryId = libraryId,
                Direction = searchModel.FilterDirection,
                Year = searchModel.Year
            };

            var sections = searchModel.Section.DecomposeString<int>('-');
            if (sections.Length == 2)
            {
                model.FromSection = sections[0];
                model.ToSection = sections[1];
            }

            return model;
        }
        public async Task<PaginatedDto<UserTreatmentDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var treatmentfilter = JsonConvert.DeserializeObject<TreatmentSearchModel>(httpContext.Items["TreartmentFilter"].ToString());
            var treatments = await _treatmentRepository.FilterTreatments(treatmentfilter);
            var result = _objectMapper.Map<List<UserTreatment>, List<UserTreatmentDto>>(treatments);
            var search_val = (httpContext.Request.Query["sSearch"]).ToString();
            var sort_col = (httpContext.Request.Query["iSortCol_0"]);
            var sort_column = (httpContext.Request.Query["mDataProp_" + sort_col]).ToString();
            var sortDirection = (httpContext.Request.Query["sSortDir_0"]).ToString();

            var propertyInfo = typeof(UserTreatmentModel).GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(sort_column.ToLower()));

            if (search_val != "")
            {
                result = result.Where(
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

            result = sortDirection == "asc" && propertyInfo != null
        ? result.OrderBy(t => propertyInfo.GetValue(t, null)).ToList()
        : result.OrderByDescending(t => propertyInfo.GetValue(t, null)).ToList();

            var displayResult = result
                   .Skip(param.iDisplayStart)
                   .Take(param.iDisplayLength)
                   .ToList();
            var totalRecords = result.Count();

            httpContext.Items["TreartmentFilter"] = JsonConvert.SerializeObject(treatmentfilter);

            return new PaginatedDto<UserTreatmentDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = displayResult
            };
        }   
        [Authorize(TreatmentPoolLibraryPermissions.Treatments.Delete)]
        public async Task<bool> DeleteTreatmentAsync(Guid treatmentId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (!Guid.TryParse(httpContext.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryId))
            {
                return false;
            }

            return await _treatmentRepository.DeleteTreatmentAsync(libraryId, treatmentId);
        }
        [Authorize(TreatmentPoolLibraryPermissions.Treatments.Edit)]
        public async Task<bool> EditTreatmentAsync(UpdateUserTreatmentDto treatmentViewModel)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (!Guid.TryParse(httpContext?.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryId) || libraryId == Guid.Empty)
            {
                throw new UserFriendlyException("Invalid Library ID.");
            }

            // Normalize committed / priority-order on the dto, then build the entity by hand:
            // there is no reverse Mapperly map and the generic IObjectMapper no-ops this pair.
            ValidateTreatmentOnEdit(treatmentViewModel);

            var entity = new UserTreatment
            {
                LibraryId = libraryId,
                Id = treatmentViewModel.Id,
                Risk = treatmentViewModel.Risk,
                Benefit = treatmentViewModel.Benefit,
                Cost = treatmentViewModel.Cost,
                PreferredYear = treatmentViewModel.PreferredYear,
                MinYear = treatmentViewModel.MinYear,
                MaxYear = treatmentViewModel.MaxYear,
                IsCommitted = treatmentViewModel.IsCommitted,
                PriorityOrder = treatmentViewModel.PriorityOrder,
                UserTreatmentTypeNo = treatmentViewModel.UserTreatmentTypeNo,
                IndirectCostDesign = treatmentViewModel.IndirectCostDesign,
                IndirectCostRow = treatmentViewModel.IndirectCostRow,
                IndirectCostOther = treatmentViewModel.IndirectCostOther,
                IndirectCostUtilities = treatmentViewModel.IndirectCostUtilities,
                Interstate = treatmentViewModel.Interstate,
                Direction = treatmentViewModel.Direction,
            };

            // EditTreatments resolves the row by (LibraryId, Id) and marks the scenario stale.
            var edited = await _treatmentRepository.EditTreatments(entity);
            if (edited)
            {
                var categoryName = await ResolveCategoryNameByTypeNoAsync(libraryId, treatmentViewModel.UserTreatmentTypeNo);
                if (!string.IsNullOrWhiteSpace(categoryName) && !string.IsNullOrWhiteSpace(treatmentViewModel.AssetType))
                {
                    await UpsertCategoryAsync(libraryId, treatmentViewModel.Id, treatmentViewModel.AssetType, categoryName);
                }
            }

            return edited;
        }
        private void ValidateTreatmentOnEditing(UpdateUserTreatmentDto treatment)
        {
            var propertiestoIgnore = new string[] { "assettype", "treatment", "district", "county", "route", "section", "interstate", "direction", "bridgeid", "brkey", "projecttreatmentid", "scenarioid" };
            foreach (var key in propertiestoIgnore)
            {
                SetValidationState(key);
            }
            ValidateTreatmentOnEdit(treatment);
        }
        public void SetValidationState(string key, ModelValidationState state = ModelValidationState.Valid)
        {
            var items = _httpContextAccessor.HttpContext?.Items;
            if (items == null || !items.TryGetValue("ModelState", out var value) || value is not ModelStateDictionary modelState)
                return;

            var entry = modelState.GetValueOrDefault(key);
            if (entry != null)
            {
                entry.ValidationState = state;
            }
        }
        private void ValidateTreatmentOnEdit(UpdateUserTreatmentDto treatmentViewModel)
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
            SetValidationState("projecttreatmentid");
            SetValidationState("projectid");
        }
        private void ValidateTreatmentOnCreation(CreateUserTreatmentDto treatmentViewModel)
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
            SetValidationState("projecttreatmentid");
            SetValidationState("projectid");
        }
        public async Task<UserTreatmentDto> GetTreatmentAsync(Guid treatmentId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (!Guid.TryParse(httpContext.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryId))
            {
                throw new UserFriendlyException("Invalid Library ID.");
            }

            // FilterTreatments queries CustomTreatments directly; pick the one matching the id.
            // The generic IObjectMapper does not route to the Mapperly mapper for this pair, so
            // map with _mapper and resolve the county name from the lookup (County is a
            // [NotMapped] navigation that is null at runtime).
            var treatments = await _treatmentRepository.FilterTreatments(new TreatmentSearchModel { LibraryId = libraryId });
            var entity = treatments.FirstOrDefault(t => t.Id == treatmentId);
            if (entity == null)
            {
                throw new UserFriendlyException("Could not find the selected treatment.");
            }

            var dto = _mapper.Map(entity);
            if (entity.CountyId.HasValue)
            {
                var counties = await _treatmentRepository.GetAllCounties();
                dto.County = counties.FirstOrDefault(c => c.Id == entity.CountyId.Value)?.CountyFullName;
            }
            return dto;
        }
        [Authorize(TreatmentPoolLibraryPermissions.Treatments.Create)]
        public async Task<string> CreateTreatment(CreateUserTreatmentDto treatmentViewModel)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (!Guid.TryParse(httpContext?.Session.GetString(Constents.LIBRARYIDKEY), out Guid libraryId) || libraryId == Guid.Empty)
            {
                throw new UserFriendlyException("Invalid Library ID.");
            }

            var treatmentName = (treatmentViewModel.Treatment ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(treatmentName))
            {
                throw new UserFriendlyException("Treatment name is required.");
            }
            if (!treatmentName.Any(char.IsLetter))
            {
                throw new UserFriendlyException("Treatment name must contain at least one letter. Numbers or special characters alone are not allowed.");
            }
            treatmentViewModel.Treatment = treatmentName;

            // Normalizes committed/priority/min-max on the dto.
            ValidateTreatmentOnCreation(treatmentViewModel);

            var entity = BuildTreatmentEntity(treatmentViewModel);
            entity.LibraryId = libraryId;

            // For bridge treatments resolve BMS ID / BRKey from BridgeToPavements; clears them otherwise.
            await ApplyBridgeLookupAsync(entity);

            // CreateUserTreatment dereferences several required .Value members downstream.
            UserTreatmentValidationHelper.ValidateRequiredForCreate(entity);
            entity.Benefit ??= 0;
            entity.Risk ??= 0;

            var treatmentId = await _treatmentRepository.CreateUserTreatment(entity);
            if (treatmentId == null || treatmentId == Guid.Empty)
            {
                throw new UserFriendlyException("Could not create new treatment. Please try again.");
            }

            var categoryName = await ResolveCategoryNameByTypeNoAsync(libraryId, treatmentViewModel.TreatmentType);
            if (!string.IsNullOrWhiteSpace(categoryName) && !string.IsNullOrWhiteSpace(entity.AssetType))
            {
                await UpsertCategoryAsync(libraryId, treatmentId.Value, entity.AssetType, categoryName);
            }

            return "Treatment was created successfully";
        }

        private UserTreatment BuildTreatmentEntity(CreateUserTreatmentDto vm)
        {
            var sections = (vm.Section ?? string.Empty).DecomposeString<int>('-');
            return new UserTreatment
            {
                Benefit = vm.Benefit,
                AssetType = vm.AssetType,
                BridgeId = vm.BridgeId,
                Brkey = vm.Brkey,
                Cost = vm.Cost,
                CountyId = vm.CountyId,
                District = vm.District,
                IndirectCostDesign = vm.IndirectCostDesign,
                IndirectCostRow = vm.IndirectCostRow,
                IndirectCostOther = vm.IndirectCostOther,
                IndirectCostUtilities = vm.IndirectCostUtilities,
                IsCommitted = vm.IsCommitted,
                IsUserTreatment = true,
                UserTreatmentTypeNo = vm.TreatmentType,
                Treatment = vm.Treatment,
                Route = vm.Route,
                Risk = vm.Risk,
                PreferredYear = vm.PreferredYear,
                PriorityOrder = vm.PriorityOrder,
                MaxYear = vm.MaxYear,
                MinYear = vm.MinYear,
                FromSection = sections.Length >= 1 ? sections[0] : (int?)null,
                ToSection = sections.Length == 2 ? sections[1] : (int?)null,
                Interstate = vm.Interstate,
                Direction = vm.Direction.GetValueOrDefault() == 1 ? (byte?)1 : (byte?)0
            };
        }

        // Treatment Type in the library grid is the CargoData "Category" value (import column / modal pick-list).
        private async Task EnrichCategoriesAsync(Guid libraryId, List<UserTreatment> treatments)
        {
            if (treatments == null || treatments.Count == 0)
            {
                return;
            }

            var importIds = treatments.Select(t => t.Id).Distinct().ToList();
            var cargoData = await _cargoDataRepository.GetQueryableAsync();
            var cargoAttributes = await _cargoAttributeRepository.GetQueryableAsync();

            var categories = await AsyncExecuter.ToListAsync(
                cargoData
                    .Join(cargoAttributes,
                        cd => new { cd.LibraryId, cd.AttributeNo, cd.AssetType },
                        ca => new { ca.LibraryId, ca.AttributeNo, ca.AssetType },
                        (cd, ca) => new { cd, ca })
                    .Where(j => j.cd.LibraryId == libraryId
                        && j.ca.AttributeName != null
                        && j.ca.AttributeName.Trim().ToUpper() == "CATEGORY"
                        && importIds.Contains(j.cd.ImportTimeGeneratedGuid))
                    .Select(j => new { j.cd.ImportTimeGeneratedGuid, j.cd.AssetType, j.cd.TextValue }));

            var lookup = categories
                .Where(c => !string.IsNullOrWhiteSpace(c.TextValue))
                .GroupBy(c => (c.ImportTimeGeneratedGuid, c.AssetType))
                .ToDictionary(g => g.Key, g => g.First().TextValue!.Trim());

            foreach (var treatment in treatments)
            {
                if (lookup.TryGetValue((treatment.Id, treatment.AssetType), out var category))
                {
                    treatment.Category = category;
                }
            }
        }

        private async Task<string> ResolveCategoryNameByTypeNoAsync(Guid libraryId, int? typeNo)
        {
            if (!typeNo.HasValue || typeNo.Value == 0)
            {
                return null;
            }

            var types = await GetTreatmentTypesByLibraryAsync(libraryId);
            return types.FirstOrDefault(t => t.Id == typeNo.Value)?.UserTreatmentName?.Trim();
        }

        private async Task UpsertCategoryAsync(Guid libraryId, Guid importTimeGeneratedId, string assetType, string categoryValue)
        {
            if (string.IsNullOrEmpty(assetType))
            {
                return;
            }

            var attributes = await _cargoAttributeRepository.GetQueryableAsync();
            var categoryAttribute = await AsyncExecuter.FirstOrDefaultAsync(
                attributes.Where(a =>
                    a.LibraryId == libraryId
                    && a.AssetType == assetType
                    && a.AttributeName != null
                    && a.AttributeName.Trim().ToUpper() == "CATEGORY"));

            if (categoryAttribute == null)
            {
                return;
            }

            var cargoData = await _cargoDataRepository.GetQueryableAsync();
            var existing = await AsyncExecuter.FirstOrDefaultAsync(
                cargoData.Where(cd =>
                    cd.LibraryId == libraryId
                    && cd.ImportTimeGeneratedGuid == importTimeGeneratedId
                    && cd.AttributeNo == categoryAttribute.AttributeNo
                    && cd.AssetType == assetType));

            if (existing != null)
            {
                existing.TextValue = categoryValue;
                await _cargoDataRepository.UpdateAsync(existing);
            }
            else if (!string.IsNullOrWhiteSpace(categoryValue))
            {
                await _cargoDataRepository.InsertAsync(new CargoData
                {
                    ImportSessionId = Guid.NewGuid(),
                    LibraryId = libraryId,
                    AssetType = assetType,
                    ImportTimeGeneratedGuid = importTimeGeneratedId,
                    AttributeNo = categoryAttribute.AttributeNo,
                    TextValue = categoryValue,
                    NumericValue = null
                });
            }
        }

        private async Task ApplyBridgeLookupAsync(UserTreatment t)
        {
            if (!string.Equals(t.AssetType, "B", StringComparison.OrdinalIgnoreCase))
            {
                t.BridgeId = null;
                t.Brkey = null;
                return;
            }

            t.Brkey = string.IsNullOrWhiteSpace(t.Brkey) ? null : t.Brkey.Trim();

            if (t.Brkey != null)
            {
                var byKey = await FindBridgeMatchByBrkeyAsync(t.Brkey);
                if (byKey != null)
                {
                    t.BridgeId = byKey.BridgeId;
                }
                return;
            }

            if (!t.District.HasValue || !t.CountyId.HasValue || !t.Route.HasValue || !t.FromSection.HasValue)
            {
                return;
            }

            var match = (await FindBridgeMatchesAsync((int)t.District.Value, (int)t.CountyId.Value, t.Route.Value, t.FromSection.Value)).FirstOrDefault();
            if (match != null)
            {
                t.BridgeId = match.BridgeId;
                t.Brkey = string.IsNullOrWhiteSpace(match.Brkey) ? null : match.Brkey.Trim();
            }
        }

        // Feeds the Create modal's live BRKey / BMS ID auto-population as District/County/Route/
        // Section are selected. Returns every bridge at that location (ordered by Offset) so the
        // user can choose among them instead of always getting the first one silently picked.
        public async Task<List<BridgeLookupDto>> GetBridgeInfoAsync(int? district, int? county, int? route, string section)
        {
            if (!district.HasValue || !county.HasValue || !route.HasValue)
            {
                return new List<BridgeLookupDto>();
            }

            var sections = (section ?? string.Empty).DecomposeString<int>('-');
            if (sections.Length == 0)
            {
                return new List<BridgeLookupDto>();
            }

            var matches = await FindBridgeMatchesAsync(district.Value, county.Value, route.Value, sections[0]);
            return matches
                .Select(m => new BridgeLookupDto
                {
                    BridgeId = m.BridgeId,
                    Brkey = string.IsNullOrWhiteSpace(m.Brkey) ? null : m.Brkey.Trim()
                })
                .ToList();
        }

        private async Task<List<BridgeMatchResult>> FindBridgeMatchesAsync(int district, int countyCode, int route, int segment)
        {
            var queryable = await _bridgeToPavementsRepository.GetQueryableAsync();
            return queryable
                .Where(b => b.District == district && b.CountyCode == countyCode && b.Route == route && b.Segment == segment)
                .OrderBy(b => b.OffSet)
                .Select(b => new BridgeMatchResult { BridgeId = b.BridgeId, Brkey = b.Id })
                .ToList();
        }

        // Exact BRKey lookup for a user-supplied/selected value — BRKEY is this table's primary
        // key, so at most one row can match.
        private async Task<BridgeMatchResult> FindBridgeMatchByBrkeyAsync(string brkey)
        {
            var queryable = await _bridgeToPavementsRepository.GetQueryableAsync();
            return queryable
                .Where(b => b.Id == brkey)
                .Select(b => new BridgeMatchResult { BridgeId = b.BridgeId, Brkey = b.Id })
                .FirstOrDefault();
        }

        private class BridgeMatchResult
        {
            public long BridgeId { get; set; }
            public string Brkey { get; set; }
        }

        // Treatment Type options for the Treatments/Index create+edit modals. Unlike the global
        // list below, these are the categories configured for *this* library — the distinct
        // CargoData "Category" values — matching the legacy MVC TreatmentsController
        // (GetCategoryTreatmentTypesAsync). Each category is resolved to its UserTreatmentTypeNo
        // (legacy ResolveUserTreatmentTypeNo) so the existing int-based create/edit binding and
        // the grid's "User Treatment Type" column keep working.
        public async Task<List<UserTreatmentTypeDto>> GetTreatmentTypesByLibraryAsync(Guid libraryId)
        {
            var cargoData = await _cargoDataRepository.GetQueryableAsync();
            var cargoAttributes = await _cargoAttributeRepository.GetQueryableAsync();

            var categories = await AsyncExecuter.ToListAsync(
                cargoData
                    .Join(cargoAttributes,
                        cd => new { cd.LibraryId, cd.AttributeNo, cd.AssetType },
                        ca => new { ca.LibraryId, ca.AttributeNo, ca.AssetType },
                        (cd, ca) => new { cd, ca })
                    .Where(j => j.cd.LibraryId == libraryId
                        && j.ca.AttributeName != null
                        && j.ca.AttributeName.Trim().ToUpper() == "CATEGORY"
                        && j.cd.TextValue != null && j.cd.TextValue != "")
                    .Select(j => j.cd.TextValue.Trim())
                    .Distinct()
                    .OrderBy(t => t));

            // Resolve each category name to its UserTreatmentTypeNo (case-insensitive by name).
            var types = await _userTreatmentTypeRepository.GetListAsync();
            var idByName = types
                .Where(t => !string.IsNullOrWhiteSpace(t.UserTreatmentName))
                .GroupBy(t => t.UserTreatmentName.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Id).First().Id);

            return categories
                .Select(c => new UserTreatmentTypeDto
                {
                    Id = idByName.TryGetValue(c.ToLowerInvariant(), out var no) ? no : 0,
                    UserTreatmentName = c
                })
                .ToList();
        }

        public async Task<List<UserTreatmentTypeDto>> GetTreatmentTypesAsync()
        {
            var types = await _userTreatmentTypeRepository.GetListAsync();
            return types
                .Where(t => !string.IsNullOrWhiteSpace(t.UserTreatmentName))
                .OrderBy(t => t.UserTreatmentName)
                .Select(t => _mapper.Map(t))
                .ToList();
        }

        // Unlike Treatment (a free-text string), TreatmentType is a required int FK to
        // tbl_pb_UserTreatments, so a pool with no imported categories yet has nothing selectable
        // and no purely client-side "type a new name" trick can work -- a new name needs a real
        // row/id. Case-insensitive lookup-or-create so the create/edit modals can offer the same
        // "Create New Treatment Type" affordance the Treatment field already has.
        public async Task<UserTreatmentTypeDto> GetOrCreateTreatmentTypeAsync(string name)
        {
            name = (name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new UserFriendlyException("Treatment type name is required.");
            }
            if (!name.Any(char.IsLetter))
            {
                throw new UserFriendlyException("Treatment type must contain at least one letter. Numbers or special characters alone are not allowed.");
            }

            var types = await _userTreatmentTypeRepository.GetListAsync();
            var existing = types.FirstOrDefault(t => string.Equals(t.UserTreatmentName?.Trim(), name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return _mapper.Map(existing);
            }

            var created = await _userTreatmentTypeRepository.InsertAsync(new UserTreatmentType { UserTreatmentName = name }, autoSave: true);
            return _mapper.Map(created);
        }

        public Task<List<byte?>> GetDistrictsAsync()
        {
            return _treatmentRepository.GetDistricts();
        }

        public async Task<List<CountyDto>> GetCountiesAsync(int? district)
        {
            var counties = await _treatmentRepository.GetCounties(district);
            return counties.Select(c => _mapper.Map(c)).ToList();
        }

        public Task<List<int?>> GetRoutesAsync(int? district, int? county)
        {
            return _treatmentRepository.GetRoutes(district, county);
        }

        public Task<List<string>> GetSectionsAsync(int? district, int? county, int? route)
        {
            return _treatmentRepository.GetSections(district, county, route);
        }

        public async Task<DefaultSlackDto> GetDefaultSlacksAsync(string assetType)
        {
            if (string.IsNullOrWhiteSpace(assetType))
            {
                return null;
            }
            var slack = await _treatmentRepository.GetAssetTypeDefaultSlack(assetType);
            return slack == null ? null : _mapper.Map(slack);
        }

    }
}
