using ProjectBuilder.Models.DefaultSlack;
using ProjectBuilder.Models.DefaultSlack.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.DefaultSlack
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDefaultSlackAppService _service;

        public IndexModel(IDefaultSlackAppService service)
        {
            _service = service;
        }

        public List<DefaultSlackDto> DefaultSlacks { get; set; } = new();

        public async Task OnGetAsync()
        {
            DefaultSlacks = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(string id, int moveBefore, int moveAfter)
        {
            await _service.EditDefaultSlack(new UpdateDefaultSlackDto { Id = id, MoveBefore = moveBefore, MoveAfter = moveAfter });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(string id)
        {
            await _service.DeleteDefaultSlack(id);
            return RedirectToPage();
        }
    }
}
