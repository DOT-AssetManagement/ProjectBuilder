using ProjectBuilder.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace ProjectBuilder.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ProjectBuilderEntityFrameworkCoreModule),
    typeof(ProjectBuilderApplicationContractsModule)
)]
public class ProjectBuilderDbMigratorModule : AbpModule
{
}
