using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Models.OtherParameter;
using ProjectBuilder.Models.OtherParameter.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Users;

namespace ProjectBuilder.Web.Pages.SetUp.OtherParameter
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public CreateOtherParameterDto OtherParameterCreateDto { get; set; }

        private readonly IOtherParameterAppService _appService;
        private readonly ICurrentUser _currentUser;

        public CreateModel(IOtherParameterAppService appService, ICurrentUser currentUser)
        {
            _appService = appService;
            _currentUser = currentUser;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostCreate(CreateOtherParameterDto otherParameterCreateDto)
        {
            otherParameterCreateDto.CreatedBy = _currentUser.UserName;
            var created = await _appService.CreateParameterAsync(otherParameterCreateDto);
            return created != null ? RedirectToPage("/SetUp/OtherParameter/List") : Page();
        }
    }
}
