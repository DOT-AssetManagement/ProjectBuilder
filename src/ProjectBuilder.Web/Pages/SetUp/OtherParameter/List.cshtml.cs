using ProjectBuilder.Models.OtherParameter;
using ProjectBuilder.Models.OtherParameter.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.OtherParameter
{
    public class ListModel : PageModel
    {
        private readonly IOtherParameterAppService _service;

        public ListModel(IOtherParameterAppService service)
        {
            _service = service;
        }

        public List<OtherParameterDto> OtherParameters { get; set; } = new();

        public async Task OnGetAsync()
        {
            OtherParameters = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostDelete(string code)
        {
            await _service.DeleteOtherParameterAsync(code);
            return RedirectToPage();
        }
    }
}
