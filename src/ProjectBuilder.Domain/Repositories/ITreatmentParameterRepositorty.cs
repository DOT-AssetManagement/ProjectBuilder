using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface ITreatmentParameterRepositorty : IRepository<TreatmentParameterEntity>
    {
        Task<TreatmentParameterEntity> FindTreatmentParameterAsync(int id);
        Task<bool> DeleteTreatmentParameter(int id);
        Task UpdateDeTreatmentParameter(Dictionary<string, object> pairs, int id);
    }
}
