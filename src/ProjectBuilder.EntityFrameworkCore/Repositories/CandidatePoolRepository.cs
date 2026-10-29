using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PBLogic;
using ProjectBuilder.Entities;
using ProjectBuilder.EntityFrameworkCore;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Linq;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Repositories
{
    public class CandidatePoolRepository : EfCoreRepository<ProjectBuilderDbContext, CandidatePool, Guid>, ICandidatePoolRepository
    {
        private readonly ProjectBuilderDbContext _context;
        private readonly IObjectMapper _objectMapper;
        public Guid UserId { get; set; }
        public bool IncludeAllUsers { get; set; }

  
        public bool? IsChangeTrackingEnabled => throw new NotImplementedException();

        private readonly string _connectionString = string.Empty;


        public CandidatePoolRepository(IDbContextProvider<ProjectBuilderDbContext> projectBuilderDbContext, ILogger<EfCoreRepository<ProjectBuilderDbContext, CandidatePool, Guid>> logger, IConfiguration configuration, ProjectBuilderDbContext dbContext, IObjectMapper objectMapper)
            : base(projectBuilderDbContext)
        {
            _context = dbContext;
            _connectionString = configuration.GetConnectionString("Default");
            _objectMapper = objectMapper;
        }



        protected virtual async Task<IQueryable<CandidatePool>> InitializeCurrrentQuery(Expression<Func<CandidatePool, bool>> filter = null)
        {
            var dbContext = await GetDbContextAsync();
            if (filter == null)
                filter = IncludeAllUsers
                    ? PredicateBuilder.New<CandidatePool>(l => l.IsActive.HasValue ? l.IsActive.Value : false)
                    : PredicateBuilder.New<CandidatePool>(l => (l.UserId == UserId || l.IsShared) && (l.IsActive.HasValue ? l.IsActive.Value : false));
            return dbContext.Libraries.Where(filter).OrderBy(e => e.Id).Join(dbContext.Users, l => l.UserId, u => u.Id, (l, u) => new CandidatePool
            {
                IsActive = l.IsActive,
                Id = l.Id,
                Description = l.Description,
                IsShared = l.IsShared,
                CandidatePoolNumber = l.CandidatePoolNumber,
                Name = l.Name,
                UserId = l.UserId,
                Owner = u.Name ?? u.UserName,
                CreatedAt = l.CreatedAt,
                PopulatedAt = dbContext.CustomTreatments.Where(t => t.LibraryId == l.Id).Select(t => t.PopulatedAt).FirstOrDefault(),
                BridgeTreatmentsCount = dbContext.CustomTreatments.Count(t => t.LibraryId == l.Id && t.AssetType == "B"),
                PavementTreatmentsCount = dbContext.CustomTreatments.Count(t => t.LibraryId == l.Id && t.AssetType == "P"),
                TreatmentsCount = dbContext.CustomTreatments.Count(t => t.LibraryId == l.Id),
                ScenarioCount = dbContext.Scenarios.Count(s => s.LibraryId == l.Id)
            });
        }


        public async Task<Dictionary<Guid, string>> GetSource(List<Guid> libraryIds)
        {
            try
            {
                var sources = new Dictionary<Guid, string>();
                string sourceBAMS, sourcePAMS = "";
                string source = "";
                foreach (var pool in libraryIds)
                {
                    sourceBAMS = "";
                    sourcePAMS = "";
                    source = "";
                    var userTreatmentBAMS = _context.CustomTreatments.FirstOrDefault(t => t.LibraryId == pool && t.AssetType == "B");
                    var userTreatmentPAMS = _context.CustomTreatments.AsNoTracking().FirstOrDefault(t => t.LibraryId == pool && t.AssetType == "P");
                    if (userTreatmentBAMS != default)
                    {
                        Guid importTimeGeneratedId = userTreatmentBAMS.Id;
                        string assetType = userTreatmentBAMS.AssetType;
                        string targetTable = "tbl_import_BAMS_Treatments";

                        try
                        {
                            using (SqlConnection conn = new SqlConnection(_context.Database.GetConnectionString()))
                            {
                                conn.Open();
                                using (SqlCommand command = new SqlCommand())
                                {
                                    command.Connection = conn;
                                    command.CommandText = $"Select TOP 1 ImportSource FROM {targetTable} bt" +
                                        $" JOIN tbl_pb_ImportSessions ims ON ims.Id = bt.ImportSessionId" +
                                        $" WHERE bt.ImportTimeGeneratedId = '{importTimeGeneratedId}' ";
                                    command.CommandType = CommandType.Text;
                                    command.CommandTimeout = 12000;
                                    sourceBAMS = (string)command.ExecuteScalar();
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            throw;
                        }
                    }

                    if (userTreatmentPAMS != default)
                    {
                        Guid importTimeGeneratedId = userTreatmentPAMS.Id;
                        string assetType = userTreatmentPAMS.AssetType;
                        string targetTable = "tbl_import_PAMS_Treatments";

                        try
                        {
                            using (SqlConnection conn = new SqlConnection(_context.Database.GetConnectionString()))
                            {
                                conn.Open();
                                using (SqlCommand command = new SqlCommand())
                                {
                                    command.Connection = conn;
                                    command.CommandText = $"Select TOP 1 ImportSource FROM {targetTable} bt" +
                                        $" JOIN tbl_pb_ImportSessions ims ON ims.Id = bt.ImportSessionId" +
                                        $" WHERE bt.ImportTimeGeneratedId = '{importTimeGeneratedId}' ";
                                    command.CommandType = CommandType.Text;
                                    command.CommandTimeout = 12000;
                                    sourcePAMS = (string)command.ExecuteScalar();
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            throw;
                        }
                    }

                    source = !string.IsNullOrEmpty(sourceBAMS) ? sourceBAMS : "";
                    source = !string.IsNullOrEmpty(sourcePAMS) ? !string.IsNullOrEmpty(source)
                        ? source + ", " + sourcePAMS : sourcePAMS : source;

                    sources.Add(pool, source);

                }
                return sources;
            }
            catch (Exception ex)
            {
                throw;

            }

        }


        //public async Task<Guid?> CheckStale(int? scenId)
        //{
        //	try
        //	{
        //		Guid? libraryId = _context.Scenarios
        //			.Where(a => a.EntityId == scenId)
        //			.Select(a => a.LibraryId)
        //			.FirstOrDefault();

        //		return libraryId;
        //	}
        //	catch (Exception)
        //	{
        //		throw;
        //	}
        //}

        public async Task<(bool Ok, string ErrorMessage)> ActivateCandidatePool(Guid libraryId)
        {
            return await Task.Run(() =>
            {
                var ok = DataManager.ReactivateUserLibrary(_connectionString, libraryId.ToString(), out string errorMessage);
                return (ok, errorMessage);
            });
        }

        public async Task<Guid> CreateNewCandidatePool(CandidatePool library,
            Guid userId,
            bool isEmptyLibrary,
            string? SelectedAsset,
            int? SelectedDistrict,
            short? SelectedCounty,
            int? SelectedRoute,
            int? MinYear,
             int? MaxYear,
            string? libraraysource = null)
        {
            var id = await CreateNewCandidatePool(library);
            if (id != Guid.Empty && !isEmptyLibrary)
                await PopulateCandidatePool(id, libraraysource, string.IsNullOrEmpty(SelectedAsset) ? null : SelectedAsset == "Brdige" ? "B" : "P", SelectedDistrict,
                                                         SelectedCounty, SelectedRoute, MinYear, MaxYear);

            return id;
        }
        public async Task<List<CandidatePool>> GetPoolsListAsync()
        {
            return await _context.Libraries.ToListAsync();
        }

        public async Task<List<CandidatePool>> GetUserLibrariesAsync()
        {
            return await (await InitializeCurrrentQuery()).ToListAsync();
        }

        public async Task<bool> IsLibraryNameTakenAsync(string name, Guid? excludeLibraryId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // SQL unique index UIX_tbl_lib_UserLibraries_Name is case-insensitive
            // (CI collation). Compare UPPER(Name) in the database so TEST and test
            // collide here instead of on insert.
            var upper = name.Trim().ToUpper();
            var query = _context.Libraries.AsNoTracking().Where(x => x.Name.ToUpper() == upper);
            if (excludeLibraryId.HasValue && excludeLibraryId.Value != Guid.Empty)
                query = query.Where(x => x.Id != excludeLibraryId.Value);

            return await query.AnyAsync();
        }

        public async Task<Guid> CreateNewCandidatePool(CandidatePool library)
        {
            var libraryId = string.Empty;
            string errorMessage = string.Empty;
            var ok = await Task.Run(() => DataManager.CreateNewUserLibrary(_connectionString, library.UserId, library.Name, library.Description, library.IsShared, out libraryId, out errorMessage));
            if (!ok || !Guid.TryParse(libraryId, out Guid id))
            {
                if (IsDuplicateLibraryNameError(errorMessage))
                    throw new UserFriendlyException("Name already exists.");
                throw new UserFriendlyException(
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Library record could not be added."
                        : "Unable to create the Treatment Pool Library. Please try again.");
            }

            return id;
        }

        private static bool IsDuplicateLibraryNameError(string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage)) return false;
            return errorMessage.IndexOf("UIX_tbl_lib_UserLibraries_Name", StringComparison.OrdinalIgnoreCase) >= 0
                || errorMessage.IndexOf("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        public async Task<(bool Ok, string ErrorMessage)> DeactivateCandidatePool(Guid libraryId)
        {
            // Previously this discarded DataManager's actual return value and derived "success"
            // from an `out string errorMessage` declared inline in the lambda — which shadowed
            // this method's errorMessage instead of assigning it, so the outer variable stayed
            // "" forever and this always reported success even when the deactivation failed.
            // DataManager logs via log4net, which is never configured in this app, so the real
            // reason was invisible; return it to the caller instead.
            return await Task.Run(() =>
            {
                var ok = DataManager.DeactivateUserLibrary(_connectionString, libraryId.ToString(), out string errorMessage);
                return (ok, errorMessage);
            });
        }
        public async Task<bool> PopulateCandidatePool(Guid libraryId, string? sourcelibrary = null, string assettype = "", int? district = null, short? county = null, int? route = null, int? minyear = null, int? maxyear = null)
        {
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.PopulateUserLibrary(_connectionString, libraryId.ToString(), out string errorMessage, true, sourcelibrary, assettype, district, county, route, "", "", minyear, maxyear));
            return string.IsNullOrEmpty(errorMessage);
        }
        public async Task<bool> DeleteScenarioByIdAsync(int scenarioId)
        {

            var scenario = await _context.Scenarios.FindAsync(scenarioId);

            if (scenario == null)
            {
                return false;
            }

            _context.Scenarios.Remove(scenario);
            await _context.SaveChangesAsync();

            return true;
        }
   

    }
}
