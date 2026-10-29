using ProjectBuilder.Models.Treatment.Dtos;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.TreatmentCancellationMatrixEntity
{
    public interface ITreatmentCancellationMatrixEntityAppService : IApplicationService
    {
        Task<List<TreatmentCancellationMatrixDto>> GetAllAsync();
        Task<TreatmentCancellationMatrixDto> EditTreatmentCancellationMatrix(UpdateTreatmentCancellationMatrixDto model);
        Task<bool> DeleteTreatmentCancellationMatrix(TreatmentCancellationMatrixDto model);
    }
}
