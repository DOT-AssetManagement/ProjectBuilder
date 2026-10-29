using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface IUserTreatmentTypeRepository : IRepository<UserTreatmentType>
    {
        Task<UserTreatmentType> FindAsync(int id);
        Task UpdateUserTreatmentTypeAsync(Dictionary<string, object> pairs, int id);
        Task<bool> DeleteAsync(int id);
    }
}
