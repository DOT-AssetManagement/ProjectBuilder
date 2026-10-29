using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.TreatmentParameterEntity;
using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
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
    public class TreatmentParameterEntityAppService : ApplicationService, ITreatmentParameterEntityAppService
    {
        private readonly ITreatmentParameterRepositorty _treatmentParameterRepositorty;
        private readonly IObjectMapper _ObjectMapper;

        public TreatmentParameterEntityAppService(ITreatmentParameterRepositorty treatmentParameterRepositorty, IObjectMapper objectMapper)
        {
            _treatmentParameterRepositorty = treatmentParameterRepositorty;
            _ObjectMapper = objectMapper;
        }

        public async Task<PaginatedDto<TreatmentParameterDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var treatmentParameter = await _treatmentParameterRepositorty.GetQueryableAsync();
            var displayResult = treatmentParameter
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();
            var totalRecords = treatmentParameter.Count();

            var mappedResults = _ObjectMapper.Map<List<TreatmentParameterEntity>, List<TreatmentParameterDto>>(displayResult);
            return new PaginatedDto<TreatmentParameterDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };

        }
        public async Task<TreatmentParameterDto> GetTreatmentParameter(int id)
        {
            var treatmentParam = await _treatmentParameterRepositorty.FindTreatmentParameterAsync(id);
            var paramViewModel = new TreatmentParameterDto
            {
                Id = treatmentParam.Id,
                UserTreatmentBenefitWeight = treatmentParam.UserTreatmentBenefitWeight,
            };
            return paramViewModel;
        }
        public async Task<TreatmentParameterDto> EditTreatmentParameter(TreatmentParameterDto model)
        {
            if (!(model.UserTreatmentBenefitWeight).HasValue)
            {
                throw new UserFriendlyException("Error occured");
            }       
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.UserTreatmentBenefitWeight), model.UserTreatmentBenefitWeight }
            };
            await _treatmentParameterRepositorty.UpdateDeTreatmentParameter(pairs,model.Id);
            var updatedEntity = await _treatmentParameterRepositorty.FindTreatmentParameterAsync(model.Id);
            var result = _ObjectMapper.Map<Entities.TreatmentParameterEntity, TreatmentParameterDto>(updatedEntity);
            return result;
        }
        public async Task<bool> DeleteTreatmentParameter(TreatmentParameterDto model)
        {
          return await _treatmentParameterRepositorty.DeleteTreatmentParameter(model.Id);
        }

        public async Task<List<TreatmentParameterDto>> GetAllAsync()
        {
            var items = await _treatmentParameterRepositorty.GetQueryableAsync();
            return items.Select(x => new TreatmentParameterDto
            {
                Id = x.Id,
                UserTreatmentBenefitWeight = x.UserTreatmentBenefitWeight
            }).ToList();
        }

    }
}
