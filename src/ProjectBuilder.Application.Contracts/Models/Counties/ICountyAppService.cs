using ProjectBuilder.Models.Counties.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Counties
{
    public interface ICountyAppService : IApplicationService
    {
        Task<List<CountyDto>> GetAllDistricts();
        Task<List<CountyDto>> GetCountiesByDistrict(int id);
        Task<List<CountyDto>> GetCounties();
        Task<CountyDto> EditCountyAsync(UpdateCountyDto model);
        Task<bool> DeleteCounty(int countyId, int district);
    }
}
