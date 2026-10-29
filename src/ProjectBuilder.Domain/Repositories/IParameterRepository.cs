using ProjectBuilder.Entities;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IParameterRepository : IRepository<ParameterEntity>
    {
        Task<ParameterEntity> GetAsync(string id, string parmfamily);
        Task<ParameterEntity> UpdateAsync(string id, string parmfamily, string parmName, string parmDescription, double? defaultValue);
        Task<bool> DeleteAsync(string id, string parmfamily);
    }
}
