using Microsoft.AspNetCore.Mvc;

namespace ProjectBuilder.Web.Components.TenantInfo
{
    public class TenantInfoViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Pages/Components/TenantInfo/Default.cshtml");
        }
    }
}
