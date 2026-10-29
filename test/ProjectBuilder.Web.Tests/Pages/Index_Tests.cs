using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace ProjectBuilder.Pages;

[Collection(ProjectBuilderTestConsts.CollectionDefinitionName)]
public class Index_Tests : ProjectBuilderWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
