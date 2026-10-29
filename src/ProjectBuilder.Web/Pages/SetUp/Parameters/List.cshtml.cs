using ProjectBuilder.Models.ParameterEntity;
using ProjectBuilder.Models.ParameterEntity.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.Parameters
{
    public class ListModel : PageModel
    {
        private readonly IParameterAppService _service;

        public ListModel(IParameterAppService service)
        {
            _service = service;
        }

        public List<ParameterDto> Parameters { get; set; } = new();

        public async Task OnGet()
        {
            Parameters = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(string id, string parmfamily, string parmName, string parmDescription, double? defaultValue)
        {
            await _service.EditParameterAsync(id, parmfamily, parmName, parmDescription, defaultValue);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(string id, string parmfamily)
        {
            await _service.DeleteParameterAsync(id, parmfamily);
            return RedirectToPage();
        }
    }
}
