using Volo.Abp.Modularity;

namespace ProjectBuilder;

[DependsOn(
    typeof(ProjectBuilderDomainModule),
    typeof(ProjectBuilderTestBaseModule)
)]
public class ProjectBuilderDomainTestModule : AbpModule
{

}
