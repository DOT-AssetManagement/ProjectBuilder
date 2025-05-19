using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectBuilder.Core
{
    public interface ITreatmentRepository : IRepository<UserTreatmentModel>
    {
        Task<bool> DeleteTreatments(Guid modelId);
        Task<bool> EditTreatments(UserTreatmentModel customTreatments);
        Task<List<UserTreatmentModel>> GetByLibraryIdAsync(Guid libraryId, byte? countyId = null, CancellationToken token = default);
        Task<List<UserTreatmentModel>> FilterTreatments(TreatmentSearchModel model);
        Task<TreatmentSearchModel> FilterTreatments(TreatmentSearchModel model, int skip = 0, int take = int.MaxValue);
        Task<Dictionary<string, int?>> GetMinMaxYears();
        Task<List<byte?>> GetDistricts();
        Task<List<CountyModel>> GetCounties(int? district);
        Task<List<int?>> GetRoutes(int? district,int? county);
        Task<List<string>> GetSections(int? district, int? county,int? route);
        Task<List<string>> GetTreatmentTypes(int? district, int? county,int? route);
        Task<List<int?>> GetTreatmentsAvailibleYears(Guid? libraryid);
        Task<DefaultSlackModel> GetAssetTypeDefaultSlack(string assetType);
        Task<SegmentationModel> GetDirectionInterstate(int? district, int? county, int? route, int? fromsection, int? toSection);
        Task<List<CountyModel>> GetAllCounties();

        Task<bool> UpdateMPMSID(Guid importTimeGeneratedGuid, int attributeNo, string textValue);
    }
}
