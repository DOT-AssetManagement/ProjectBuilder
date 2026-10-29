using ProjectBuilder.Models.ParameterEntity.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.ParameterEntity
{
    public interface IParameterAppService : IApplicationService
    {
        Task<List<ParameterDto>> GetAllAsync();
        Task<ParameterDto> EditParameterAsync(string id, string parmfamily, string parmName, string parmDescription, double? defaultValue);
        Task<bool> DeleteParameterAsync(string id, string parmfamily);
    }
}
