using ProjectBuilder.Models.Measure.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.Measure
{
    public interface IMeasureAppService : ICrudAppService<MeasureDto, Guid, GetMeasureInput, CreateMeasureDto, UpdateMeasureDto>
    {
        Task<List<MeasureDto>> GetAllAsync();
    }
}
