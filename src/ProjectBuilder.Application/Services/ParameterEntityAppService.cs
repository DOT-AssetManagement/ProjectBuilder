using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using ProjectBuilder.Models.Counties.Dtos;
using ProjectBuilder.Models.ImportSessions.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.ParameterEntity;
using ProjectBuilder.Models.ParameterEntity.Dtos;
using ProjectBuilder.Models.ScenarioParamater.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    public class ParameterEntityAppService : ApplicationService, IParameterEntityAppService
    {
        private readonly IParametersRepository _parametersRepository;
        private readonly IScenarioParametersRepository _scenarioParametersRepository;

        public ParameterEntityAppService(IParametersRepository parametersRepository, IScenarioParametersRepository scenarioParametersRepository)
        {
            _parametersRepository = parametersRepository;
            _scenarioParametersRepository = scenarioParametersRepository;
        }

        public async Task<PaginatedDto<ParameterDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var parameter = await _parametersRepository.GetQueryableAsync();
            var displayResult = parameter.Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList();
            var totalRecords = parameter.Count();
            var mappedResults = ObjectMapper.Map<List<ParameterEntity>, List<ParameterDto>>(displayResult);
            return new PaginatedDto<ParameterDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };
        }
        public async Task<List<ParameterDto>> GetAllparameters()
        {
            var parameters = await _parametersRepository.GetListAsync();
            var result = ObjectMapper.Map<List<ParameterEntity>, List<ParameterDto>>(parameters);
            return result;
        }
        public async Task<ParameterDto> GetParameter(string parameterId, string parmfamily)
        {
            var param = await _parametersRepository.FindAsync(parameterId, parmfamily);
            var paramViewModel = new ParameterDto
            {
                Id = param.Id,
                ParmDescription = param.ParmDescription,
                Parmfamily = param.Parmfamily,
                ParmName = param.ParmName,
                DefaultValue = param.DefaultValue
            };
            return paramViewModel;
        }
        public async Task<ParameterDto> EditParameter(UpdateParameterDto model)
        {
            if (string.IsNullOrEmpty(model.ParmDescription) && string.IsNullOrEmpty(model.ParmName) && model.DefaultValue != null)
            {
                throw new InvalidOperationException("the provided information are not valid to edit a Role");
            }
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.ParmDescription), model.ParmDescription },
                { nameof(model.ParmName), model.ParmName },
                { nameof(model.DefaultValue), model.DefaultValue }
            };
            var updatedCounty = await _parametersRepository.UpdateAsync(pairs, model.Id, model.Parmfamily);
            var result = ObjectMapper.Map<ParameterEntity, ParameterDto>(updatedCounty);
            return result;
        }
        public async Task InsertParameterAsync(List<ParameterDto> models, int scenarioId)
        {
            foreach (var model in models)
            {
                if (string.IsNullOrEmpty(model.ParmDescription) && string.IsNullOrEmpty(model.ParmName) && model.DefaultValue != null)
                {
                    throw new UserFriendlyException("The provided information is not valid to insert a parameter.");
                }

                var parameter = new ScenarioParameterDto
                {
                    ParameterId = model.Id,
                    ScenarioId = scenarioId,
                    ParameterValue = model.DefaultValue,
                    ParameterDescription = model.ParmDescription,
                    ParameterName = model.ParmName
                };

                var mappedParameter = ObjectMapper.Map<ScenarioParameterDto, ScenarioParamater>(parameter);
               await _scenarioParametersRepository.InsertAsync(mappedParameter);
            }

        }
        public async Task<bool> DeleteParameter(string parameterId, string parmfamily)
        {
           return  await _parametersRepository.DeleteAsync(parameterId, parmfamily);
        }
    }
}
