using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.DefaultSlack
{
    public interface IDefaultSlackAppService : IApplicationService
    {
        Task<List<DefaultSlackDto>> GetAllAsync();
        Task<DefaultSlackDto> EditDefaultSlack(UpdateDefaultSlackDto model);
        Task<bool> DeleteDefaultSlack(string assetType);
    }
}
