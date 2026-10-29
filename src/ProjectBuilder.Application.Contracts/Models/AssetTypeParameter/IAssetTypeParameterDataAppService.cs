using ProjectBuilder.Models.AssetTypeParameter.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectBuilder.Models.AssetTypeParameter
{
    public interface IAssetTypeParameterDataAppService : ICrudAppService<AssetTypeParameterDto, Guid, GetAssetTypeParameterInput, CreateAssetTypeParameterDto, UpdateAssetTypeParameterDto>
    {
        Task<List<AssetTypeParameterDto>> GetAllAsync();
    }
}
