using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.ImportSessions.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.ImportSessions
{
    public interface IImportSessionsAppService : IApplicationService
    {
        PaginatedDto<ImportSessionDto> GetPagedImportSessions(JqueryDatatableParamDto param);
        Task<ImportSessionDto> GetByPoolId(Guid poolId);
        List<ImportSessionDto> GetAllAsync();
    }
}
