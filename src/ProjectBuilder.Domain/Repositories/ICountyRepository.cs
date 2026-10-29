using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface ICountyRepository : IRepository<CountyEntity>
    {
        Task<CountyEntity> FindAsync(int? district, int? id);
        Task<bool> DeleteCountyAsync(int countyId, int district);
        Task<CountyEntity> UpdateCountyAsync(Dictionary<string, object> updateValues, int? district, int? countyId);
        Task<List<CountyEntity>> GetAllDistricts();
        Task<List<CountyEntity>> GetCountiesByDistrict(int id);

        Task<List<CountyEntity>> GetCounties();
    }
}
