using ProjectBuilder.Models.BridgeReferenceData.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.BridgeReferenceData
{
    public interface IBridgeReferenceDataAppService : IApplicationService
    {
        PaginatedDto<BridgeReferenceDataDto> GetPagedBridgeReferenceData(JqueryDatatableParamDto param);
    }
}
