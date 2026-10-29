using ProjectBuilder.Samples;
using Xunit;

namespace ProjectBuilder.EntityFrameworkCore.Domains;

[Collection(ProjectBuilderTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<ProjectBuilderEntityFrameworkCoreTestModule>
{

}
