using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.UserTreatmentType;
using ProjectBuilder.Models.UserTreatmentType.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectBuilder.Web.Pages.SetUp.UserTreatments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserTreatmentTypeAppService _service;

        public IndexModel(IUserTreatmentTypeAppService service)
        {
            _service = service;
        }

        public List<UserTreatmentTypeDto> UserTreatments { get; set; } = new();

        public async Task OnGetAsync()
        {
            UserTreatments = await _service.GetAllAsync();
        }

        public async Task<IActionResult> OnPostEdit(int id, string userTreatmentName)
        {
            await _service.EditUserTreatments(new UpdateUserTreatmentTypeDto { Id = id, UserTreatmentName = userTreatmentName });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            await _service.DeleteUserTreatments(new UserTreatmentTypeDto { Id = id });
            return RedirectToPage();
        }
    }
}
