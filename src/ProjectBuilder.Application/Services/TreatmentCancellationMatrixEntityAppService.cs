using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using ProjectBuilder.Models.JqueryDatatableParam;
using ProjectBuilder.Models.PaginatedResultDto;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using ProjectBuilder.Models.TreatmentType.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    public class TreatmentCancellationMatrixEntityAppService : ApplicationService, ITreatmentCancellationMatrixEntityAppService
    {
        private readonly ITreatmentCancellationMatrixRepository _treatmentCancellationMatrixRepository;

        public TreatmentCancellationMatrixEntityAppService(ITreatmentCancellationMatrixRepository treatmentCancellationMatrixRepository)
        {
            _treatmentCancellationMatrixRepository = treatmentCancellationMatrixRepository;
        }

        public async Task<PaginatedDto<TreatmentCancellationMatrixDto>> ApplyPagination(JqueryDatatableParamDto param)
        {
            var treatmentCancellationMatrix = await _treatmentCancellationMatrixRepository.GetQueryableAsync();
            var displayResult = treatmentCancellationMatrix
                 .Skip(param.iDisplayStart)
                 .Take(param.iDisplayLength)
                 .ToList();
            var totalRecords = treatmentCancellationMatrix.Count();

            var mappedResults = ObjectMapper.Map<List<TreatmentCancellationMatrixEntity>, List<TreatmentCancellationMatrixDto>>(displayResult);
            return new PaginatedDto<TreatmentCancellationMatrixDto>
            {
                SEcho = param.sEcho,
                ITotalRecords = totalRecords,
                ITotalDisplayRecords = totalRecords,
                AaData = mappedResults
            };

        }
        public async Task<TreatmentCancellationMatrixDto> GetTreatmentCancellationMatrix(char assetType, string treatment)
        {
            var treatmentCancellationMatrix = await _treatmentCancellationMatrixRepository.FindAsync(assetType, treatment);
            var matrixViewModel = new TreatmentCancellationMatrixDto
            {
                AssetTypeA = treatmentCancellationMatrix.AssetTypeA,
                Id = treatmentCancellationMatrix.Id,
                AssetTypeB = treatmentCancellationMatrix.AssetTypeB,
                TreatmentB = treatmentCancellationMatrix.TreatmentB,
            };

            return matrixViewModel;  
        }
        public async Task<TreatmentCancellationMatrixDto> EditTreatmentCancellationMatrix(UpdateTreatmentCancellationMatrixDto model)
        {
            if (model.AssetTypeB != null && string.IsNullOrEmpty(model.TreatmentB))
            {

            }
            var pairs = new Dictionary<string, object>
            {
                { nameof(model.AssetTypeB), model.AssetTypeB },
                { nameof(model.TreatmentB), model.TreatmentB }
            };
            await _treatmentCancellationMatrixRepository.UpdateAsync(pairs, model.AssetTypeA, model.Id);
            var updatedEntity = await _treatmentCancellationMatrixRepository.FindAsync(model.AssetTypeA,model.TreatmentB);
            var result = ObjectMapper.Map<TreatmentCancellationMatrixEntity, TreatmentCancellationMatrixDto>(updatedEntity);
            return result;
        }
        public async Task<bool> DeleteTreatmentCancellationMatrix(TreatmentCancellationMatrixDto model)
        {
           return await _treatmentCancellationMatrixRepository.DeleteAsync(model);
        }

        public async Task<List<TreatmentCancellationMatrixDto>> GetAllAsync()
        {
            var items = await _treatmentCancellationMatrixRepository.GetQueryableAsync();
            return items.Select(x => new TreatmentCancellationMatrixDto
            {
                Id = x.Id,
                AssetTypeA = x.AssetTypeA,
                AssetTypeB = x.AssetTypeB,
                TreatmentB = x.TreatmentB
            }).ToList();
        }

    }
}
