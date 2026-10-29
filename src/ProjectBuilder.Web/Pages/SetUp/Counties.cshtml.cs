using ProjectBuilder.Models.Counties;
using ProjectBuilder.Models.Counties.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp
{
    public class CountiesModel : PageModel
    {
        private readonly ICountyAppService _countyAppService;
        public List<CountyDto> Counties { get; set; }

        public CountiesModel(ICountyAppService countyAppService)
        {
            _countyAppService = countyAppService;
        }

        public async Task OnGet()
        {
            Counties = await _countyAppService.GetCounties();
        }

        public async Task<IActionResult> OnPostEdit(int id, int? district, string countyName)
        {
            await _countyAppService.EditCountyAsync(new UpdateCountyDto { Id = id, District = district, CountyName = countyName });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id, int district)
        {
            await _countyAppService.DeleteCounty(id, district);
            return RedirectToPage();
        }
    }
}
