using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.ProjectTreatment;
using ProjectBuilder.Models.ProjectTreatment.Dtos;
using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(ScenarioPermissions.Default)]
    public class ProjectTreatmentAppService : ApplicationService, IProjectTreatmentAppService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProjectTreatmentRepository _projectTreatmentRepository;
        private readonly IRunScenarioRepository _runScenarioRepository;
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IRepository<UserTreatmentType, int> _userTreatmentTypeRepository;

        public ProjectTreatmentAppService(IHttpContextAccessor httpContextAccessor, IProjectTreatmentRepository projectTreatmentRepository, IRunScenarioRepository runScenarioRepository, ITreatmentRepository treatmentRepository, IRepository<UserTreatmentType, int> userTreatmentTypeRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _projectTreatmentRepository = projectTreatmentRepository;
            _runScenarioRepository = runScenarioRepository;
            _treatmentRepository = treatmentRepository;
            _userTreatmentTypeRepository = userTreatmentTypeRepository;
        }

        //public async Task<ActionResult> ApplyPagination(JqueryDatatableParamDto param)
        //{
        //    var treatments = await LoadProjectTreatments();
        //    var search_val = (Request.Query["sSearch"]).ToString();
        //    var sort_col = (Request.Query["iSortCol_0"]);
        //    var sort_column = (Request.Query["mDataProp_" + sort_col]).ToString();
        //    var sortDirection = (Request.Query["sSortDir_0"]).ToString();
        //    var propertyInfo = typeof(ProjectTreatmentModel).GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(sort_column.ToLower()));
        //    if (search_val != "")
        //    {
        //        treatments = treatments.Where(
        //            t => t.Section.ToLower().ToString().Contains(search_val.ToLower()) ||
        //            (t.AssetType == "P" && "Pavement".ToLower().Contains(search_val.ToLower())) ||
        //            (t.AssetType == "B" && "Bridge".ToLower().Contains(search_val.ToLower())) ||
        //            t.District.ToString().Contains(search_val.ToLower()) ||
        //            t.County.ToLower().ToString().Contains(search_val.ToLower()) ||
        //            t.TreatmentType.ToLower().ToString().Contains(search_val.ToLower()) ||
        //            t.BridgeId.ToString().Contains(search_val.ToLower()) ||
        //            t.Route.ToString().Contains(search_val.ToLower()) ||
        //            t.Interstate.ToString().Contains(search_val.ToLower()) ||
        //            t.Direction.ToString().Contains(search_val.ToLower()) ||
        //            Math.Round((decimal)t.TotalCost / 1000000, 2).ToString().Contains(search_val.ToLower()) ||
        //            Math.Round((decimal)t.Benefit / 1000000, 2).ToString().Contains(search_val.ToLower()) ||
        //            Math.Round((decimal)t.Risk, 2).ToString().Contains(search_val.ToLower()) ||
        //            t.PreferredYear.ToString().Contains(search_val.ToLower()) ||
        //            t.MinYear.ToString().Contains(search_val.ToLower()) ||
        //            t.MaxYear.ToString().Contains(search_val.ToLower()) ||
        //            t.IndirectCostDesign.ToString().Contains(search_val.ToLower()) ||
        //            t.IndirectCostRow.ToString().Contains(search_val.ToLower()) ||
        //            t.IndirectCostUtilities.ToString().Contains(search_val.ToLower()) ||
        //            t.IndirectCostOther.ToString().Contains(search_val.ToLower())
        //            ).ToList();
        //    }
        //    treatments = sortDirection == "asc" && propertyInfo != default
        //        ? treatments.OrderBy(t => propertyInfo.GetValue(t, null))
        //        : treatments.OrderByDescending(t => propertyInfo.GetValue(t, null));
        //    var displayResult = treatments.Skip(param.iDisplayStart).Take(param.iDisplayLength);
        //    var totalRecords = treatments.Count();
        //    return Json(new
        //    {
        //        param.sEcho,
        //        iTotalRecords = totalRecords,
        //        iTotalDisplayRecords = totalRecords,
        //        aaData = displayResult
        //    });
        //}
        //private async Task<IEnumerable<ProjectTreatmentModel>> LoadProjectTreatments()
        //{

        //    var hasscenid = int.TryParse(TempData.Peek("ScenId")?.ToString(), out int scenid);
        //    var hasprojectid = int.TryParse(TempData.Peek("ProjectId")?.ToString(), out int projectid);
        //    if (!hasscenid || !hasprojectid)
        //        return Enumerable.Empty<ProjectTreatmentModel>();
        //    var filterdata = TempData.Peek("Filter");
        //    var filter = filterdata == null ? new() : JsonConvert.DeserializeObject<TreatmentSearchModel>(filterdata.ToString());
        //    filter.ScenarioId = scenid;
        //    filter.ProjectId = projectid;
        //    return await _filterUnitOfWork.ProjectTreatmentRepo.FilterProjectTreatments(filter);
        //}

        public async Task<PaginatedDto<ProjectTreatmentDto>> ApplyPagination(JqueryDatatableParamDto param, int scenarioId, int projectId, string filterJson = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var treatments = await LoadProjectTreatments(scenarioId, projectId, filterJson);
            var search_val = (httpContext.Request.Query["sSearch"]).ToString();
            var sort_col = (httpContext.Request.Query["iSortCol_0"]);
            var sort_column = (httpContext.Request.Query["mDataProp_" + sort_col]).ToString();
            var sortDirection = (httpContext.Request.Query["sSortDir_0"]).ToString();
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
            var displayResult = treatments.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            var totalRecords = treatments.Count();
            return new PaginatedDto<ProjectTreatmentDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = displayResult
            };
        }
        public async Task<IEnumerable<ProjectTreatmentDto>> LoadProjectTreatments(int scenarioId, int projectId, string filterJson = null)
        {
            if (scenarioId <= 0 || projectId <= 0)
            {
                return Enumerable.Empty<ProjectTreatmentDto>();
            }

            var filter = string.IsNullOrEmpty(filterJson)
                ? new TreatmentSearchModel()
                : JsonConvert.DeserializeObject<TreatmentSearchModel>(filterJson);

            filter.ScenarioId = scenarioId;
            filter.ProjectId = projectId;
            // The map's project detail view (right-side panel + this bottom editor bar) should
            // show a project's treatments the same way regardless of whether the scenario run
            // funded it — unlike the scenario-wide report below, which stays selected-only.
            filter.IncludeUnselected = true;

            var projectTreatments = await _projectTreatmentRepository.FilterProjectTreatments(filter);

            return projectTreatments.Select(ToDto).ToList();
        }

        // Scenario-wide treatment list (all projects) — backs /ProjectTreatments/List,
        // the legacy "Project Treatments" flat view linked from the Projects list page.
        public async Task<IEnumerable<ProjectTreatmentDto>> LoadScenarioTreatments(int scenarioId, string filterJson = null)
        {
            if (scenarioId <= 0)
            {
                return Enumerable.Empty<ProjectTreatmentDto>();
            }

            var filter = string.IsNullOrEmpty(filterJson)
                ? new TreatmentSearchModel()
                : JsonConvert.DeserializeObject<TreatmentSearchModel>(filterJson);

            filter.ScenarioId = scenarioId;

            var projectTreatments = await _projectTreatmentRepository.FilterProjectTreatments(filter);

            return projectTreatments.Select(ToDto).ToList();
        }

        // Server-paginated version of LoadScenarioTreatments — backs the /ProjectTreatments/List
        // grid's remote Tabulator pagination so it no longer has to load the whole scenario up front.
        public async Task<ScenarioTreatmentsPageDto> GetScenarioTreatmentsPageAsync(
            int scenarioId, string assetType, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, string sortDir, int page, int pageSize)
        {
            var (items, totalCount) = await _projectTreatmentRepository.GetPagedScenarioTreatmentsAsync(
                scenarioId, assetType, district, countyId, route, section, year,
                sortField, string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase),
                page, pageSize);

            return new ScenarioTreatmentsPageDto { Data = items.Select(ToDto).ToList(), TotalCount = totalCount };
        }
        [Authorize(ScenarioPermissions.Analysis.ProjectTreatments.Delete)]
        public async Task<bool> DeleteProjectTreatment(long? treatmentid, TreatmentDto treatmentViewModel)
        {
            // Loaded before the delete so we still have the link to the library row afterwards.
            var target = await _projectTreatmentRepository.FindAsync(treatmentid);

            var success = await _projectTreatmentRepository.DeleteAsync(treatmentid);
            if (success)
            {
                _projectTreatmentRepository.UpdateScenarioNotes(treatmentViewModel.ScenarioId);

                // Remove the linked library treatment too, so a scenario rerun doesn't
                // regenerate the very treatment the user just deleted.
                if (target?.ImportTimeGeneratedId != null)
                {
                    await DeleteLinkedLibraryTreatmentAsync(target.ScenarioId, target.ImportTimeGeneratedId.Value);
                }
            }
            if (!success)
            {
                throw new Exception("Could not delete the selected Project Treatment because it was not found.");
            }

            return true;
        }

        // No library sync: project assignment is re-derived by the optimizer, so a move only holds until the next run.
        [Authorize(ScenarioPermissions.Analysis.ProjectTreatments.Edit)]
        public async Task<bool> MoveProjectTreatment(MoveProjectTreatmentDto input)
        {
            var treatment = await _projectTreatmentRepository.FindAsync(input.TreatmentId);
            if (treatment == null || treatment.ScenarioId != input.ScenarioId)
            {
                throw new UserFriendlyException("Could not find the selected treatment in this scenario.");
            }

            if (treatment.ProjectId == input.TargetProjectId)
            {
                throw new UserFriendlyException("The treatment already belongs to that project.");
            }

            // Null means the project isn't in the scenario at all; either funding status is a valid
            // destination, so a treatment can move funded -> unfunded as well as unfunded -> funded.
            var targetFundingStatus = await _projectTreatmentRepository.IsProjectFundedAsync(input.ScenarioId, input.TargetProjectId);
            if (targetFundingStatus == null)
            {
                throw new UserFriendlyException("Could not find the destination project in this scenario.");
            }

            var moved = await _projectTreatmentRepository.MoveTreatmentToProjectAsync(
                input.ScenarioId, input.TreatmentId, input.TargetProjectId);
            if (!moved)
            {
                throw new UserFriendlyException("Could not move the treatment. Please try again.");
            }

            await _projectTreatmentRepository.UpdateScenarioNotes(input.ScenarioId);
            return true;
        }

        private async Task DeleteLinkedLibraryTreatmentAsync(int? scenarioId, Guid importTimeGeneratedId)
        {
            var scenario = await _runScenarioRepository.FindScenarioByIdAsync(scenarioId);
            if (scenario?.LibraryId == null)
            {
                return;
            }

            await _treatmentRepository.DeleteTreatmentAsync(scenario.LibraryId.Value, importTimeGeneratedId);
        }
        [Authorize(ScenarioPermissions.Analysis.ProjectTreatments.Edit)]
        public async Task<bool> EditProjectTreatment(UpdateProjectTreatmentDto treatmentViewModel)
        {
            ValidateTreatmentOnEditing(treatmentViewModel);

            // Loaded before the edit so we know whether this row already has a real library
            // link (ImportTimeGeneratedId) to sync into, or needs one created (self-heal).
            var target = await _projectTreatmentRepository.FindAsync(treatmentViewModel.Id);

            var entity = new ProjectTreatment
            {
                Id = treatmentViewModel.Id,
                Risk = treatmentViewModel.Risk,
                Benefit = treatmentViewModel.Benefit,
                DirectCost = treatmentViewModel.DirectCost,
                PreferredYear = treatmentViewModel.PreferredYear,
                MinYear = treatmentViewModel.MinYear,
                MaxYear = treatmentViewModel.MaxYear,
                IsCommitted = treatmentViewModel.IsCommitted,
                PriorityOrder = treatmentViewModel.PriorityOrder,
                UserTreatmentTypeNo = treatmentViewModel.UserTreatmentTypeNo,
                IndirectCostUtilities = treatmentViewModel.IndirectCostUtilities,
                IndirectCostDesign = treatmentViewModel.IndirectCostDesign,
                IndirectCostOther = treatmentViewModel.IndirectCostOther,
                IndirectCostRow = treatmentViewModel.IndirectCostRow,
                IsUserCreated = treatmentViewModel.IsUserCreated,
                Interstate = treatmentViewModel.Interstate,
                Direction = treatmentViewModel.Direction,
            };
            var success = await _projectTreatmentRepository.EditProjectTreatments(entity, treatmentViewModel.Category);
            if (success)
            {
                await _projectTreatmentRepository.UpdateScenarioNotes(treatmentViewModel.ScenarioId);

                if (target != null)
                {
                    await SyncLibraryTreatmentOnEditAsync(target, treatmentViewModel);
                }
            }
            return true;
        }

        // Writes the same edited fields through to the scenario's tbl_lib_LibraryTreatments row
        // so the change survives a scenario rerun/regeneration instead of being lost.
        private async Task SyncLibraryTreatmentOnEditAsync(ProjectTreatment target, UpdateProjectTreatmentDto treatmentViewModel)
        {
            var scenario = await _runScenarioRepository.FindScenarioByIdAsync(target.ScenarioId);
            if (scenario?.LibraryId == null)
            {
                return;
            }
            var libraryId = scenario.LibraryId.Value;

            if (target.ImportTimeGeneratedId == null)
            {
                // Never linked to a library row (legacy data predating this feature) — create
                // one now using the treatment's (unchanged) location fields so future edits
                // have a real row to sync into.
                int.TryParse(target.Section, out var fromSection);
                var resolvedTypeNo = await UserTreatmentValidationHelper.ResolveUserTreatmentTypeNoByCategoryAsync(_userTreatmentTypeRepository, treatmentViewModel.Category);
                var newLibraryTreatment = new UserTreatment
                {
                    LibraryId = libraryId,
                    AssetType = target.AssetType,
                    Treatment = target.TreatmentType,
                    UserTreatmentTypeNo = resolvedTypeNo,
                    District = target.District,
                    CountyId = target.CountyId,
                    Route = target.Route,
                    FromSection = fromSection,
                    ToSection = fromSection,
                    Brkey = target.Brkey,
                    BridgeId = target.BridgeId,
                    Interstate = treatmentViewModel.Interstate ?? target.Interstate,
                    Direction = treatmentViewModel.Direction ?? target.Direction,
                    Cost = treatmentViewModel.DirectCost,
                    Benefit = treatmentViewModel.Benefit ?? 0,
                    Risk = treatmentViewModel.Risk ?? 0,
                    PreferredYear = treatmentViewModel.PreferredYear,
                    MinYear = treatmentViewModel.MinYear,
                    MaxYear = treatmentViewModel.MaxYear,
                    PriorityOrder = (byte?)treatmentViewModel.PriorityOrder,
                    IsCommitted = treatmentViewModel.IsCommitted,
                    IndirectCostDesign = treatmentViewModel.IndirectCostDesign,
                    IndirectCostRow = treatmentViewModel.IndirectCostRow,
                    IndirectCostUtilities = treatmentViewModel.IndirectCostUtilities,
                    IndirectCostOther = treatmentViewModel.IndirectCostOther,
                    IsUserTreatment = true,
                };

                try
                {
                    UserTreatmentValidationHelper.ValidateRequiredForCreate(newLibraryTreatment);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Could not create a linked library treatment for ProjectTreatment {target.Id}: {ex.Message}");
                    return;
                }

                var newId = await _treatmentRepository.CreateUserTreatment(newLibraryTreatment);
                if (newId.HasValue && newId != Guid.Empty)
                {
                    await _projectTreatmentRepository.SetImportTimeGeneratedId(target.Id, newId.Value);
                }
                return;
            }

            // Already linked — update the existing library row. If it's gone (e.g. deleted
            // directly from the Treatment Library page), don't silently recreate it: that would
            // resurrect something a user deliberately removed elsewhere.
            var linkedTypeNo = await UserTreatmentValidationHelper.ResolveUserTreatmentTypeNoByCategoryAsync(_userTreatmentTypeRepository, treatmentViewModel.Category);
            var libraryTreatment = new UserTreatment
            {
                LibraryId = libraryId,
                Id = target.ImportTimeGeneratedId.Value,
                Risk = treatmentViewModel.Risk,
                Benefit = treatmentViewModel.Benefit,
                Cost = treatmentViewModel.DirectCost,
                PreferredYear = treatmentViewModel.PreferredYear,
                MinYear = treatmentViewModel.MinYear,
                MaxYear = treatmentViewModel.MaxYear,
                IsCommitted = treatmentViewModel.IsCommitted,
                PriorityOrder = (byte?)treatmentViewModel.PriorityOrder,
                UserTreatmentTypeNo = linkedTypeNo,
                IndirectCostDesign = treatmentViewModel.IndirectCostDesign,
                IndirectCostRow = treatmentViewModel.IndirectCostRow,
                IndirectCostOther = treatmentViewModel.IndirectCostOther,
                IndirectCostUtilities = treatmentViewModel.IndirectCostUtilities,
                Interstate = treatmentViewModel.Interstate,
                Direction = treatmentViewModel.Direction,
            };

            var updated = await _treatmentRepository.EditTreatments(libraryTreatment);
            if (!updated)
            {
                Logger.LogWarning($"ProjectTreatment {target.Id} references library treatment {target.ImportTimeGeneratedId} in library {libraryId}, but no matching row was found. Skipping library sync.");
            }
        }

        public async Task<ProjectTreatmentDto> CreateProjectTreatment(CreateProjectTreatmentDto input)
        {
            // Create the library treatment FIRST: tbl_pb_ExtendedImportedTreatments is written
            // via a single raw INSERT (not a tracked EF Add), so doing this the other way round
            // would need a second UPDATE statement. The returned id becomes the correlation key
            // (ProjectTreatment.ImportTimeGeneratedId) future edits/deletes use to find this row
            // back in the library.
            var scenario = await _runScenarioRepository.FindScenarioByIdAsync(input.ScenarioId);
            if (scenario?.LibraryId == null)
            {
                throw new UserFriendlyException("Could not resolve the treatment library for this scenario.");
            }
            var libraryId = scenario.LibraryId.Value;

            UserTreatmentValidationHelper.ApplyCommittedDefaults(input);

            int.TryParse(input.Section, out var fromSection);
            var createTypeNo = await UserTreatmentValidationHelper.ResolveUserTreatmentTypeNoByCategoryAsync(_userTreatmentTypeRepository, input.Category);
            var libraryTreatment = new UserTreatment
            {
                LibraryId = libraryId,
                AssetType = input.AssetType,
                Treatment = input.TreatmentType,
                UserTreatmentTypeNo = createTypeNo,
                District = input.District,
                CountyId = input.CountyId,
                Route = input.Route,
                FromSection = fromSection,
                ToSection = fromSection,
                Brkey = input.Brkey,
                BridgeId = input.BridgeId,
                Interstate = input.Interstate,
                Direction = input.Direction,
                Cost = input.DirectCost,
                Benefit = input.Benefit ?? 0,
                Risk = input.Risk ?? 0,
                PreferredYear = input.PreferredYear,
                MinYear = input.MinYear,
                MaxYear = input.MaxYear,
                PriorityOrder = (byte?)input.PriorityOrder,
                IsCommitted = input.IsCommitted ?? false,
                IndirectCostDesign = input.IndirectCostDesign,
                IndirectCostRow = input.IndirectCostRow,
                IndirectCostUtilities = input.IndirectCostUtilities,
                IndirectCostOther = input.IndirectCostOther,
                IsUserTreatment = true,
            };

            UserTreatmentValidationHelper.ValidateRequiredForCreate(libraryTreatment);

            var importTimeGeneratedId = await _treatmentRepository.CreateUserTreatment(libraryTreatment);
            if (importTimeGeneratedId == null || importTimeGeneratedId == Guid.Empty)
            {
                throw new UserFriendlyException("Could not create the linked library treatment. Please try again.");
            }

            var entity = new ProjectTreatment
            {
                ScenarioId = input.ScenarioId,
                ProjectId = input.ProjectId,
                AssetType = input.AssetType,
                District = input.District,
                CountyId = input.CountyId,
                County = input.County,
                Route = input.Route,
                Direction = input.Direction,
                Section = input.Section,
                Brkey = input.Brkey,
                BridgeId = input.BridgeId,
                Interstate = input.Interstate,
                TreatmentType = input.TreatmentType,
                TotalCost = input.TotalCost,
                DirectCost = input.DirectCost,
                IndirectCostDesign = input.IndirectCostDesign,
                IndirectCostRow = input.IndirectCostRow,
                IndirectCostUtilities = input.IndirectCostUtilities,
                IndirectCostOther = input.IndirectCostOther,
                Benefit = input.Benefit,
                Risk = input.Risk,
                IsCommitted = input.IsCommitted,
                PriorityOrder = input.PriorityOrder,
                IsUserCreated = true,
                PreferredYear = input.PreferredYear,
                MinYear = input.MinYear,
                MaxYear = input.MaxYear,
                SelectedYear = input.SelectedYear,
                UserTreatmentTypeNo = input.UserTreatmentTypeNo,
                ImportTimeGeneratedId = importTimeGeneratedId,
            };

            ProjectTreatment created;
            try
            {
                created = await _projectTreatmentRepository.CreateProjectTreatment(entity, input.Category);
            }
            catch
            {
                // Compensating rollback: the library insert (DataManager's own raw ADO.NET
                // connection) and this insert (EF ExecuteSqlInterpolatedAsync) are not part of
                // the same transaction, so a failure here would otherwise leave an orphaned
                // library row with no linked project treatment.
                await _treatmentRepository.DeleteTreatmentAsync(libraryId, importTimeGeneratedId.Value);
                throw;
            }

            await _projectTreatmentRepository.UpdateScenarioNotes(input.ScenarioId);

            return ToDto(created);
        }

        public async Task<List<string>> GetTreatmentCategoriesAsync(int scenarioId, string assetType = null)
        {
            return await _projectTreatmentRepository.GetTreatmentTypes(scenarioId, assetType);
        }

        public async Task<List<string>> GetTreatmentTypeCategoriesAsync(int scenarioId, string assetType)
        {
            return await _projectTreatmentRepository.GetTreatmentCategoryOptionsAsync(scenarioId, assetType);
        }

        // Scenario-scoped location cascade for the Create modal — the same source legacy uses
        // (the scenario's own project treatments), so every district/county/route offered has
        // sections available to pick. Filtered reads off the treatments view are fast.
        public Task<List<byte?>> GetScenarioDistrictsAsync(int scenarioId)
        {
            return _projectTreatmentRepository.GetDistricts(scenarioId);
        }

        public async Task<List<CountyDto>> GetScenarioCountiesAsync(int scenarioId, int district)
        {
            var counties = await _projectTreatmentRepository.GetCounties(scenarioId, district);
            return counties
                .Select(c => new CountyDto { Id = c.Id, CountyName = c.CountyName, District = c.District })
                .ToList();
        }

        public Task<List<int?>> GetScenarioRoutesAsync(int scenarioId, int district, int county)
        {
            return _projectTreatmentRepository.GetRoutes(scenarioId, district, county);
        }

        public Task<List<string>> GetScenarioSectionsAsync(int scenarioId, int district, int county, int route)
        {
            return _projectTreatmentRepository.GetSections(scenarioId, district, county, route);
        }

        public Task<List<int?>> GetScenarioYearsAsync(int scenarioId)
        {
            return _projectTreatmentRepository.GetYears(scenarioId);
        }

        public async Task<List<SectionRangeDto>> GetScenarioSectionRangesAsync(int scenarioId, int district, int county, int route)
        {
            var sections = await _projectTreatmentRepository.GetSectionRanges(scenarioId, district, county, route);
            return sections
                .Select(s => new SectionRangeDto { Section = s.Section, FromSection = s.FromSection, ToSection = s.ToSection })
                .ToList();
        }

        public Task<List<int?>> GetMasterDistrictsAsync(string assetType)
        {
            return _projectTreatmentRepository.GetMasterDistricts(assetType);
        }

        public async Task<List<CountyDto>> GetMasterCountiesAsync(string assetType, int district)
        {
            var counties = await _projectTreatmentRepository.GetMasterCounties(assetType, district);
            return counties
                .Select(c => new CountyDto { Id = c.Id, CountyName = c.CountyName, District = c.District })
                .ToList();
        }

        public Task<List<int?>> GetMasterRoutesAsync(string assetType, int district, int county)
        {
            return _projectTreatmentRepository.GetMasterRoutes(assetType, district, county);
        }

        public async Task<List<SectionRangeDto>> GetMasterSectionRangesAsync(string assetType, int district, int county, int route)
        {
            var sections = await _projectTreatmentRepository.GetMasterSectionRanges(assetType, district, county, route);
            return sections
                .Select(s => new SectionRangeDto { Section = s.Section, FromSection = s.FromSection, ToSection = s.ToSection })
                .ToList();
        }

        public async Task<List<BridgeCandidateDto>> GetMasterBridgeCandidatesAsync(int district, int county, int route, string section)
        {
            var candidates = await _projectTreatmentRepository.GetMasterBridgeCandidates(district, county, route, section);
            return candidates
                .Select(c => new BridgeCandidateDto { BrKey = c.BrKey, BridgeId = c.BridgeId })
                .ToList();
        }

        private void ValidateTreatmentOnEditing(UpdateProjectTreatmentDto treatment)
        {
            var propertiestoIgnore = new string[] { "assettype" , "treatment" , "district" , "county" , "route" , "section", "interstate", "direction",
               "bridgeid","brkey","projectId","treatmentid","scenarioid"};
            foreach (var key in propertiestoIgnore)
            {
                SetValidationState(key);
            }
            ValidateGeneratedValues(treatment);
        }
        private void SetValidationState(string key, ModelValidationState state = ModelValidationState.Valid)
        {
            var items = _httpContextAccessor.HttpContext?.Items;
            if (items == null || !items.TryGetValue("ModelState", out var value) || value is not ModelStateDictionary modelState)
                return;

            var entry = modelState.GetValueOrDefault(key);
            if (entry != null)
                entry.ValidationState = state;
        }
        private void ValidateGeneratedValues(UpdateProjectTreatmentDto treatmentViewModel)
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
        public async Task<bool> CheckRunAvailAbilty(int? scenarioId)
        {
            try
            {
                var check = await _projectTreatmentRepository.CheckIsUserCreated(scenarioId);
                return check;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ProjectTreatmentDto> GetProjectTreatment(long? projectTreatmentId)
        {
            var target = await _projectTreatmentRepository.FindAsync(projectTreatmentId);
            if (target == null)
            {
                throw new Exception("could not find the selected treatment");
            }
            return ToDto(target);
        }

        private static ProjectTreatmentDto ToDto(ProjectTreatment entity)
        {
            return new ProjectTreatmentDto
            {
                Id = entity.Id,
                ScenarioId = entity.ScenarioId,
                ProjectId = entity.ProjectId,
                AssetType = entity.AssetType,
                District = entity.District,
                CountyId = entity.CountyId,
                County = entity.County,
                Route = entity.Route,
                Direction = entity.Direction,
                Section = entity.Section,
                FromSection = entity.FromSection,
                ToSection = entity.ToSection,
                Brkey = entity.Brkey,
                BridgeId = entity.BridgeId,
                Interstate = entity.Interstate,
                TreatmentType = entity.TreatmentType,
                TotalCost = entity.TotalCost,
                DirectCost = entity.DirectCost,
                IndirectCostDesign = entity.IndirectCostDesign,
                IndirectCostRow = entity.IndirectCostRow,
                IndirectCostUtilities = entity.IndirectCostUtilities,
                IndirectCostOther = entity.IndirectCostOther,
                Benefit = entity.Benefit,
                Risk = entity.Risk,
                IsCommitted = entity.IsCommitted,
                PriorityOrder = entity.PriorityOrder,
                IsUserCreated = entity.IsUserCreated,
                PreferredYear = entity.PreferredYear,
                MinYear = entity.MinYear,
                MaxYear = entity.MaxYear,
                SelectedYear = entity.SelectedYear,
                UserTreatmentTypeNo = entity.UserTreatmentTypeNo,
                UserTreatmentName = entity.UserTreatmentName,
                Category = entity.Category,
            };
        }
        public Task<ProjectTreatmentDto> CreateAsync(CreateProjectTreatmentDto input)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public Task<ProjectTreatmentDto> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }
        public Task<PagedResultDto<ProjectTreatmentDto>> GetListAsync(GetProjectTreatmentInput input)
        {
            throw new NotImplementedException();
        }
        public Task<ProjectTreatmentDto> UpdateAsync(Guid id, UpdateProjectTreatmentDto input)
        {
            throw new NotImplementedException();
        }
    }
}
