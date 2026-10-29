using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectBuilder.Web.Authorization;
using Volo.Abp.Authorization.Permissions;

namespace ProjectBuilder.Web.Pages;

public class IndexModel : ProjectBuilderPageModel
{
    private readonly IPermissionChecker _permissionChecker;

    public IndexModel(IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await PermissionAwareRedirect.ToLandingPageAsync(this, _permissionChecker);
        }

        return RedirectToPage("/Account/Login");
    }
}
