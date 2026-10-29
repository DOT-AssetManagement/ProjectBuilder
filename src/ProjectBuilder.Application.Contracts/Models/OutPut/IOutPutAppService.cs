using ProjectBuilder.Models.OutPut.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.OutPut
{
    public interface IOutPutAppService : IApplicationService
    {
        Task<MapsResultDto> GenerateMapsResponse(MapsDto mapsViewModel);
        Task<MapsResultDto> SendMapRequest(string data);
    }
}
