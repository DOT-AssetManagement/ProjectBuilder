using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Security.Claims;

namespace ProjectBuilder.Web.Authorization;

// Lets a specific user be explicitly denied a permission even when a Role grants it.
// A "UD" grant row means deny, never grant - ABP's PermissionChecker treats Prohibited
// from any provider as an unconditional override of Granted from other providers.
public class UserPermissionDenyValueProvider : PermissionValueProvider
{
    public const string ProviderName = "UD";

    public override string Name => ProviderName;

    public UserPermissionDenyValueProvider(IPermissionStore permissionStore)
        : base(permissionStore)
    {
    }

    public override async Task<PermissionGrantResult> CheckAsync(PermissionValueCheckContext context)
    {
        var userId = context.Principal?.FindFirst(AbpClaimTypes.UserId)?.Value;
        if (userId == null)
        {
            return PermissionGrantResult.Undefined;
        }

        return await PermissionStore.IsGrantedAsync(context.Permission.Name, Name, userId)
            ? PermissionGrantResult.Prohibited
            : PermissionGrantResult.Undefined;
    }

    public override async Task<MultiplePermissionGrantResult> CheckAsync(PermissionValuesCheckContext context)
    {
        var permissionNames = context.Permissions.Select(x => x.Name).Distinct().ToArray();

        var userId = context.Principal?.FindFirst(AbpClaimTypes.UserId)?.Value;
        if (userId == null)
        {
            return new MultiplePermissionGrantResult(permissionNames);
        }

        var result = await PermissionStore.IsGrantedAsync(permissionNames, Name, userId);

        foreach (var permissionName in result.Result.Keys.ToList())
        {
            if (result.Result[permissionName] == PermissionGrantResult.Granted)
            {
                result.Result[permissionName] = PermissionGrantResult.Prohibited;
            }
        }

        return result;
    }
}
