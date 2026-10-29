using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.Measure;
using ProjectBuilder.Models.Measure.Dtos;
using ProjectBuilder.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(DSSPermissions.MeasurePermissions.Default)]
    public class MeasureAppService : ApplicationService, IMeasureAppService
    {
        private readonly IMeasureRepository _measureRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public MeasureAppService(IMeasureRepository measureRepository, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _measureRepository = measureRepository;
            _objectMapper = objectMapper;
            _mapper = mapper;
        }

        [Authorize(DSSPermissions.MeasurePermissions.Create)]
        public async Task<MeasureDto> CreateAsync(CreateMeasureDto input)
        {
            var measure = _mapper.Map(input);
            var response = await _measureRepository.InsertAsync(measure);
            return _mapper.Map(response);
        }

        [Authorize(DSSPermissions.MeasurePermissions.Delete)]
        [HttpGet("api/app/measure/delete")]
        public async Task DeleteAsync(Guid id)
        {
            await _measureRepository.DeleteAsync(id);
        }

        [HttpGet("api/app/measure/getAll")]
        public async Task<List<MeasureDto>> GetAllAsync()
        {
            var measures = await _measureRepository.GetListAsync();
            return _mapper.Map(measures);
        }

        public async Task<MeasureDto> GetAsync(Guid id)
        {
            var measure = await _measureRepository.GetAsync(id);
            return _objectMapper.Map<MeasureEntity, MeasureDto>(measure);
        }

        public async Task<PagedResultDto<MeasureDto>> GetListAsync(GetMeasureInput input)
        {
            var measures = await _measureRepository.GetListAsync();
            var paged = measures.Skip(input.SkipCount).Take(input.MaxResultCount);
            var dtos = _mapper.Map(new List<MeasureEntity>(paged));
            return new PagedResultDto<MeasureDto>(measures.Count, dtos);
        }

        [Authorize(DSSPermissions.MeasurePermissions.Edit)]
        public async Task<MeasureDto> UpdateAsync(Guid id, UpdateMeasureDto input)
        {
            var measure = await _measureRepository.GetAsync(id);
            measure.Name = input.Name;
            measure.Description = input.Description;
            measure.Status = input.Status;
            var response = await _measureRepository.UpdateAsync(measure);
            return _mapper.Map(response);
        }
    }
}
