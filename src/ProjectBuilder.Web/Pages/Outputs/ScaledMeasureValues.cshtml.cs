using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjectBuilder.Web.Pages.Outputs
{
    [Authorize]
    public class ScaledMeasureValuesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
