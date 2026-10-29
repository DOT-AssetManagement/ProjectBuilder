using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.AssetTypeParameter;
using ProjectBuilder.Models.AssetTypeParameter.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.AssetTypeParameter
{
    [Authorize]
    public class ListModel : PageModel
    {
        private readonly IAssetTypeParameterDataAppService _service;

        public ListModel(IAssetTypeParameterDataAppService service)
        {
            _service = service;
        }

        public List<AssetTypeParameterDto> AssetTypeParameters { get; set; } = new();

        public async Task OnGetAsync()
        {
            AssetTypeParameters = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(Guid id, string assetType, float benefitScalingFactor)
        {
            await _service.UpdateAsync(id, new UpdateAssetTypeParameterDto
            {
                AssetType = assetType,
                BenefitScalingFactor = benefitScalingFactor
            });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToPage();
        }
    }
}
