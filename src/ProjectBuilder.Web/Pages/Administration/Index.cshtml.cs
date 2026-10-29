using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjectBuilder.Web.Pages.Administration
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public void OnGet() { }
    }
}
