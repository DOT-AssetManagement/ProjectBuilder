using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.CreateBridgeToPavements.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.BridgeToPavements
{
    public interface IBridgeToPavmentsAppService : IApplicationService
    {
        List<BridgeToPavementDto> GetAllBridgeToPavements();
        List<BridgeToPavementDto> GetAllAsync();
        PaginatedDto<BridgeToPavementDto> GetPagedBridgeToPavements(JqueryDatatableParamDto param);
    }
}
