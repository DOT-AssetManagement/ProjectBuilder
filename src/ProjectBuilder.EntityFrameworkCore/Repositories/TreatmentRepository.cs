using AutoMapper;
using ExcelWrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PBLogic;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using System.IO;
using System.Diagnostics.Metrics;
using Volo.Abp.ObjectMapping;


namespace ProjectBuilder.Repositories
{
    public class TreatmentRepository : EfCoreRepository<ProjectBuilderDbContext, UserTreatment, Guid>, ITreatmentRepository
    {
        private ProjectBuilderDbContext _context;
        private readonly string _connectionString = string.Empty;
        private readonly IObjectMapper _objectMapper;

        public TreatmentRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ProjectBuilderDbContext dbContext, ILogger<EfCoreRepository<ProjectBuilderDbContext, UserTreatment, Guid>> logger, IConfiguration configuration, IObjectMapper objectMapper)
                            : base(projectBuilderDbContext)
        {
            _context = dbContext;
            _connectionString = configuration.GetConnectionString("Default");
            _objectMapper = objectMapper;
        }
        protected virtual IQueryable<UserTreatment> InitializeCurrrentQuery(Expression<Func<UserTreatment, bool>> filter = null)
        {
            if (filter is null)
                filter = PredicateBuilder.New<UserTreatment>(true);
            return _context.CustomTreatments.Where(filter)
                                                           .OrderBy(e => e.Id)
                                                           .Join(_context.Counties, t => t.CountyId, c => c.Id, (t, c) => new { treatment = t, county = c })
                                                           .GroupJoin(_context.UserTreatmentTypes, tr => tr.treatment.UserTreatmentTypeNo, ty => ty.Id, (tr, ty) => new { tr.treatment, tr.county, type = ty })
                                                           .SelectMany(ty => ty.type.DefaultIfEmpty(), (tr, ty) => new UserTreatment
                                                           {
                                                               LibraryId = tr.treatment.LibraryId,
                                                               ImportedTreatmentId = tr.treatment.ImportedTreatmentId,
                                                               Id = tr.treatment.Id,
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
            return _context.CustomTreatments.Select(t => t.District).Distinct().OrderBy(d => d).ToListAsync();
        }
        public Task<List<byte?>> GetTreatmentCountyIds()
        {
            return _context.CustomTreatments.Where(t => t.CountyId != null).Select(t => t.CountyId).Distinct().ToListAsync();
        }
        public Task<List<CountyEntity>> GetCounties(int? district)
        {
            //return ProjectBuilderDbContext.Counties.Where(t => t.District == district).ProjectTo<CountyModel>(Mapper.ConfigurationProvider).ToListAsync();
            return _context.CustomTreatments.Where(t => t.District == district)
                                                     .Join(_context.Counties, t => t.CountyId, c => c.Id, (t, c) => c)
                                                     .Distinct()
                                                     .OrderBy(c => c.Id)
                                                     //.ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                                                     .ToListAsync();
        }
        public Task<List<int?>> GetRoutes(int? district, int? county)
        {
            return _context.CustomTreatments.Where(t => t.District == district && t.CountyId == county)
                                                     .Select(t => t.Route)
                                                     .Distinct()
                                                     .OrderBy(r => r)
                                                     .ToListAsync();
        }
        public Task<List<string>> GetSections(int? district, int? county, int? route)
        {
            return _context.CustomTreatments.Where(t => t.District == district && t.CountyId == county && t.Route == route)
                                                     .Select(t => string.Format("{0}-{1}", t.FromSection, t.ToSection))
                                                     .Distinct()
                                                     .ToListAsync();
        }
        public Task<List<string>> GetTreatmentTypes(int? district, int? county, int? route)
        {
            return _context.CustomTreatments.Where(t => t.District == district && t.CountyId == county && t.Route == route)
                                                     .Select(t => t.Treatment)
                                                     .Distinct()
                                                     .ToListAsync();
        }
        public Task<List<CountyEntity>> GetAllCounties()
        {
            return _context.Counties
                .OrderBy(c => c.Id)
                // .ProjectTo<CountyModel>(Mapper.ConfigurationProvider)
                .ToListAsync();
        }
        public async Task<DefaultSlack> GetAssetTypeDefaultSlack(string assetType)
        {
            var result = await _context.DefaultSlacksValues.FirstOrDefaultAsync(s => s.Id == assetType);
            // return Mapper.Map<DefaultSlack, DefaultSlackModel>(result);
            return result;
        }
        public async Task<SegmentationModel> GetDirectionInterstate(int? district, int? county, int? route, int? fromsection, int? toSection)
        {
            var pams = await _context.PAMS.FirstOrDefaultAsync(p => p.District == district && p.CountyId == county && p.Route == route && p.FromSection == fromsection && p.ToSection == toSection);
            if (pams is null)
                return new();
            return new SegmentationModel { Direction = pams.Direction, isInterstate = pams.Interstate };
        }
        public async Task<bool> DeleteTreatments(Guid modelId)
        {
            var target = _context.CustomTreatments.Where(x => x.LibraryId == modelId).ToList();
            if (target is null)
                return false;
            _context.CustomTreatments.RemoveRange(target);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<UserTreatment>> FilterTreatments(TreatmentSearchModel model)
        {
            var result = await _context.CustomTreatments
                .Where(x => (!model.LibraryId.HasValue || x.LibraryId == model.LibraryId)
                && (string.IsNullOrEmpty(model.AssetType) || x.AssetType.Contains(model.AssetType))
                && (!model.Cnty.HasValue || x.CountyId.Equals(model.Cnty))
                && (!model.District.HasValue || x.District.Equals(model.District))
                  && (!model.Route.HasValue || x.Route.Equals(model.Route))
                   && (!model.Year.HasValue || x.PreferredYear.Equals(model.Year))
                   && (!model.FromSection.HasValue || x.FromSection.Equals(model.FromSection))
                   && (!model.ToSection.HasValue || x.ToSection.Equals(model.ToSection)) && (!model.Direction.HasValue || x.Direction.Equals((byte)(model.Direction.Value ? 1 : 0))))
                //.ProjectTo<UserTreatmentModel>(Mapper.ConfigurationProvider)
                .ToListAsync();
            return result;
        }
        public async Task<bool> EditTreatments(UserTreatment customTreatment)
        {
            var target = await _context.CustomTreatments.FindAsync(customTreatment.LibraryId, customTreatment.Id);
            if (target == null)
                return false;

            target.Risk = customTreatment.Risk;
            target.Benefit = customTreatment.Benefit;
            target.Cost = customTreatment.Cost;
            target.PreferredYear = customTreatment.PreferredYear;
            target.MinYear = customTreatment.MinYear;
            target.MaxYear = customTreatment.MaxYear;
            target.IsCommitted = customTreatment.IsCommitted;
            target.PriorityOrder = customTreatment.PriorityOrder;
            target.UserTreatmentTypeNo = customTreatment.UserTreatmentTypeNo;
            target.IndirectCostDesign = customTreatment.IndirectCostDesign;
            target.IndirectCostRow = customTreatment.IndirectCostRow;
            target.IndirectCostOther = customTreatment.IndirectCostOther;
            target.IndirectCostUtilities = customTreatment.IndirectCostUtilities;
            target.Interstate = customTreatment.Interstate;
            target.Direction = customTreatment.Direction;

            _context.Update(target);
            await _context.SaveChangesAsync();

            return await UpdateScenarioNotes(customTreatment.LibraryId);
        }

        private async Task<bool> UpdateScenarioNotes(Guid libraryId)
        {
            // Any scenario that draws from this candidate pool needs a re-run after pool
            // treatments change (edit, delete, or Excel import). Mark every matching scenario
            // — not just FirstOrDefault — so the PB Scenarios screen shows the bold warning
            // for all of them.
            var scenarios = await _context.Scenarios
                .Where(a => a.LibraryId == libraryId)
                .ToListAsync();
            if (scenarios.Count == 0)
            {
                return true;
            }

            foreach (var scenario in scenarios)
            {
                scenario.Notes = "Candidate Pool has been modified. Please perform a scenario re-run";
                scenario.Stale = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<string, int?>> GetMinMaxYears()
        {
            var max = await _context.Treatments.MaxAsync(t => t.MaxYear);
            var min = await _context.Treatments.MinAsync(t => t.MinYear);
            return new Dictionary<string, int?> { { "MaxYear", max }, { "MinYear", min } };
        }
        public async Task<List<int?>> GetTreatmentsAvailibleYears(Guid? libraryid)
        {
            return await _context.CustomTreatments.Where(t => t.PreferredYear.HasValue && t.LibraryId == libraryid).Select(t => t.PreferredYear).Distinct().OrderBy(y => y).ToListAsync();
        }
        public async Task<Guid?> CreateUserTreatment(UserTreatment userTreatment)
        {
            Guid? treatmentId = Guid.Empty;
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.CreateUserTreatment(_connectionString, userTreatment.LibraryId.ToString(), userTreatment.UserTreatmentTypeNo.Value, userTreatment.District.Value,
            userTreatment.CountyId.Value, userTreatment.Route.Value, userTreatment.FromSection.Value, userTreatment.PreferredYear.Value, userTreatment.Cost.Value, userTreatment.Benefit.Value, out treatmentId, out errorMessage,
            userTreatment.Treatment, userTreatment.AssetType.FirstOrDefault(), 0, 0, userTreatment.MinYear, userTreatment.MaxYear, userTreatment.ToSection, userTreatment.Direction,
            userTreatment.Offset, userTreatment.Brkey, null, userTreatment.Asset, userTreatment.Interstate.Value, false, userTreatment.PriorityOrder.Value, userTreatment.Risk
            , null, userTreatment.IsCommitted.Value, false, userTreatment.IndirectCostDesign, userTreatment.IndirectCostRow, userTreatment.IndirectCostUtilities, userTreatment.IndirectCostOther));
            if (string.IsNullOrEmpty(errorMessage))
            {
                await UpdateScenarioNotes(userTreatment.LibraryId);
                return treatmentId;
            }
            return Guid.Empty;
        }
        public async Task<bool> UnAssignTreatmentsFromProject(int projectId)
        {
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.UnAssignProjectTreatments(_connectionString, projectId, out string errorMessage));
            return string.IsNullOrEmpty(errorMessage);
        }
        public void DeleteExcelFile(string excelPath)
        {
            try
            {
                if (File.Exists(excelPath))
                    File.Delete(excelPath);

            }
            catch
            {
            }
        }
        public string ValidateTreatmentExcelFile(string excelPath, string tabName, string importType)
        {
            ExcelHandler.ValidateTreatmentImportFile(excelPath, tabName, importType, out string errorMessage);
            return errorMessage;
        }

        public async Task<string> ImportTreatmentsFromExcelFile(string src, string assetType, string excelPath, string excelTabName, Guid? targetLibraryId, bool fromScratch, bool keepAll)
        {
            string errorMessage = "";
            await Task.Run(() => ExcelHandler.ImportTreatments(_connectionString, src, assetType, excelPath, excelTabName, false, targetLibraryId, fromScratch, keepAll, out errorMessage));

            // Edit/delete already call UpdateScenarioNotes; import went through ExcelHandler
            // and skipped that, so the Scenarios list never showed the stale/re-run warning.
            if (string.IsNullOrWhiteSpace(errorMessage) && targetLibraryId.HasValue)
            {
                await UpdateScenarioNotes(targetLibraryId.Value);
            }

            return errorMessage;
        }
        public async Task<string> ExportProjectsToExcelFile(string excelPath, int scenid, string source)
        {
            string errorMessage = "Selected type doesn't have export feature";
            long recordsWritten = 0;
            if (source == "PAMS")
                await Task.Run(() => ExcelHandler.ExportProjectsToPAMS(_connectionString, excelPath, scenid, out recordsWritten, out errorMessage));
            else if (source == "BAMS")
                await Task.Run(() => ExcelHandler.ExportProjectsToBAMS(_connectionString, excelPath, scenid, out recordsWritten, out errorMessage));
            return errorMessage;
        }
        public async Task<string> ImportMASAndB2PFromExcelFile(bool isMas, string excelPath, string excelTabName)
        {
            string errorMessage = "";
            if (isMas)
            {
                await Task.Run(() => ExcelHandler.ImportMaintainableAssetSegmentation(_connectionString, excelPath, excelTabName, false, out errorMessage));
            }
            else
            {
                await Task.Run(() => ExcelHandler.ImportBridgeToPavement(_connectionString, excelPath, excelTabName, out errorMessage));
            }
            return errorMessage;
        }
        public async Task<bool> DeleteTreatmentAsync(Guid libraryId, Guid treatmentId)
        {
            var treatment = await _context.CustomTreatments
                .FirstOrDefaultAsync(t => t.LibraryId == libraryId && t.Id == treatmentId);

            if (treatment == null)
            {
                return false;
            }

            _context.CustomTreatments.Remove(treatment);
            await _context.SaveChangesAsync();
            return await UpdateScenarioNotes(libraryId);
        }


        #endregion
    }
}
