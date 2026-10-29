using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PAMSSectionSegment.Dtos;
using ProjectBuilder.Models.PaginatedResultDto;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.PAMSSectionSegment
{
    public interface IPAMSSectionSegmentAppService : IApplicationService
    {
        PaginatedDto<PAMSSectionSegmentDto> GetPagedMaintainableAssetsSegmentation(JqueryDatatableParamDto param);
    }
}
