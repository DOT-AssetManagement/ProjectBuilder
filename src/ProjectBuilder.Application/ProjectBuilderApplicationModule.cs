using Microsoft.Extensions.DependencyInjection;
using ProjectBuilder.Helpers;
using ProjectBuilder.Helpers.Logger;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace ProjectBuilder;

[DependsOn(
    typeof(AbpMapperlyModule),
    typeof(ProjectBuilderDomainModule),
    typeof(ProjectBuilderApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class ProjectBuilderApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //Configure<AbpAutoMapperOptions>(options =>
        //{
        //    options.AddMaps<ProjectBuilderApplicationModule>();
        //});
        context.Services.AddSingleton<ProjectBuilderMapper>();
        context.Services.AddTransient(typeof(IAppLogger<>), typeof(AppLogger<>));

    }
}
