using ProjectBuilder.Models.Map.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Map
{
    public interface IMapAppService : IApplicationService
    {
        Task<List<MapScenarioDto>> GetScenariosAsync();
        Task<List<MapDistrictDto>> GetDistrictsAsync();
        Task<List<MapCountyDto>> GetCountiesAsync(int districtId);
        Task<List<MapRouteDto>> GetRoutesAsync(int districtId, int countyId);
        Task<MapGisDataDto> GenerateMapAsync(MapGenerateRequestDto input);
    }
}
