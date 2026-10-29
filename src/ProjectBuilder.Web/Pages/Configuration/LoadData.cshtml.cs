using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjectBuilder.Web.Pages.Configuration
{
    [Authorize]
    public class LoadDataModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
