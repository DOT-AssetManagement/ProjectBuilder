using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IParametersRepository : IRepository<ParameterEntity>
    {
        Task<ParameterEntity> FindAsync(string parameterId, string parmfamily);
        Task<ParameterEntity> UpdateAsync(Dictionary<string, object> pairs, string parameterId, string parmfamily);
        Task<bool> DeleteAsync(string parameterId, string parmfamily);
    }
}
