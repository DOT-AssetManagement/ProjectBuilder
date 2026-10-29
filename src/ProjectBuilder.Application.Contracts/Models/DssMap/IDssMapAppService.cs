using ProjectBuilder.Models.DssMap.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.DssMap
{
    public interface IDssMapAppService : IApplicationService
    {
        Task<DssMapGisDataDto> GenerateMapAsync(DssMapGenerateRequestDto input);
    }
}
