using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;

namespace ProjectBuilder.Web.Authorization;

public static class PermissionAwareRedirect
{
    public static async Task<IActionResult> ToLandingPageAsync(PageModel page, IPermissionChecker permissionChecker)
    {
        if (await permissionChecker.IsGrantedAsync(DSSPermissions.TopBarPermissions.Dashboard))
        {
            return page.RedirectToPage("/Dashboard/Index");
        }

        if (await permissionChecker.IsGrantedAsync(ScenarioPermissions.Default))
        {
            return page.RedirectToPage("/Scenarios/Index");
        }

        if (await permissionChecker.IsGrantedAsync(AdministrationPermissions.Default))
        {
            return page.RedirectToPage("/Administration/Index");
        }

        if (await permissionChecker.IsGrantedAsync(IdentityPermissions.Users.Default))
        {
            return page.RedirectToPage("/Identity/Users/Index");
        }

        return page.RedirectToPage("/Account/AccessDenied");
    }
}
