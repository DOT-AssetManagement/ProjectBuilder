using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface ICandidatePoolUnitOfWork
    {
        Task<Guid> CreateNewCandidatePool(CandidatePoolModel candidatePoolModel);
        Task<bool> PopulateCandidatePool(Guid candidatePoolId,string? sourceCandidatePool = null,string assettype = "",int? district = null,short? county = null,int? route = null,int? minyear = null,int? maxyear = null);
        Task<bool> ActivateCandidatePool(Guid candidatePoolId);
        Task<bool> DeactivateCandidatePool(Guid candidatePoolId);
        Task<Guid> CreateNewCandidatePool(CandidatePoolModel library, long userId, bool isEmptyLibrary, string? SelectedAsset,
            int? SelectedDistrict,
            short? SelectedCounty,
            int? SelectedRoute,
            int? MinYear,
             int? MaxYear,
             string? libraraysource = null);
    }
}
