using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity;
using ProjectBuilder.Models.TreatmentCancellationMatrixEntity.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.TreatmentCancellationMatrix
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ITreatmentCancellationMatrixEntityAppService _service;

        public IndexModel(ITreatmentCancellationMatrixEntityAppService service)
        {
            _service = service;
        }

        public List<TreatmentCancellationMatrixDto> MatrixItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            MatrixItems = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(string id, string assetTypeA, string assetTypeB, string treatmentB)
        {
            var dto = new UpdateTreatmentCancellationMatrixDto
            {
                Id = id,
                AssetTypeA = assetTypeA.Length > 0 ? assetTypeA[0] : default,
                AssetTypeB = assetTypeB.Length > 0 ? assetTypeB[0] : default,
                TreatmentB = treatmentB
            };
            await _service.EditTreatmentCancellationMatrix(dto);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(string assetTypeA, string treatmentB)
        {
            var dto = new TreatmentCancellationMatrixDto
            {
                AssetTypeA = assetTypeA.Length > 0 ? assetTypeA[0] : default,
                TreatmentB = treatmentB
            };
            await _service.DeleteTreatmentCancellationMatrix(dto);
            return RedirectToPage();
        }
    }
}
