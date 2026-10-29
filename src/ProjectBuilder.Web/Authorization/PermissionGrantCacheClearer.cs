using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace ProjectBuilder.Web.Authorization;

/// <summary>
/// Clears cached permission-grant lookups after grants are changed in the database
/// (e.g. via SQL migration scripts) so <see cref="IPermissionChecker"/> sees fresh data.
/// </summary>
public class PermissionGrantCacheClearer : ITransientDependency
{
    private readonly IDistributedCache<PermissionGrantCacheItem> _permissionGrantCache;
    private readonly IPermissionGrantRepository _permissionGrantRepository;

    public PermissionGrantCacheClearer(
        IDistributedCache<PermissionGrantCacheItem> permissionGrantCache,
        IPermissionGrantRepository permissionGrantRepository)
    {
        _permissionGrantCache = permissionGrantCache;
        _permissionGrantRepository = permissionGrantRepository;
    }

    public async Task ClearForRoleNamesAsync(IEnumerable<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            var grants = await _permissionGrantRepository.GetListAsync("R", roleName);
            foreach (var grant in grants)
            {
                await _permissionGrantCache.RemoveAsync(
                    PermissionGrantCacheItem.CalculateCacheKey(
                        grant.Name,
                        grant.ProviderName,
                        grant.ProviderKey));
            }
        }
    }
}
