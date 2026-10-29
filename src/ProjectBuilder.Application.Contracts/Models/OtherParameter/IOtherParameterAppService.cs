using ProjectBuilder.Models.OtherParameter.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.OtherParameter
{
    public interface IOtherParameterAppService : IApplicationService
    {
        Task<List<OtherParameterDto>> GetAllAsync();
        Task<OtherParameterDto> GetParameterAsync(string code);
        Task<OtherParameterDto> CreateParameterAsync(CreateOtherParameterDto input);
        Task<OtherParameterDto> UpdateAsync(string code, UpdateOtherParameterDto input);
        Task DeleteOtherParameterAsync(string code);
    }
}
