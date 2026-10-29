using AutoMapper;
using AutoMapper.QueryableExtensions;
using log4net.Repository.Hierarchy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ProjectBuilder
{ 
        public class ProjectTreatmentRepository : EfCoreRepository<ProjectBuilderDbContext, ProjectTreatment, long>, IProjectTreatmentRepository
        {
            private readonly ProjectBuilderDbContext _context;

            public ProjectTreatmentRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ILogger<EfCoreRepository<ProjectBuilderDbContext, ProjectTreatment, long>> logger, ProjectBuilderDbContext dbContext)
                                : base(projectBuilderDbContext)
            {
                _context = dbContext;
            }
            //protected override IQueryable<ProjectTreatment> InitializeCurrrentQuery(Expression<Func<ProjectTreatment, bool>> filter = null)
            //{
            //    if (filter is null)
            //        filter = PredicateBuilder.New<ProjectTreatment>(true);
            //    return ProjectBuilderDbContext.ProjectTreatments.Where(filter)
            //                                             .OrderBy(t => t.EntityId)
            //                                             .Join(ProjectBuilderDbContext.Counties,t => t.CountyId,c => c.EntityId,(t,c) => new { treatment = t,county = c })
            //                                             .GroupJoin(ProjectBuilderDbContext.UserTreatmentTypes,t => t.treatment.UserTreatmentTypeNo,ty => ty.EntityId,(t,ty) =>new { t.treatment,t.county, type = ty})
            //                                             .SelectMany(ty=> ty.type.DefaultIfEmpty(),(tr,ty) => new ProjectTreatment
            //                                             {
            //                                                 ScenarioId = tr.treatment.ScenarioId,
            //                                                 EntityId = tr.treatment.EntityId,
            //                                                 District = tr.treatment.District,
            //                                                 AssetType = tr.treatment.AssetType,
            //                                                 CountyId = tr.treatment.CountyId
            //                                                 Route = tr.treatment.Route,
            //                                                 TreatmentType = tr.treatment.TreatmentType,
            //                                                 Benefit = tr.treatment.Benefit,
            //                                                 Interstate = tr.treatment.Interstate,
            //                                                 IsCommitted = tr.treatment.IsCommitted,
            //                                                 Cost = tr.treatment.Cost,
            //                                                 Risk = tr.treatment.Risk,
            //                                                 PriorityOrder = tr.treatment.PriorityOrder,
            //                                                 PreferredYear = tr.treatment.PreferredYear,
            //                                                 MinYear = tr.treatment.MinYear,
            //                                                 MaxYear = tr.treatment.MaxYear,
            //                                                 FromSection = tr.treatment.FromSection,
            //                                                 ToSection = tr.treatment.ToSection,
            //                                                 Direction = tr.treatment.Direction,
            //                                                 BridgeId = tr.treatment.BridgeId,
            //                                                 Brkey = tr.treatment.Brkey,
            //                                                 IndirectCostDesign = tr.treatment.IndirectCostDesign,
            //                                                 IndirectCostOther = tr.treatment.IndirectCostOther,
            //                                                 IndirectCostRow = tr.treatment.IndirectCostRow,
            //                                                 IndirectCostUtilities = tr.treatment.IndirectCostUtilities,
            //                                                 IsUserTreatment = tr.treatment.IsUserTreatment,
            //                                                 UserTreatmentTypeNo = tr.treatment.UserTreatmentTypeNo,
            //                                                 ProjectId = tr.treatment.ProjectId,
            //                                                 AssignedSpatiallyConstrainedProjectId = tr.treatment.AssignedSpatiallyConstrainedProjectId,
            //                                                 AssignedUnconstrainedProjectId = tr.treatment.AssignedUnconstrainedProjectId,
            //                                                 ImportTimeGeneratedId = tr.treatment.ImportTimeGeneratedId,
            //                                                 UserTreatmentName = ty.UserTreatmentName
            //                                             }).AsNoTracking();
            //}

            #region Additional Queries

            public async Task<bool> DeleteProjectTreatments(long modelId)
        {
            var target = await _context.ProjectTreatments.FirstOrDefaultAsync(x => x.Id == modelId);
            if (target is null)
                return false;
            _context.ProjectTreatments.Remove(target);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectTreatment> CreateProjectTreatment(ProjectTreatment newTreatment, string category)
        {
            newTreatment.ImportTimeGeneratedId ??= Guid.NewGuid();

            int.TryParse(newTreatment.Section, out var section);

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO tbl_pb_ExtendedImportedTreatments
    (ScenId, AssetType, Treatment, IsCommitted, PriorityOrder, District, Cnty, Route, FromSection, ToSection,
     Interstate, Direction, PreferredYear, MinYear, MaxYear, Cost, IndirectCostROW, IndirectCostDesign,
     IndirectCostUtilities, IndirectCostOther, AssignedUnconstrainedProjectId, AssignedSpatiallyConstrainedProjectId,
     AssignedTimewiseConstrainedProjectId, BRKEY, BRIDGE_ID, ImportTimeGeneratedId, UserTreatmentTypeNo,
     Benefit, Risk, IsSelected, IsUserCreated)
VALUES
    ({newTreatment.ScenarioId}, {newTreatment.AssetType}, {newTreatment.TreatmentType}, {newTreatment.IsCommitted ?? false}, {newTreatment.PriorityOrder},
     {newTreatment.District}, {newTreatment.CountyId}, {newTreatment.Route}, {section}, {section},
     {newTreatment.Interstate ?? false}, {newTreatment.Direction ?? (byte)0}, {newTreatment.PreferredYear}, {newTreatment.MinYear}, {newTreatment.MaxYear},
     {newTreatment.DirectCost}, {newTreatment.IndirectCostRow}, {newTreatment.IndirectCostDesign},
     {newTreatment.IndirectCostUtilities}, {newTreatment.IndirectCostOther}, {newTreatment.ProjectId}, {newTreatment.ProjectId},
     {newTreatment.ProjectId}, {newTreatment.Brkey}, {newTreatment.BridgeId}, {newTreatment.ImportTimeGeneratedId}, {newTreatment.UserTreatmentTypeNo ?? 0},
     {0.0}, {0.0},
     -- Inherit the project's funded state, so a treatment added to an unfunded project stays unfunded on the map.
     (SELECT p.IsSelected FROM tbl_pb_ExtendedTimewiseConstrainedProjects p
        WHERE p.ScenId = {newTreatment.ScenarioId} AND p.ProjectId = {newTreatment.ProjectId}),
     {true})");

            var libraryId = await GetLibraryIdAsync(newTreatment.ScenarioId);
            if (libraryId.HasValue)
            {
                await UpsertCategoryAsync(libraryId.Value, newTreatment.ImportTimeGeneratedId, newTreatment.AssetType, category);
            }

            return newTreatment;
        }

        public async Task UpdateScenarioNotes(int? scenid)
        {
            var scenario = await _context.Scenarios.FindAsync(scenid);
            if (scenario != null)
            {
                scenario.Stale = true;
                scenario.Notes = "Scenario Needs to be Run Again";
                _context.Scenarios.Update(scenario);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<ProjectTreatment> FindAsync(long? projectTreatmentId)
        {
            return await _context.ProjectTreatments
                .Where(c => c.Id == projectTreatmentId)
                .FirstOrDefaultAsync();
        }

        // Persists the link to a library treatment created after the fact (self-heal path for
        // ProjectTreatment rows edited before they ever had a real ImportTimeGeneratedId).
        public async Task SetImportTimeGeneratedId(long treatmentId, Guid importTimeGeneratedId)
        {
            var target = await _context.ProjectTreatments.FindAsync(treatmentId);
            if (target == null)
                return;

            target.ImportTimeGeneratedId = importTimeGeneratedId;
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteAsync(long? treatmentid)
        {
            var treatment = await _context.ProjectTreatments.FindAsync(treatmentid);
            if (treatment == null)
            {
                return false;
            }

            _context.ProjectTreatments.Remove(treatment);
            await _context.SaveChangesAsync();

            return true;
        }

        // Opts out of the funded-only global filter so a missing project is distinguishable from an unfunded one.
        public async Task<bool?> IsProjectFundedAsync(int scenarioId, int projectId)
        {
            var selected = await _context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.ScenarioId == scenarioId && p.Id == projectId)
                .Select(p => p.Selected)
                .FirstOrDefaultAsync();

            return selected == null ? null : selected == "Y";
        }

        // IsSelected drives which map layer the treatment renders on, so it has to follow the target project.
        public async Task<bool> MoveTreatmentToProjectAsync(int scenarioId, long treatmentId, int targetProjectId)
        {
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE t
SET t.AssignedTimewiseConstrainedProjectId = p.ProjectId,
    t.AssignedUnconstrainedProjectId = p.RefUnconstrainedProjectId,
    t.AssignedSpatiallyConstrainedProjectId = p.RefSpatiallyConstrainedProjectId,
    t.IsSelected = p.IsSelected,
    t.SelectedYear = p.SelectedFirstYear
FROM tbl_pb_ExtendedImportedTreatments t
INNER JOIN tbl_pb_ExtendedTimewiseConstrainedProjects p
    ON p.ScenId = t.ScenId AND p.ProjectId = {targetProjectId}
WHERE t.ScenId = {scenarioId} AND t.ExtendedTreatmentId = {treatmentId}");

            return rows > 0;
        }

        public async Task<List<ProjectTreatment>> FilterProjectTreatments(TreatmentSearchModel model)
        {
            var result = await _context.ProjectTreatments
                .Where(x => (!model.ProjectId.HasValue || x.ProjectId == model.ProjectId) &&
                (!model.ScenarioId.HasValue || x.ScenarioId == model.ScenarioId)
                && (string.IsNullOrEmpty(model.AssetType) || x.AssetType.Contains(model.AssetType))
                && (!model.Cnty.HasValue || x.CountyId.Equals(model.Cnty))
                && (!model.District.HasValue || x.District.Equals(model.District))
                  && (!model.Route.HasValue || x.Route.Equals(model.Route))
                   && (!model.Year.HasValue || x.PreferredYear.Equals(model.Year))
                   && (model.IncludeUnselected || x.IsSelected == true))
                //&& (!model.FromSection.HasValue || x.FromSection.Equals(model.FromSection))
                //&& (!model.ToSection.HasValue || x.ToSection.Equals(model.ToSection)))
                //.ProjectTo<ProjectTreatmentModel>(Mapper.ConfigurationProvider)
                .ToListAsync();

            await EnrichCategoriesAsync(model.ScenarioId, result);

            return result;
        }

        // Populates the [NotMapped] Category scratch field per row (the treatment's CargoData
        // "Category" attribute value) in one batched query, rather than one lookup per row.
        private async Task EnrichCategoriesAsync(int? scenarioId, List<ProjectTreatment> treatments)
        {
            var libraryId = await GetLibraryIdAsync(scenarioId);
            if (!libraryId.HasValue)
                return;

            var importIds = treatments
                .Where(t => t.ImportTimeGeneratedId.HasValue)
                .Select(t => t.ImportTimeGeneratedId.Value)
                .Distinct()
                .ToList();
            if (importIds.Count == 0)
                return;

            var categories = await _context.CargoData
                .Join(_context.CargoAttributes,
                    cd => new { cd.LibraryId, cd.AttributeNo, cd.AssetType },
                    ca => new { ca.LibraryId, ca.AttributeNo, ca.AssetType },
                    (cd, ca) => new { cd.ImportTimeGeneratedGuid, cd.AssetType, ca.AttributeName, cd.TextValue })
                .Where(x => x.AttributeName.ToUpper() == "CATEGORY" && importIds.Contains(x.ImportTimeGeneratedGuid))
                .ToListAsync();

            // TextValue is nullable in tbl_lib_CargoData; drop blanks so a treatment whose only
            // category row is NULL keeps its default rather than being assigned null.
            var lookup = categories
                .Where(c => !string.IsNullOrWhiteSpace(c.TextValue))
                .GroupBy(c => (c.ImportTimeGeneratedGuid, c.AssetType))
                .ToDictionary(g => g.Key, g => g.First().TextValue!);

            foreach (var treatment in treatments)
            {
                if (treatment.ImportTimeGeneratedId.HasValue
                    && lookup.TryGetValue((treatment.ImportTimeGeneratedId.Value, treatment.AssetType), out var category))
                {
                    treatment.Category = category;
                }
            }
        }

        public async Task<List<string>> GetTreatmentCategoryOptionsAsync(int? scenarioId, string assetType)
        {
            var libraryId = await GetLibraryIdAsync(scenarioId);
            if (!libraryId.HasValue || string.IsNullOrEmpty(assetType))
                return new List<string>();

            return await _context.CargoAttributeOptions
                .Where(o => o.LibraryId == libraryId.Value
                    && o.AssetType == assetType
                    && o.AttributeName.ToUpper() == "CATEGORY"
                    && !string.IsNullOrEmpty(o.AttributeValue))
                .Select(o => o.AttributeValue.Trim())
                .Distinct()
                .OrderBy(v => v)
                .ToListAsync();
        }
        public async Task<bool> CheckIsUserCreated(int? scenId)
        {
            bool isUserCreated;
            try
            {
                isUserCreated = await _context.ProjectTreatments.AnyAsync(a => a.ScenarioId == scenId && a.IsUserCreated.HasValue && a.IsUserCreated == true);
            }
            catch (Exception)
            {
                throw;
            }

            return isUserCreated;
        }
        public async Task<bool?> CheckStale(int? scenId)
        {
            bool? stale;
            try
            {
                stale = _context.Scenarios
                    .Where(a => a.Id == scenId)
                    .Select(a => a.Stale)
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }

            return stale;
        }

        public async Task<bool> SetIsUserCreated(int? scenId)
        {
            var isUserCreated = await _context.ProjectTreatments.Where(a => a.ScenarioId == scenId && a.IsUserCreated.HasValue && a.IsUserCreated == true).ToListAsync();
            try
            {
                foreach (var item in isUserCreated)
                {
                    item.IsUserCreated = false;
                }
               await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }


        public async Task<bool> EditProjectTreatments(ProjectTreatment customProjectTreatments, string category)
        {
            ProjectTreatment target = await _context.ProjectTreatments.FindAsync(customProjectTreatments.Id);

            if (target == null)
                return false;
            try
            {
                target.Risk = customProjectTreatments.Risk;
                target.Benefit = customProjectTreatments.Benefit;
                target.DirectCost = customProjectTreatments.DirectCost;
                target.PreferredYear = customProjectTreatments.PreferredYear;
                target.MinYear = customProjectTreatments.MinYear;
                target.MaxYear = customProjectTreatments.MaxYear;
                target.IsCommitted = customProjectTreatments.IsCommitted;
                target.PriorityOrder = customProjectTreatments.PriorityOrder;
                target.UserTreatmentTypeNo = customProjectTreatments.UserTreatmentTypeNo;
                target.IndirectCostUtilities = customProjectTreatments.IndirectCostUtilities;
                target.IndirectCostDesign = customProjectTreatments.IndirectCostDesign;
                target.IndirectCostOther = customProjectTreatments.IndirectCostOther;
                target.IndirectCostRow = customProjectTreatments.IndirectCostRow;
                target.IsUserCreated = customProjectTreatments.IsUserCreated;
                target.Interstate = customProjectTreatments.Interstate;
                target.Direction = customProjectTreatments.Direction;
                await _context.SaveChangesAsync();

                var libraryId = await GetLibraryIdAsync(target.ScenarioId);
                if (libraryId.HasValue)
                {
                    await UpsertCategoryAsync(libraryId.Value, target.ImportTimeGeneratedId, target.AssetType, category);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.ToString());
                return false;
            }
        }

        // "Treatment Type" on the ProjectTreatments Edit/Create modals is the treatment's
        // CargoData "Category" attribute (library + asset-type scoped free-text classification,
        // e.g. "Preservation"/"Applied") — matches legacy's EditProjectTreatment/CreateProjectTreatment
        // controller actions. NOT the tbl_pb_UserTreatments lookup (UserTreatmentTypeNo), which is
        // a separate, unrelated concept used by the Treatments/Index library page.
        private async Task<Guid?> GetLibraryIdAsync(int? scenarioId)
        {
            return await _context.Scenarios
                .Where(s => s.Id == scenarioId)
                .Select(s => s.LibraryId)
                .FirstOrDefaultAsync();
        }

        private async Task UpsertCategoryAsync(Guid libraryId, Guid? importTimeGeneratedId, string assetType, string categoryValue)
        {
            if (!importTimeGeneratedId.HasValue || string.IsNullOrEmpty(assetType))
                return;

            var categoryAttribute = await _context.CargoAttributes.FirstOrDefaultAsync(a =>
                a.LibraryId == libraryId && a.AssetType == assetType && a.AttributeName.ToUpper() == "CATEGORY");
            if (categoryAttribute == null)
                return; // this library/asset type has no Category attribute defined — nothing to persist

            var existing = await _context.CargoData.FirstOrDefaultAsync(cd =>
                cd.LibraryId == libraryId
                && cd.ImportTimeGeneratedGuid == importTimeGeneratedId.Value
                && cd.AttributeNo == categoryAttribute.AttributeNo
                && cd.AssetType == assetType);

            if (existing != null)
            {
                existing.TextValue = categoryValue;
            }
            else if (!string.IsNullOrWhiteSpace(categoryValue))
            {
                _context.CargoData.Add(new CargoData
                {
                    ImportSessionId = Guid.NewGuid(),
                    LibraryId = libraryId,
                    AssetType = assetType,
                    ImportTimeGeneratedGuid = importTimeGeneratedId.Value,
                    AttributeNo = categoryAttribute.AttributeNo,
                    TextValue = categoryValue,
                    NumericValue = null
                });
            }
            else
            {
                return;
            }

            await _context.SaveChangesAsync();
        }

        public Task<List<byte?>> GetDistricts(int? scenarioId)
        {
            return _context.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId)
                                                            .Select(pt => pt.District)
                                                            .Distinct()
                                                            .OrderBy(d => d)
                                                            .ToListAsync();
        }

        public Task<List<CountyEntity>> GetCounties(int? scenarioId, int? district)
        {
            return _context.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.District == district)
                                                            .Join(_context.Counties, p => p.CountyId, c => c.Id, (p, c) => c)
                                                            .Distinct()
                                                            .OrderBy(c => c.Id)
                                                            //.ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                                                            .ToListAsync();
        }

        public Task<List<int?>> GetRoutes(int? scenarioId, int? district, int? county)
        {
            return _context.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.District == district && pt.CountyId == county)
                                                            .Select(pt => pt.Route)
                                                            .Distinct()
                                                            .OrderBy(d => d)
                                                            .ToListAsync();
        }

        public Task<List<string>> GetSections(int? scenarioId, int? district, int? county, int? route)
        {
            return _context.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.District == district && pt.CountyId == county && pt.Route == route)
                                                            .Select(pt => pt.Section)
                                                            .Distinct()
                                                            .ToListAsync();
        }

        // Same lookup as GetSections, but carries FromSection/ToSection so the Map treatment
        // editor's location cascade can render "10 - 10" like the project treatments table does.
        public Task<List<SectionOptionModel>> GetSectionRanges(int? scenarioId, int? district, int? county, int? route)
        {
            return _context.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.District == district && pt.CountyId == county && pt.Route == route)
                                                            .Select(pt => new SectionOptionModel { Section = pt.Section, FromSection = pt.FromSection, ToSection = pt.ToSection })
                                                            .Distinct()
                                                            .OrderBy(s => s.FromSection)
                                                            .ThenBy(s => s.ToSection)
                                                            .ToListAsync();
        }

        // Master reference-data cascade — Pavement reads tbl_pams_MaintainableAssetsSegmentation
        // (already used elsewhere, e.g. PamsSimulationOutputManager), Bridge reads the new
        // tbl_pb_BridgeReferenceDataFromPennshare populated by Admin > Import > Fetch Bridge
        // Reference Data. Neither depends on any scenario's own imported treatments.
        public Task<List<int?>> GetMasterDistricts(string assetType)
        {
            if (assetType == "B")
            {
                return _context.BridgeReferenceData
                    .Where(b => b.District != null)
                    .Select(b => b.District)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();
            }
            return _context.PAMS
                .Select(p => (int?)p.District)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        public Task<List<CountyEntity>> GetMasterCounties(string assetType, int district)
        {
            if (assetType == "B")
            {
                return _context.BridgeReferenceData
                    .Where(b => b.District == district && b.CountyCode != null)
                    .Select(b => b.CountyCode)
                    .Distinct()
                    .Join(_context.Counties, code => code, c => c.Id, (code, c) => c)
                    .OrderBy(c => c.Id)
                    .ToListAsync();
            }
            return _context.PAMS
                .Where(p => p.District == district)
                .Select(p => (int?)p.CountyId)
                .Distinct()
                .Join(_context.Counties, code => code, c => c.Id, (code, c) => c)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        public Task<List<int?>> GetMasterRoutes(string assetType, int district, int county)
        {
            if (assetType == "B")
            {
                return _context.BridgeReferenceData
                    .Where(b => b.District == district && b.CountyCode == county)
                    .Select(b => b.Route)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToListAsync();
            }
            return _context.PAMS
                .Where(p => p.District == district && p.CountyId == county)
                .Select(p => (int?)p.Route)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public Task<List<SectionOptionModel>> GetMasterSectionRanges(string assetType, int district, int county, int route)
        {
            if (assetType == "B")
            {
                // Bridges are a point location (Segment + Offset), not a from/to range — the
                // section option collapses to a single value, same convention already used by
                // sp_pb_MoveBAMSTreatmentsToLibraryTreatments (Segment AS FromSection/ToSection).
                return _context.BridgeReferenceData
                    .Where(b => b.District == district && b.CountyCode == county && b.Route == route && b.Segment != null)
                    .Select(b => new SectionOptionModel { Section = b.Segment.Value.ToString(), FromSection = b.Segment, ToSection = b.Segment })
                    .Distinct()
                    .OrderBy(s => s.FromSection)
                    .ToListAsync();
            }
            return _context.PAMS
                .Where(p => p.District == district && p.CountyId == county && p.Route == route)
                .Select(p => new SectionOptionModel { Section = p.FromSection.ToString(), FromSection = p.FromSection, ToSection = p.ToSection })
                .Distinct()
                .OrderBy(s => s.FromSection)
                .ThenBy(s => s.ToSection)
                .ToListAsync();
        }

        public Task<List<BridgeCandidateModel>> GetMasterBridgeCandidates(int district, int county, int route, string section)
        {
            var segment = int.Parse(section);
            return _context.BridgeReferenceData
                .Where(b => b.District == district && b.CountyCode == county && b.Route == route && b.Segment == segment)
                .OrderBy(b => b.Id)
                .Select(b => new BridgeCandidateModel { BrKey = b.Id, BridgeId = b.BridgeId })
                .ToListAsync();
        }

        public Task<List<int?>> GetYears(int? scenarioId)
        {
            return _context.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.PreferredYear.HasValue).Select(pt => pt.PreferredYear).Distinct().OrderBy(d => d).ToListAsync();
        }

        // Distinct treatment names already in use across this scenario's own project treatments
        // (not the vestigial, always-empty tbl_pb_UserTreatments lookup table). Backs the
        // "Treatment Category" pick-list on the Map editor and the ProjectTreatments page.
        public Task<List<string>> GetTreatmentTypes(int? scenarioId, string assetType)
        {
            return _context.ProjectTreatments
                .Where(pt => pt.ScenarioId == scenarioId
                    && !string.IsNullOrEmpty(pt.TreatmentType)
                    && (string.IsNullOrEmpty(assetType) || pt.AssetType == assetType))
                .Select(pt => pt.TreatmentType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<(List<ProjectTreatment> Items, int TotalCount)> GetPagedScenarioTreatmentsAsync(
            int scenarioId, string assetType, byte? district, byte? countyId, int? route, string section, int? year,
            string sortField, bool sortDesc, int page, int pageSize)
        {
            pageSize = Math.Clamp(pageSize, 1, 200);

            var filtered = _context.ProjectTreatments.AsNoTracking().Where(t =>
                t.ScenarioId == scenarioId
                && (string.IsNullOrEmpty(assetType)
                    || (assetType == "O" ? (t.AssetType != "B" && t.AssetType != "P") : t.AssetType == assetType))
                && (!district.HasValue || t.District == district)
                && (!countyId.HasValue || t.CountyId == countyId)
                && (!route.HasValue || t.Route == route)
                && (string.IsNullOrEmpty(section) || t.Section == section)
                && (!year.HasValue || t.PreferredYear == year));

            var totalCount = await filtered.CountAsync();

            var maxPage = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            page = Math.Clamp(page, 1, maxPage);

            var sorted = ApplyTreatmentSort(filtered, sortField, sortDesc);
            var pageItems = await sorted.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            await EnrichCategoriesAsync(scenarioId, pageItems);

            return (pageItems, totalCount);
        }

        // Whitelisted switch, not string-built SQL. Category is a [NotMapped] scratch field
        // populated post-query (see EnrichCategoriesAsync), so it can't be sorted in SQL — falls
        // through to the Id default like any other unrecognized field.
        private static IQueryable<ProjectTreatment> ApplyTreatmentSort(IQueryable<ProjectTreatment> query, string sortField, bool desc)
        {
            IOrderedQueryable<ProjectTreatment> ordered = (sortField ?? "").ToLowerInvariant() switch
            {
                "assettype" => desc ? query.OrderByDescending(t => t.AssetType) : query.OrderBy(t => t.AssetType),
                "district" => desc ? query.OrderByDescending(t => t.District) : query.OrderBy(t => t.District),
                "county" => desc ? query.OrderByDescending(t => t.County) : query.OrderBy(t => t.County),
                "route" => desc ? query.OrderByDescending(t => t.Route) : query.OrderBy(t => t.Route),
                "section" => desc ? query.OrderByDescending(t => t.Section) : query.OrderBy(t => t.Section),
                "brkey" => desc ? query.OrderByDescending(t => t.Brkey) : query.OrderBy(t => t.Brkey),
                "bridgeid" => desc ? query.OrderByDescending(t => t.BridgeId) : query.OrderBy(t => t.BridgeId),
                "treatment" => desc ? query.OrderByDescending(t => t.TreatmentType) : query.OrderBy(t => t.TreatmentType),
                "interstate" => desc ? query.OrderByDescending(t => t.Interstate) : query.OrderBy(t => t.Interstate),
                "direction" => desc ? query.OrderByDescending(t => t.Direction) : query.OrderBy(t => t.Direction),
                "cost" => desc ? query.OrderByDescending(t => t.DirectCost) : query.OrderBy(t => t.DirectCost),
                "benefit" => desc ? query.OrderByDescending(t => t.Benefit) : query.OrderBy(t => t.Benefit),
                "risk" => desc ? query.OrderByDescending(t => t.Risk) : query.OrderBy(t => t.Risk),
                "preferredyear" => desc ? query.OrderByDescending(t => t.PreferredYear) : query.OrderBy(t => t.PreferredYear),
                "minyear" => desc ? query.OrderByDescending(t => t.MinYear) : query.OrderBy(t => t.MinYear),
                "maxyear" => desc ? query.OrderByDescending(t => t.MaxYear) : query.OrderBy(t => t.MaxYear),
                "iscommitted" => desc ? query.OrderByDescending(t => t.IsCommitted) : query.OrderBy(t => t.IsCommitted),
                "indirectcostdesign" => desc ? query.OrderByDescending(t => t.IndirectCostDesign) : query.OrderBy(t => t.IndirectCostDesign),
                "indirectcostrow" => desc ? query.OrderByDescending(t => t.IndirectCostRow) : query.OrderBy(t => t.IndirectCostRow),
                "indirectcostutilities" => desc ? query.OrderByDescending(t => t.IndirectCostUtilities) : query.OrderBy(t => t.IndirectCostUtilities),
                "indirectcostother" => desc ? query.OrderByDescending(t => t.IndirectCostOther) : query.OrderBy(t => t.IndirectCostOther),
                _ => desc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id)
            };
            return ordered.ThenBy(t => t.Id);
        }
        #endregion
    }
}
