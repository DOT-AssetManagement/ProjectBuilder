using Xunit;

namespace ProjectBuilder.EntityFrameworkCore;

[CollectionDefinition(ProjectBuilderTestConsts.CollectionDefinitionName)]
public class ProjectBuilderEntityFrameworkCoreCollection : ICollectionFixture<ProjectBuilderEntityFrameworkCoreFixture>
{

}
