using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace ProjectBuilder.Web.Authorization;

// Management-side counterpart to UserPermissionDenyValueProvider: lets the admin UI
// read/write "UD" grant rows through the normal IPermissionManager Get/SetAsync APIs.
public class UserPermissionDenyManagementProvider : PermissionManagementProvider
{
    public override string Name => UserPermissionDenyValueProvider.ProviderName;

    public UserPermissionDenyManagementProvider(
        IPermissionGrantRepository permissionGrantRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
        : base(
            permissionGrantRepository,
            guidGenerator,
            currentTenant)
    {
    }
}
