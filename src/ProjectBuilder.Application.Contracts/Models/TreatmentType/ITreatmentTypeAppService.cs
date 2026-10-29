using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
using ProjectBuilder.Models.TreatmentType.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.TreatmentType
{
    public interface ITreatmentTypeAppService : IApplicationService
    {
        Task<List<TreatmentTypeDto>> GetAllAsync();
        Task<TreatmentTypeDto> EditTreatmentType(UpdateTreatmentTypeDto model);
        Task DeleteTreatmentType(string id);
    }
}
