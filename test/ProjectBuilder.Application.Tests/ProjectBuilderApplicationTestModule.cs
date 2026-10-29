using Volo.Abp.Modularity;

namespace ProjectBuilder;

[DependsOn(
    typeof(ProjectBuilderApplicationModule),
    typeof(ProjectBuilderDomainTestModule)
)]
public class ProjectBuilderApplicationTestModule : AbpModule
{

}
