using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace ProjectBuilder.Web.Pages.Administration
{
    [Authorize]
    public class BackgroundJobsModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
