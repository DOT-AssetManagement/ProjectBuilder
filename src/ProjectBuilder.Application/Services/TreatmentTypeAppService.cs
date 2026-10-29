using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.TreatmentType;
using ProjectBuilder.Models.TreatmentType.Dtos;
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
    public class TreatmentTypeAppService : ApplicationService, ITreatmentTypeAppService
    {
        private readonly ITreatmentTypeRepository _treatmentTypeRepository;
        private readonly IObjectMapper _ObjectMapper;

        public TreatmentTypeAppService(ITreatmentTypeRepository treatmentTypeRepository, IObjectMapper objectMapper)
        {
            _treatmentTypeRepository = treatmentTypeRepository;
            _ObjectMapper = objectMapper;
        }

        public async Task<PaginatedDto<TreatmentTypeDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var treatmentType = await _treatmentTypeRepository.GetQueryableAsync();
            var displayResult = treatmentType
                  .Skip(param.iDisplayStart)
                  .Take(param.iDisplayLength)
                  .ToList();
            var totalRecords = treatmentType.Count();

            var mappedResults = _ObjectMapper.Map<List<TreatmentType>, List<TreatmentTypeDto>>(displayResult);
            return new PaginatedDto<TreatmentTypeDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };

        }
        public async Task<TreatmentTypeDto> GetTreatmentType(string id)
        {
            var treatmentType = await _treatmentTypeRepository.FindAssetType(id);

            if (treatmentType == null)
            {
                return null; 
            }

            var viewModel = new TreatmentTypeDto
            {
                MoveEarlier = treatmentType.MoveEarlier,
                MoveLater = treatmentType.MoveLater,
                AssetType = treatmentType.AssetType
            };
            return viewModel;
        }
        public async Task<TreatmentTypeDto> EditTreatmentType(UpdateTreatmentTypeDto model)
        {
            if (!((model.MoveEarlier).HasValue && (model.MoveLater).HasValue) && (model.AssetType != null))
            {
                throw new UserFriendlyException("Error occured");
            }
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.MoveEarlier), model.MoveEarlier },
                { nameof(model.MoveLater), model.MoveLater },
                { nameof(model.AssetType), model.AssetType }
            };
            await _treatmentTypeRepository.UpdateTreatmnetTypeAsync(pairs,model.Id);
            var updatedEntity = await _treatmentTypeRepository.FindAssetType(model.Id);
            var result = _ObjectMapper.Map<TreatmentType, TreatmentTypeDto>(updatedEntity);
            return result;

        }

        public async Task<List<TreatmentTypeDto>> GetAllAsync()
        {
            var items = await _treatmentTypeRepository.GetQueryableAsync();
            return items.Select(x => new TreatmentTypeDto
            {
                Id = x.Id,
                AssetType = x.AssetType,
                MoveEarlier = x.MoveEarlier,
                MoveLater = x.MoveLater
            }).ToList();
        }

        public async Task DeleteTreatmentType(string id)
        {
            await _treatmentTypeRepository.DeleteAsync(x => x.Id == id, autoSave: true);
        }
    }
}
