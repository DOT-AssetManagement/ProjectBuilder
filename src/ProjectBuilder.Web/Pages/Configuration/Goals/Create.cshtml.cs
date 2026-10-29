using ProjectBuilder.Models.Goal;
using ProjectBuilder.Models.Goal.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.Configuration.Goals
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IGoalAppService _goalAppService;

        public CreateModel(IGoalAppService goalAppService)
        {
            _goalAppService = goalAppService;
        }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostCreate(CreateGoalDto goalCreateDto)

        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _goalAppService.CreateAsync(goalCreateDto);
            return RedirectToPage("/Configuration/Goals/List");
        }
    }
}
