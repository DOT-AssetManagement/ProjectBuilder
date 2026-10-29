using ProjectBuilder.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface ITreatmentTypeRepository : IRepository<TreatmentType>
    {
        Task<TreatmentType> FindAssetType(string id);
        Task<TreatmentType> UpdateTreatmnetTypeAsync(Dictionary<string, object> pairs, string id);
    }
}
