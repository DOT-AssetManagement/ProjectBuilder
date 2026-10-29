using Volo.Abp.Modularity;

namespace ProjectBuilder;

/* Inherit from this class for your domain layer tests. */
public abstract class ProjectBuilderDomainTestBase<TStartupModule> : ProjectBuilderTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
