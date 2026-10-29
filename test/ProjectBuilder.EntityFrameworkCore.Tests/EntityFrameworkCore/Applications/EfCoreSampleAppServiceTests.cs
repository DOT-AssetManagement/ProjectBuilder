using ProjectBuilder.Samples;
using Xunit;

namespace ProjectBuilder.EntityFrameworkCore.Applications;

[Collection(ProjectBuilderTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<ProjectBuilderEntityFrameworkCoreTestModule>
{

}
