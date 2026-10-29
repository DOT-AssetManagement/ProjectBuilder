using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.AraEnumerations
{
    public interface IAraEnumerationAppService : IApplicationService
    {
        PaginatedDto<AraEnumerationDto> GetPagedAraEnumerations(JqueryDatatableParamDto param);
        List<AraEnumerationDto> GetAllAraEnumerations();
        Task<bool> UpdateAraEnumeration(UpdateAraEnumerationDto input);
        Task<bool> DeleteAraEnumeration(string enumFamily, string enumName);
    }
}
