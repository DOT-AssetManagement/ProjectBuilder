using ProjectBuilder.Entities;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ProjectBuilder.Repositories
{
    public interface ITreatmentCancellationMatrixRepository : IRepository<TreatmentCancellationMatrixEntity>
    {
        Task<TreatmentCancellationMatrixEntity> FindAsync(char assettype, string treatment);
        Task UpdateAsync(Dictionary<string, object> pairs, char assettype, string id);
        Task<bool> DeleteAsync(TreatmentCancellationMatrixDto input);
    }
}
