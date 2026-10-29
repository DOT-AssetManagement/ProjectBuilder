using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System;
using ProjectBuilder.Models.Goal.Dtos;
using ProjectBuilder.Models.Goal;
using Microsoft.AspNetCore.Authorization;
namespace ProjectBuilder.Web.Pages.Configuration.Goals
{
    [Authorize]
    public class EditModel : PageModel
    {
        public GoalDto Goal { get; set; }
        private readonly IGoalAppService _goalAppService;
        public EditModel( IGoalAppService goalAppService)
        {
            _goalAppService = goalAppService;
        }
        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Goal = await _goalAppService.GetAsync(id);
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(UpdateGoalDto goalUpdateDto)
        {
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _goalAppService.UpdateAsync(Goal.Id, goalUpdateDto);

            if (Goal == null)
            {
                return NotFound();
            }
            return RedirectToPage("/Configuration/Goals/List");
        }
    }
}