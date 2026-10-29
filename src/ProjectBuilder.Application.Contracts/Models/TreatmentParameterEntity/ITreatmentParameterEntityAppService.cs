using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.TreatmentParameterEntity
{
    public interface ITreatmentParameterEntityAppService : IApplicationService
    {
        Task<List<TreatmentParameterDto>> GetAllAsync();
        Task<TreatmentParameterDto> EditTreatmentParameter(TreatmentParameterDto model);
        Task<bool> DeleteTreatmentParameter(TreatmentParameterDto model);
    }
}
