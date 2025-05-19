using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class TreatmentRepository : ProjectBuilderRepository<UserTreatment, UserTreatmentModel, Guid>, ITreatmentRepository
    {
        private ProjectBuilderDbContext _context;
        public TreatmentRepository(ProjectBuilderDbContext projectBuilderDbContext, ProjectBuilderDbContext context, IMapper mapper, ILogger<ProjectBuilderRepository<UserTreatment, UserTreatmentModel, Guid>> logger)
                            : base(projectBuilderDbContext, mapper, logger)
        {
            _context = context;
        }
        protected override IQueryable<UserTreatment> InitializeCurrrentQuery(Expression<Func<UserTreatment, bool>> filter = null)
        {
            if (filter is null)
                filter = PredicateBuilder.New<UserTreatment>(true);
            return ProjectBuilderDbContext.CustomTreatments.Where(filter)
                                                           .OrderBy(e => e.EntityId)
                                                           .Join(ProjectBuilderDbContext.Counties, t => t.CountyId, c => c.EntityId, (t, c) => new { treatment = t, county = c })
                                                           .GroupJoin(ProjectBuilderDbContext.UserTreatmentTypes, tr => tr.treatment.UserTreatmentTypeNo, ty => ty.EntityId, (tr, ty) => new { tr.treatment, tr.county, type = ty })
                                                           .SelectMany(ty => ty.type.DefaultIfEmpty(), (tr, ty) => new UserTreatment
                                                           {
                                                               LibraryId = tr.treatment.LibraryId,
                                                               ImportedTreatmentId = tr.treatment.ImportedTreatmentId,
                                                               EntityId = tr.treatment.EntityId,
                                                               District = tr.treatment.District,
                                                               AssetType = tr.treatment.AssetType,
                                                               CountyId = tr.treatment.CountyId,
                                                               County = tr.county,
                                                               Route = tr.treatment.Route,
                                                               Treatment = tr.treatment.Treatment,
                                                               Benefit = tr.treatment.Benefit,
                                                               Interstate = tr.treatment.Interstate,
                                                               IsCommitted = tr.treatment.IsCommitted,
                                                               Cost = tr.treatment.Cost,
                                                               Risk = tr.treatment.Risk,
                                                               PriorityOrder = tr.treatment.PriorityOrder,
                                                               PreferredYear = tr.treatment.PreferredYear,
                                                               MinYear = tr.treatment.MinYear,
                                                               MaxYear = tr.treatment.MaxYear,
                                                               FromSection = tr.treatment.FromSection,
                                                               ToSection = tr.treatment.ToSection,
                                                               Direction = tr.treatment.Direction,
                                                               BridgeId = tr.treatment.BridgeId,
                                                               Brkey = tr.treatment.Brkey,
                                                               IndirectCostDesign = tr.treatment.IndirectCostDesign,
                                                               IndirectCostOther = tr.treatment.IndirectCostOther,
                                                               IndirectCostRow = tr.treatment.IndirectCostRow,
                                                               IndirectCostUtilities = tr.treatment.IndirectCostUtilities,
                                                               IsUserTreatment = tr.treatment.IsUserTreatment,
                                                               UserTreatmentTypeNo = tr.treatment.UserTreatmentTypeNo,
                                                               UserTreatmentName = ty.UserTreatmentName
                                                           }).AsNoTracking();
        }

        #region Additional Queries
        public Task<List<byte?>> GetDistricts()
        {
            return ProjectBuilderDbContext.Treatments.Select(t => t.District).Distinct().OrderBy(d => d).ToListAsync();
        }

        public Task<List<CountyModel>> GetCounties(int? district)
        {
            //return ProjectBuilderDbContext.Counties.Where(t => t.District == district).ProjectTo<CountyModel>(Mapper.ConfigurationProvider).ToListAsync();
            return ProjectBuilderDbContext.CustomTreatments.Where(t => t.District == district)
                                                     .Join(ProjectBuilderDbContext.Counties, t => t.CountyId, c => c.EntityId, (t, c) => c)
                                                     .Distinct()
                                                     .OrderBy(c => c.EntityId)
                                                     .ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                                                     .ToListAsync();
        }

        public Task<List<int?>> GetRoutes(int? district, int? county)
        {
            return ProjectBuilderDbContext.CustomTreatments.Where(t => t.District == district && t.CountyId == county)
                                                     .Select(t => t.Route)
                                                     .Distinct()
                                                     .OrderBy(r => r)
                                                     .ToListAsync();
        }

        public Task<List<string>> GetSections(int? district, int? county, int? route)
        {
            return ProjectBuilderDbContext.CustomTreatments.Where(t => t.District == district && t.CountyId == county && t.Route == route)
                                                     .Select(t => string.Format("{0}-{1}", t.FromSection, t.ToSection))
                                                     .Distinct()
                                                     .ToListAsync();
        }

        public Task<List<string>> GetTreatmentTypes(int? district, int? county, int? route)
        {
            return ProjectBuilderDbContext.CustomTreatments.Where(t => t.District == district && t.CountyId == county && t.Route == route)
                                                     .Select(t => t.Treatment)
                                                     .Distinct()
                                                     .ToListAsync();
        }
        public Task<List<CountyModel>> GetAllCounties()
        {
            return ProjectBuilderDbContext.Counties
                .OrderBy(c => c.EntityId)
                .ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<DefaultSlackModel> GetAssetTypeDefaultSlack(string assetType)
        {
            var result = await ProjectBuilderDbContext.DefaultSlacksValues.FirstOrDefaultAsync(s => s.EntityId == assetType);
            return Mapper.Map<DefaultSlack, DefaultSlackModel>(result);
        }
        public async Task<SegmentationModel> GetDirectionInterstate(int? district, int? county, int? route, int? fromsection, int? toSection)
        {
            var pams = await ProjectBuilderDbContext.PAMS.FirstOrDefaultAsync(p => p.District == district && p.CountyId == county && p.Route == route && p.FromSection == fromsection && p.ToSection == toSection);
            if (pams is null)
                return new();
            return new SegmentationModel { Direction = pams.Direction == 1, isInterstate = pams.Interstate };
        }
        public async Task<bool> DeleteTreatments(Guid modelId)
        {
            var target = ProjectBuilderDbContext.CustomTreatments.Where(x => x.LibraryId == modelId).ToList();
            if (target is null)
                return false;
            ProjectBuilderDbContext.CustomTreatments.RemoveRange(target);
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<List<UserTreatmentModel>> FilterTreatments(TreatmentSearchModel model)
        {
            var result = await CurrentQuery
                .Where(x => (!model.LibraryId.HasValue || x.LibraryId == model.LibraryId)
                && (string.IsNullOrEmpty(model.AssetType) || x.AssetType.Contains(model.AssetType))
                && (!model.Cnty.HasValue || x.CountyId.Equals(model.Cnty))
                && (!model.District.HasValue || x.District.Equals(model.District))
                  && (!model.Route.HasValue || x.Route.Equals(model.Route))
                   && (!model.Year.HasValue || x.PreferredYear.Equals(model.Year))
                   && (!model.FromSection.HasValue || x.FromSection.Equals(model.FromSection))
                   && (!model.ToSection.HasValue || x.ToSection.Equals(model.ToSection)) && (!model.Direction.HasValue || x.Direction.Equals((byte)(model.Direction.Value ? 1 : 0))))
                .ProjectTo<UserTreatmentModel>(Mapper.ConfigurationProvider)
                .ToListAsync();
            return result;
        }

        public async Task<TreatmentSearchModel> FilterTreatments(TreatmentSearchModel model, int skip = 0, int take = int.MaxValue)
        {
            var query = CurrentQuery
                .Where(x => (!model.LibraryId.HasValue || x.LibraryId == model.LibraryId)
                    && (string.IsNullOrEmpty(model.AssetType) || x.AssetType.Contains(model.AssetType))
                    && (!model.Cnty.HasValue || x.CountyId.Equals(model.Cnty))
                    && (!model.District.HasValue || x.District.Equals(model.District))
                    && (!model.Route.HasValue || x.Route.Equals(model.Route))
                    && (!model.Year.HasValue || x.PreferredYear.Equals(model.Year))
                    && (!model.FromSection.HasValue || x.FromSection.Equals(model.FromSection))
                    && (!model.ToSection.HasValue || x.ToSection.Equals(model.ToSection))
                    && (!model.Direction.HasValue || x.Direction.Equals((byte)(model.Direction.Value ? 1 : 0))));

            model.TotalCount = await query.CountAsync();

            model.Items = await query
                .ProjectTo<UserTreatmentModel>(Mapper.ConfigurationProvider)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return model;
        }
        public async Task<bool> EditTreatments(UserTreatmentModel customTreatment)
        {
            var target = await ProjectBuilderDbContext.CustomTreatments.FindAsync(customTreatment.LibraryId, customTreatment.ImportTimeGeneratedId);
            if (target == null)
                return false;
            if (customTreatment.Risk != null) target.Risk = customTreatment.Risk;
            if (customTreatment.Benefit != null) target.Benefit = customTreatment.Benefit;
            if (customTreatment.Cost != null) target.Cost = customTreatment.Cost;
            if (customTreatment.PreferredYear != null) target.PreferredYear = customTreatment.PreferredYear;
            if (customTreatment.MinYear != null) target.MinYear = customTreatment.MinYear;
            if (customTreatment.MaxYear != null) target.MaxYear = customTreatment.MaxYear;
            if (customTreatment.IsCommitted != null) target.IsCommitted = customTreatment.IsCommitted;
            if (customTreatment.PriorityOrder != null) target.PriorityOrder = customTreatment.PriorityOrder;
            if (customTreatment.UserTreatmentTypeNo != null) target.UserTreatmentTypeNo = customTreatment.UserTreatmentTypeNo;
            if (customTreatment.IndirectCostDesign != null) target.IndirectCostDesign = customTreatment.IndirectCostDesign;
            if (customTreatment.IndirectCostRow != null) target.IndirectCostRow = customTreatment.IndirectCostRow;
            if (customTreatment.IndirectCostOther != null) target.IndirectCostOther = customTreatment.IndirectCostOther;
            if (customTreatment.IndirectCostUtilities != null) target.IndirectCostUtilities = customTreatment.IndirectCostUtilities;
            ProjectBuilderDbContext.Update(target);
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditTreatmentsFromAPI(UserTreatmentModel customTreatment)
        {
            var target = await ProjectBuilderDbContext.CustomTreatments.FindAsync(customTreatment.LibraryId, customTreatment.ImportTimeGeneratedId);
            if (target == null)
                return false;
            target.Benefit = customTreatment.Benefit;
            target.Cost = customTreatment.Cost;
            target.PreferredYear = customTreatment.PreferredYear;
            target.Treatment = customTreatment.Treatment;
            target.District = customTreatment.District;
            target.CountyId = customTreatment.CountyId;
            target.Route = customTreatment.Route;
            target.Direction = customTreatment.Direction;
            target.FromSection = customTreatment.FromSection;
            target.ToSection = customTreatment.ToSection;
            target.Brkey = customTreatment.Brkey;
            target.BridgeId = customTreatment.BridgeId;

            ProjectBuilderDbContext.Update(target);
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }


        public async Task<Dictionary<string, int?>> GetMinMaxYears()
        {
            var max = await ProjectBuilderDbContext.Treatments.MaxAsync(t => t.MaxYear);
            var min = await ProjectBuilderDbContext.Treatments.MinAsync(t => t.MinYear);
            return new Dictionary<string, int?> { { "MaxYear", max }, { "MinYear", min } };
        }

        public async Task<List<int?>> GetTreatmentsAvailibleYears(Guid? libraryid)
        {
            return await ProjectBuilderDbContext.CustomTreatments.Where(t => t.PreferredYear.HasValue && t.LibraryId == libraryid).Select(t => t.PreferredYear).Distinct().OrderBy(y => y).ToListAsync();
        }

        public async Task<List<UserTreatmentModel>> GetByLibraryIdAsync(Guid libraryId, byte? countyId = null, CancellationToken token = default)
        {
            var treatmentsQuery = ProjectBuilderDbContext.CustomTreatments.Where(t => t.LibraryId == libraryId);
            if (countyId.HasValue)
            {
                treatmentsQuery = treatmentsQuery.Where(t => t.CountyId == countyId);
            }
            var treatmentsWithCounties = await treatmentsQuery
                .Join(ProjectBuilderDbContext.Counties,
                    treatment => treatment.CountyId,
                    county => county.EntityId,
                    (treatment, county) => new { treatment, county })
                .ToListAsync(token);

            var userTreatmentModels = treatmentsWithCounties
                .Select(x => new UserTreatmentModel
                {
                    AssetType = x.treatment.AssetType,
                    Asset = x.treatment.Asset,
                    County = x.county.CountyFullName,
                    District = x.treatment.District,
                    Route = x.treatment.Route,
                    Brkey = x.treatment.Brkey,
                    BridgeId = x.treatment.BridgeId,
                    Treatment = x.treatment.Treatment,
                    Benefit = x.treatment.Benefit,
                    Cost = x.treatment.Cost,
                    Risk = x.treatment.Risk,
                    PreferredYear = x.treatment.PreferredYear,
                    MinYear = x.treatment.MinYear,
                    MaxYear = x.treatment.MaxYear,
                    IsCommitted = x.treatment.IsCommitted,
                    UserTreatmentName = x.treatment.UserTreatmentName,
                    IndirectCostDesign = x.treatment.IndirectCostDesign,
                    IndirectCostRow = x.treatment.IndirectCostRow,
                    IndirectCostUtilities = x.treatment.IndirectCostUtilities,
                    IndirectCostOther = x.treatment.IndirectCostOther,
                    Interstate = x.treatment.Interstate
                }).ToList();

            return userTreatmentModels;
        }
        public async Task<bool> UpdateMPMSID(Guid importTimeGeneratedGuid, int attributeNo, string textValue)
        {
            var project = await ProjectBuilderDbContext.CargoData.FindAsync(importTimeGeneratedGuid, attributeNo);
            if (project is null)
                return false;

            project.TextValue = textValue;
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
