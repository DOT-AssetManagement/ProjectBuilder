using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.AraEnumerations;
using ProjectBuilder.Models.AraEnumerations.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.AraEnumerations
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IAraEnumerationAppService _service;

        public IndexModel(IAraEnumerationAppService service)
        {
            _service = service;
        }

        public List<AraEnumerationDto> AraEnumerations { get; set; } = new();

        public Task OnGetAsync()
        {
            AraEnumerations = _service.GetAllAraEnumerations();
            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostEdit(string enumFamily, string originalEnumName, string enumName, int enumInt)
        {
            await _service.UpdateAraEnumeration(new UpdateAraEnumerationDto
            {
                EnumFamily = enumFamily,
                OriginalEnumName = originalEnumName,
                EnumName = enumName,
                EnumInt = enumInt
            });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(string enumFamily, string enumName)
        {
            await _service.DeleteAraEnumeration(enumFamily, enumName);
            return RedirectToPage();
        }
    }
}
