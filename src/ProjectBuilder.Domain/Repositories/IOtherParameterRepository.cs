
using ProjectBuilder.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IOtherParameterRepository : IRepository<OtherParameterEntity>
    {
        Task<OtherParameterEntity> GetAsync(string code);
        Task<OtherParameterEntity> UpdateAsync(OtherParameterEntity entity);
    }
}
