using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using ProjectBuilder.Models.AraEnumerations;
using ProjectBuilder.Models.AraEnumerations.Dtos;
using ProjectBuilder.Models.Counties;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
using ProjectBuilder.Permissions;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    [Authorize(SetUpLookupsPermissions.AraEnumerations)]
    public class AraEnumerationAppService : ApplicationService, IAraEnumerationAppService
    {
        private readonly IAraEnumerationRepository _araEnumerationRepository;
        private readonly IObjectMapper _objectMapper;

        public AraEnumerationAppService(IAraEnumerationRepository araEnumerationRepository, IObjectMapper objectMapper)
        {
            _araEnumerationRepository = araEnumerationRepository;
            _objectMapper = objectMapper;
        }
        public List<AraEnumerationDto> GetAllAraEnumerations()
        {
            var araEnumerations = _araEnumerationRepository.GetQueryable();

            return araEnumerations.Select(item => new AraEnumerationDto
            {
                EnumFamily = item.Id,
                EnumName = item.EnumName,
                EnumInt = item.EnumInt
            }).ToList();
        }
        public async Task<bool> UpdateAraEnumeration(UpdateAraEnumerationDto input)
        {
            return await _araEnumerationRepository.UpdateAraEnumerationAsync(input.EnumFamily, input.OriginalEnumName, input.EnumName, input.EnumInt);
        }

        public async Task<bool> DeleteAraEnumeration(string enumFamily, string enumName)
        {
            return await _araEnumerationRepository.DeleteAraEnumerationAsync(enumFamily, enumName);
        }

        public PaginatedDto<AraEnumerationDto> GetPagedAraEnumerations(JqueryDatatableParamDto param)
        {
            var query = _araEnumerationRepository.GetQueryable();

            var totalRecords = query.Count();

            var data = query
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();

            var mappedResults = _objectMapper.Map<List<AraEnumerations>, List<AraEnumerationDto>>(data);

            return new PaginatedDto<AraEnumerationDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };
        }
    }
}