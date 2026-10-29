using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.TreatmentType;
using ProjectBuilder.Models.TreatmentType.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.TreatmentTypes
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ITreatmentTypeAppService _service;

        public IndexModel(ITreatmentTypeAppService service)
        {
            _service = service;
        }

        public List<TreatmentTypeDto> TreatmentTypes { get; set; } = new();

        public async Task OnGetAsync()
        {
            TreatmentTypes = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(string id, int? moveEarlier, int? moveLater, string assetType)
        {
            await _service.EditTreatmentType(new UpdateTreatmentTypeDto { Id = id, MoveEarlier = moveEarlier, MoveLater = moveLater, AssetType = assetType });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(string id)
        {
            await _service.DeleteTreatmentType(id);
            return RedirectToPage();
        }
    }
}
