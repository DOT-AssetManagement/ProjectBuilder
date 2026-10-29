using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface ICandidatePoolRepository : IRepository<CandidatePool>
    {
        public Guid UserId { get; set; }
        /// <summary>When true, GetUserLibrariesAsync returns every user's libraries instead of only own + shared.</summary>
        public bool IncludeAllUsers { get; set; }
        Task<Dictionary<Guid, string>> GetSource(List<Guid> libraryIds);
        //Task<Guid?> CheckStale(int? scenId);
        Task<Guid> CreateNewCandidatePool(CandidatePool candidatePoolModel);
        Task<bool> PopulateCandidatePool(Guid candidatePoolId, string? sourceCandidatePool = null, string assettype = "", int? district = null, short? county = null, int? route = null, int? minyear = null, int? maxyear = null);
        Task<(bool Ok, string ErrorMessage)> ActivateCandidatePool(Guid candidatePoolId);
        Task<(bool Ok, string ErrorMessage)> DeactivateCandidatePool(Guid candidatePoolId);
        Task<Guid> CreateNewCandidatePool(CandidatePool library, Guid userId, bool isEmptyLibrary, string? SelectedAsset,
            int? SelectedDistrict,
            short? SelectedCounty,
            int? SelectedRoute,
            int? MinYear,
             int? MaxYear,
             string? libraraysource = null);
        Task<List<CandidatePool>> GetPoolsListAsync();
        Task<List<CandidatePool>> GetUserLibrariesAsync();
        Task<bool> IsLibraryNameTakenAsync(string name, Guid? excludeLibraryId = null);
        Task<bool> DeleteScenarioByIdAsync(int scenarioId);

    }
}
