using Microsoft.Extensions.Configuration;
using PBLogic;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Services
{
    public class CandidatePoolUnitOfWork : ICandidatePoolUnitOfWork
    {
        private readonly string _connectionString;
        public CandidatePoolUnitOfWork(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")!;
        }
        public async Task<bool> ActivateCandidatePool(Guid libraryId)
        {
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.ReactivateUserLibrary(_connectionString,libraryId.ToString(),out string errorMessage));
            return string.IsNullOrEmpty(errorMessage);
        }

        public async Task<Guid> CreateNewCandidatePool(CandidatePoolModel library, 
            long userId, 
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

        public async Task<Guid> CreateNewCandidatePool(CandidatePoolModel library)
        {
            var libraryId = string.Empty;
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.CreateNewUserLibrary(_connectionString, library.UserId,library.Name,library.Description,library.IsShared, out libraryId,out errorMessage));
            return Guid.TryParse(libraryId, out Guid id) ? id : Guid.Empty;
        }
        public async Task<bool> DeactivateCandidatePool(Guid libraryId)
        {
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.DeactivateUserLibrary(_connectionString, libraryId.ToString(), out string errorMessage));
            return string.IsNullOrEmpty(errorMessage);
        }
        public async Task<bool> PopulateCandidatePool(Guid libraryId,string? sourcelibrary = null ,string assettype = "", int? district = null, short? county = null, int? route = null, int? minyear = null, int? maxyear = null)
        {
            string errorMessage = string.Empty;
            await Task.Run(() => DataManager.PopulateUserLibrary(_connectionString, libraryId.ToString(),out string errorMessage, true,sourcelibrary, assettype, district, county, route, "", "", minyear, maxyear));
            return string.IsNullOrEmpty(errorMessage);
        }
    }
}
