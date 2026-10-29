using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProjectBuilder.Web.Authorization;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Localization;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.Web;
using PermissionGroupViewModel = Volo.Abp.PermissionManagement.Web.Pages.AbpPermissionManagement.PermissionManagementModal.PermissionGroupViewModel;
using PermissionGrantInfoViewModel = Volo.Abp.PermissionManagement.Web.Pages.AbpPermissionManagement.PermissionManagementModal.PermissionGrantInfoViewModel;

namespace ProjectBuilder.Web.Pages.AbpPermissionManagement;

// Overrides Volo.Abp.PermissionManagement.Web's stock PermissionManagementModal page
// (same route, same view models - see the AbpPermissionManagementModal alias above) so a
// permission granted to a user via a Role can be explicitly denied for that one user.
// Role editing (ProviderName == "R") is untouched: every branch below only changes
// behavior when ProviderName == UserPermissionValueProvider.ProviderName ("U").
public class PermissionManagementModal : AbpPageModel
{
    [Required]
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ProviderName { get; set; }

    [Required]
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ProviderKey { get; set; }

    // Nullable: legitimately optional (OnGetAsync falls back to result.EntityDisplayName when
    // blank) — as a non-nullable string it was implicitly [Required] by ASP.NET Core's nullable-
    // reference-type validation, and since the view never rendered it as a hidden input, every
    // POST submitted it empty, so ModelState.IsValid was always false and every permission save
    // via this modal (any role, any permission) failed with "The ProviderKeyDisplayName field is
    // required" — not something introduced by adding new permissions.
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string? ProviderKeyDisplayName { get; set; }

    [BindProperty]
    public List<PermissionGroupViewModel> Groups { get; set; }

    public string EntityDisplayName { get; set; }

    public bool SelectAllInThisTab { get; set; }

    public bool SelectAllInAllTabs { get; set; }

    public HashSet<string> DeniedPermissionNames { get; set; } = new();

    protected IPermissionAppService PermissionAppService { get; }

    protected IPermissionManager PermissionManager { get; }

    protected ILocalEventBus LocalEventBus { get; }

    public AbpLocalizationOptions LocalizationOptions { get; }

    public bool IsAllTabsDisabled => Groups.All(IsGroupDisabled);

    public PermissionManagementModal(
        IPermissionAppService permissionAppService,
        IPermissionManager permissionManager,
        ILocalEventBus localEventBus,
        IOptions<AbpLocalizationOptions> localizationOptions)
    {
        ObjectMapperContext = typeof(AbpPermissionManagementWebModule);

        PermissionAppService = permissionAppService;
        PermissionManager = permissionManager;
        LocalEventBus = localEventBus;
        LocalizationOptions = localizationOptions.Value;
    }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        ValidateModel();

        var result = await PermissionAppService.GetAsync(ProviderName, ProviderKey);

        EntityDisplayName = !string.IsNullOrWhiteSpace(ProviderKeyDisplayName)
            ? ProviderKeyDisplayName
            : result.EntityDisplayName;

        Groups = ObjectMapper
            .Map<List<PermissionGroupDto>, List<PermissionGroupViewModel>>(result.Groups)
            .OrderBy(g => g.DisplayName)
            .ToList();

        foreach (var group in Groups)
        {
            new Volo.Abp.PermissionManagement.Web.Utils.FlatTreeDepthFinder<PermissionGrantInfoViewModel>().SetDepths(group.Permissions);
        }

        if (IsUserProvider())
        {
            var allNames = Groups.SelectMany(g => g.Permissions).Select(p => p.Name).ToArray();
            if (allNames.Any())
            {
                var denyResult = await PermissionManager.GetAsync(allNames, UserPermissionDenyValueProvider.ProviderName, ProviderKey);
                DeniedPermissionNames = denyResult.Result
                    .Where(r => r.IsGranted)
                    .Select(r => r.Name)
                    .ToHashSet();
            }

            foreach (var permission in Groups.SelectMany(g => g.Permissions))
            {
                var isRoleGranted = permission.GrantedProviders.Any(p => p.ProviderName == RolePermissionValueProvider.ProviderName);
                if (isRoleGranted && DeniedPermissionNames.Contains(permission.Name))
                {
                    permission.IsGranted = false;
                }
            }
        }

        foreach (var group in Groups)
        {
            group.IsAllPermissionsGranted = group.Permissions.All(p => p.IsGranted);
        }

        SelectAllInAllTabs = Groups.All(g => g.IsAllPermissionsGranted);

        return Page();
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        ValidateModel();

        var postedPermissions = Groups.SelectMany(g => g.Permissions).ToList();

        var roleGrantedNames = new HashSet<string>();
        if (IsUserProvider() && postedPermissions.Any())
        {
            var allNames = postedPermissions.Select(p => p.Name).ToArray();
            var currentGrants = await PermissionManager.GetAsync(allNames, ProviderName, ProviderKey);
            roleGrantedNames = currentGrants.Result
                .Where(r => r.Providers.Any(p => p.Name == RolePermissionValueProvider.ProviderName))
                .Select(r => r.Name)
                .ToHashSet();
        }

        var updatePermissionDtos = postedPermissions
            .Where(p => !roleGrantedNames.Contains(p.Name))
            .Select(p => new UpdatePermissionDto
            {
                Name = p.Name,
                IsGranted = p.IsGranted
            })
            .ToArray();

        await PermissionAppService.UpdateAsync(
            ProviderName,
            ProviderKey,
            new UpdatePermissionsDto
            {
                Permissions = updatePermissionDtos
            }
        );

        foreach (var permission in postedPermissions.Where(p => roleGrantedNames.Contains(p.Name)))
        {
            await PermissionManager.SetAsync(
                permission.Name,
                UserPermissionDenyValueProvider.ProviderName,
                ProviderKey,
                !permission.IsGranted);
        }

        Guid? userId = null;
        if (ProviderName == UserPermissionValueProvider.ProviderName && Guid.TryParse(ProviderKey, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        await LocalEventBus.PublishAsync(new CurrentApplicationConfigurationCacheResetEventData(userId));

        return NoContent();
    }

    public bool IsPermissionDisabled(PermissionGrantInfoViewModel permission)
    {
        if (!permission.IsEditable)
        {
            return true;
        }

        if (IsUserProvider())
        {
            // Role-granted permissions used to be locked here - they're now editable
            // (via the "UD" deny-override provider) instead of being a dead end.
            return false;
        }

        return permission.IsDisabled(ProviderName);
    }

    public bool IsGroupDisabled(PermissionGroupViewModel group)
    {
        return group.Permissions.All(IsPermissionDisabled);
    }

    public string GetShownName(PermissionGrantInfoViewModel permission)
    {
        if (!IsUserProvider())
        {
            return permission.GetShownName(ProviderName);
        }

        var grantedByOtherProviders = permission.GrantedProviders
            .Where(p => p.ProviderName != ProviderName)
            .Select(p => p.ProviderName)
            .ToList();

        if (DeniedPermissionNames.Contains(permission.Name))
        {
            grantedByOtherProviders.Add("denied");
        }

        if (!grantedByOtherProviders.Any())
        {
            return permission.DisplayName;
        }

        return $"{permission.DisplayName} ({string.Join(", ", grantedByOtherProviders)})";
    }

    private bool IsUserProvider()
    {
        return ProviderName == UserPermissionValueProvider.ProviderName;
    }
}
