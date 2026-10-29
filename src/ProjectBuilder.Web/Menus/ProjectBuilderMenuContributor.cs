using System.Threading.Tasks;
using ProjectBuilder.Localization;
using ProjectBuilder.MultiTenancy;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.UI.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;

namespace ProjectBuilder.Web.Menus;

public class ProjectBuilderMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<ProjectBuilderResource>();

        //Home
        context.Menu.AddItem(
            new ApplicationMenuItem(
                ProjectBuilderMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );


        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 5;

        //Administration->Identity
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);

        //Administration->Tenants / Settings — removed from the left menu
        administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        administration.TryRemoveMenuItem(SettingManagementMenuNames.GroupName);

        return Task.CompletedTask;
    }
}
