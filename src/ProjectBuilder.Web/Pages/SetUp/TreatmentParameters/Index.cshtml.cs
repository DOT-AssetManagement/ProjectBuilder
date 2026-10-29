using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.TreatmentParameterEntity;
using ProjectBuilder.Models.TreatmentParameterEntity.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.TreatmentParameters
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ITreatmentParameterEntityAppService _service;

        public IndexModel(ITreatmentParameterEntityAppService service)
        {
            _service = service;
        }

        public List<TreatmentParameterDto> TreatmentParameters { get; set; } = new();

        public async Task OnGetAsync()
        {
            TreatmentParameters = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(int id, double? userTreatmentBenefitWeight)
        {
            await _service.EditTreatmentParameter(new TreatmentParameterDto { Id = id, UserTreatmentBenefitWeight = userTreatmentBenefitWeight });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            await _service.DeleteTreatmentParameter(new TreatmentParameterDto { Id = id });
            return RedirectToPage();
        }
    }
}
