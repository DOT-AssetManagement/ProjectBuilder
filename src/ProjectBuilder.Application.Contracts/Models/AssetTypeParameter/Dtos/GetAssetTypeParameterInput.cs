using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.AssetTypeParameter.Dtos
{
    public class GetAssetTypeParameterInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
