using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Entities;
using ProjectBuilder.Models.AssetTypeParameter;
using ProjectBuilder.Models.AssetTypeParameter.Dtos;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;

namespace ProjectBuilder.Services
{
    public class AssetTypeParameterDataAppService : ApplicationService, IAssetTypeParameterDataAppService
    {
        private readonly IAssetTypeParameterRepository _assetTypeParameterRepository;
        private readonly IObjectMapper _objectMapper;
        private readonly ProjectBuilderMapper _mapper;

        public AssetTypeParameterDataAppService(IAssetTypeParameterRepository assetTypeParameterRepository, IObjectMapper objectMapper, ProjectBuilderMapper mapper)
        {
            _assetTypeParameterRepository = assetTypeParameterRepository;
            _objectMapper = objectMapper;
            _mapper = mapper;
        }

        public async Task<AssetTypeParameterDto> CreateAsync(CreateAssetTypeParameterDto input)
        {
            var asset = _mapper.Map(input);
            var response = await _assetTypeParameterRepository.InsertAsync(asset);
            return _mapper.Map(response);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _assetTypeParameterRepository.DeleteAsync(id);
        }

        [HttpGet("api/app/asset-type-parameter-data/GetAll")]
        public async Task<List<AssetTypeParameterDto>> GetAllAsync()
        {
            var assets = await _assetTypeParameterRepository.GetListAsync();
            return _mapper.Map(assets);
        }

        public async Task<AssetTypeParameterDto> GetAsync(Guid id)
        {
            var asset = await _assetTypeParameterRepository.GetAsync(id);
            return _mapper.Map(asset);
        }

        [HttpGet("api/app/asset-type-parameter-data/GetList")]
        public Task<PagedResultDto<AssetTypeParameterDto>> GetListAsync(GetAssetTypeParameterInput input)
        {
            throw new NotImplementedException();
        }

        public async Task<AssetTypeParameterDto> UpdateAsync(Guid id, UpdateAssetTypeParameterDto input)
        {
            var asset = await _assetTypeParameterRepository.GetAsync(id);
            asset.AssetType = input.AssetType;
            asset.BenefitScalingFactor = input.BenefitScalingFactor;
            asset.ScenId = input.ScenId;
            var response = await _assetTypeParameterRepository.UpdateAsync(asset);
            return _mapper.Map(response);
        }
    }
}
