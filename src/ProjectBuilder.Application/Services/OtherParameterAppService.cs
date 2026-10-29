using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.OtherParameter;
using ProjectBuilder.Models.OtherParameter.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(DSSPermissions.OtherParameterPermissions.Default)]
    public class OtherParameterAppService : ApplicationService, IOtherParameterAppService
    {
        private readonly IOtherParameterRepository _otherParameterRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public OtherParameterAppService(IOtherParameterRepository otherParameterRepository, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _otherParameterRepository = otherParameterRepository;
            _objectMapper = objectMapper;
            _mapper = mapper;
        }

        public async Task<OtherParameterDto> CreateParameterAsync(CreateOtherParameterDto input)
        {
            var parameter = _mapper.Map(input);
            var response = await _otherParameterRepository.InsertAsync(parameter);
            return _mapper.Map(response);
        }

        public async Task DeleteOtherParameterAsync(string code)
        {
            var entity = await _otherParameterRepository.GetAsync(code);
            if (entity != null)
            {
                await _otherParameterRepository.DeleteAsync(entity);
            }
        }

        [HttpGet("api/app/other-parameter/GetAll")]
        public async Task<List<OtherParameterDto>> GetAllAsync()
        {
            var parameters = await _otherParameterRepository.GetListAsync();
            return _mapper.Map(parameters);
        }

        public async Task<OtherParameterDto> GetParameterAsync(string code)
        {
            var parameter = await _otherParameterRepository.GetAsync(code);
            return _mapper.Map(parameter);
        }

        public async Task<OtherParameterDto> UpdateAsync(string code, UpdateOtherParameterDto input)
        {
            var existing = await _otherParameterRepository.GetAsync(code);
            if (existing == null)
                return null;

            existing.Description = input.Description;
            existing.Val = input.Val;
            existing.CreatedBy = input.CreatedBy;
            existing.CreatedAt = input.CreatedAt;

            try
            {
                var updated = await _otherParameterRepository.UpdateAsync(existing);
                return _mapper.Map(updated);
            }
            catch
            {
                return null;
            }
        }
    }
}
