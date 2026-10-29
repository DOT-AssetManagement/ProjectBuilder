using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.Measure;
using ProjectBuilder.Models.Measure.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.Measures
{
    [Authorize]
    public class ListModel : PageModel
    {
        private readonly IMeasureAppService _service;

        public ListModel(IMeasureAppService service)
        {
            _service = service;
        }

        public List<MeasureDto> Measures { get; set; } = new();

        public async Task OnGetAsync()
        {
            Measures = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(Guid id, string name, string description, byte? status)
        {
            await _service.UpdateAsync(id, new UpdateMeasureDto { Name = name, Description = description, Status = status });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToPage();
        }
    }
}
