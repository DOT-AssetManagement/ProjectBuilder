using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Models.UserTreatment.Dtos;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Treatment
{
    public interface ITreatmentAppService : IApplicationService
    {
        Task<List<UserTreatmentDto>> GetByLibraryIdAsync(Guid libraryId, string assetType = null);
        Task<List<UserTreatmentDto>> GetByProjectAsync(int scenId, int projectId);
        Task<bool> DeleteTreatmentAsync(Guid treatmentId);

        // Create / Edit treatment (used by the PBDSS Treatments/Index page).
        Task<UserTreatmentDto> GetTreatmentAsync(Guid treatmentId);
        Task<string> CreateTreatment(CreateUserTreatmentDto treatmentViewModel);
        Task<bool> EditTreatmentAsync(UpdateUserTreatmentDto treatmentViewModel);

        // Lookups feeding the create/edit modals + cascading District -> County -> Route -> Section.
        Task<List<UserTreatmentTypeDto>> GetTreatmentTypesAsync();
        Task<List<UserTreatmentTypeDto>> GetTreatmentTypesByLibraryAsync(Guid libraryId);
        Task<UserTreatmentTypeDto> GetOrCreateTreatmentTypeAsync(string name);
        Task<List<byte?>> GetDistrictsAsync();
        Task<List<CountyDto>> GetCountiesAsync(int? district);
        Task<List<int?>> GetRoutesAsync(int? district, int? county);
        Task<List<string>> GetSectionsAsync(int? district, int? county, int? route);
        Task<DefaultSlackDto> GetDefaultSlacksAsync(string assetType);

        Task<List<BridgeLookupDto>> GetBridgeInfoAsync(int? district, int? county, int? route, string section);
    }
}
