using ProjectBuilder.Entities;
using ProjectBuilder.Models.OtherParameter.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System;
using ProjectBuilder.Services;
using ProjectBuilder.Models.OtherParameter;

namespace ProjectBuilder.Web.Pages.SetUp.OtherParameter
{
    public class EditcshtmlModel : PageModel
    {
        private readonly IOtherParameterAppService _appService;

        public EditcshtmlModel(OtherParameterAppService appService)
        {
            _appService = appService;
        }

        public OtherParameterDto otherParameterDto { get; set; }
        public async Task<IActionResult> OnGetAsync(string code)
         {
            otherParameterDto = await _appService.GetParameterAsync(code);
            return Page();
        }


        public async Task<IActionResult> OnPostAsync(string code, UpdateOtherParameterDto input)
        {
            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            var result = await _appService.UpdateAsync(code, input);

            if (result == null)
            {
                return NotFound();
            }

            return RedirectToPage("/SetUp/OtherParameter/List");
        }
    }
}
