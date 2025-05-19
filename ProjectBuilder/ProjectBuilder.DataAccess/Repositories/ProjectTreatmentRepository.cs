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
    public class ProjectTreatmentRepository : ProjectBuilderRepository<ProjectTreatment, ProjectTreatmentModel, long>, IProjectTreatmentRepository
    {
		private ProjectBuilderDbContext _context;

		public ProjectTreatmentRepository(ProjectBuilderDbContext projectBuilderDbContext, IMapper mapper, ILogger<ProjectBuilderRepository<ProjectTreatment, ProjectTreatmentModel, long>> logger)
                            : base(projectBuilderDbContext, mapper, logger)
        {
			_context = projectBuilderDbContext;
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
            var target = await ProjectBuilderDbContext.ProjectTreatments.FirstOrDefaultAsync(x => x.EntityId == modelId);
            if (target is null)
                return false;
            ProjectBuilderDbContext.ProjectTreatments.Remove(target);
            await ProjectBuilderDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProjectTreatmentModel>> FilterProjectTreatments(TreatmentSearchModel model)
        {
            var result = await CurrentQuery
                .Where(x => (!model.ProjectId.HasValue || x.ProjectId == model.ProjectId) && 
                (!model.ScenarioId.HasValue || x.ScenarioId == model.ScenarioId)
                && (string.IsNullOrEmpty(model.AssetType) || x.AssetType.Contains(model.AssetType))
                && (!model.Cnty.HasValue || x.CountyId.Equals(model.Cnty))
                && (!model.District.HasValue || x.District.Equals(model.District))
                  && (!model.Route.HasValue || x.Route.Equals(model.Route))
                   && (!model.Year.HasValue || x.PreferredYear.Equals(model.Year))) 
                   //&& (!model.FromSection.HasValue || x.FromSection.Equals(model.FromSection))
                   //&& (!model.ToSection.HasValue || x.ToSection.Equals(model.ToSection)))
                .ProjectTo<ProjectTreatmentModel>(Mapper.ConfigurationProvider)
                .ToListAsync();
            return result;
        }
        public async Task<bool> CheckIsUserCreated(int? scenId)
        {
            bool isUserCreated;
            try
            {
                isUserCreated = await CurrentQuery.AnyAsync(a => a.ScenarioId == scenId && a.IsUserCreated.HasValue && a.IsUserCreated == true);
            }
            catch (Exception)
            {
                throw;
            }
            
            return isUserCreated;
        }
		public async Task<bool> CheckStale(int? scenId)
		{
			bool stale; 
			try
			{
				stale = _context.Scenarios
					.Where(a => a.EntityId == scenId)
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
            var isUserCreated = await ProjectBuilderDbContext.ProjectTreatments.Where(a => a.ScenarioId == scenId && a.IsUserCreated.HasValue && a.IsUserCreated == true).ToListAsync();
            try
            {
                foreach (var item in isUserCreated)
                {
                    item.IsUserCreated = false;
                }
                ProjectBuilderDbContext.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }
              

        public async Task<Guid> GetProjectTreatmentImportTimeGeneratedId(long projectTreatmentId)
        {
            var target = await ProjectBuilderDbContext.ProjectTreatments
                .FirstOrDefaultAsync(pt => pt.EntityId == projectTreatmentId);

            return target?.ImportTimeGeneratedId ?? Guid.Empty;
        }


        public async Task<bool> EditProjectTreatments(ProjectTreatmentModel customProjectTreatments)
        {
            ProjectTreatment target = await ProjectBuilderDbContext.ProjectTreatments.FindAsync(customProjectTreatments.ProjectTreatmentId);

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
                await ProjectBuilderDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
               Logger.Log(LogLevel.Error, ex.ToString());
               return false;
            }
        }

        public Task<List<byte?>> GetDistricts(int? scenarioId)
        {
            return ProjectBuilderDbContext.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId)
                                                            .Select(pt => pt.District)
                                                            .Distinct()
                                                            .OrderBy(d => d)
                                                            .ToListAsync();
        }

        public Task<List<CountyModel>> GetCounties(int? scenarioId, int? district)
        {
            return ProjectBuilderDbContext.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.District == district)
                                                            .Join(ProjectBuilderDbContext.Counties,p => p.CountyId,c => c.EntityId,(p,c) => c) 
                                                            .Distinct()
                                                            .OrderBy(c => c.EntityId)
                                                            .ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                                                            .ToListAsync();
        }

        public Task<List<int?>> GetRoutes(int? scenarioId, int? district, int? county)
        {
            return ProjectBuilderDbContext.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.District == district && pt.CountyId == county)
                                                            .Select(pt => pt.Route)
                                                            .Distinct()
                                                            .OrderBy(d => d)
                                                            .ToListAsync();
        }

        public Task<List<string>> GetSections(int? scenarioId, int? district, int? county, int? route)
        {
            return ProjectBuilderDbContext.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.District == district && pt.CountyId == county && pt.Route == route)
                                                            .Select(pt => pt.Section)
                                                            .Distinct()
                                                            .ToListAsync();
        }

        public Task<List<int?>> GetYears(int? scenarioId)
        {
            return ProjectBuilderDbContext.ProjectTreatments.Where(pt => pt.ScenarioId == scenarioId && pt.PreferredYear.HasValue).Select(pt => pt.PreferredYear).Distinct().OrderBy(d => d).ToListAsync();
        }
        #endregion
    }
}
