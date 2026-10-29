using Volo.Abp.Modularity;

namespace ProjectBuilder;

public abstract class ProjectBuilderApplicationTestBase<TStartupModule> : ProjectBuilderTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
