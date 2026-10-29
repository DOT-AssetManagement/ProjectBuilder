using Microsoft.AspNetCore.Builder;
using ProjectBuilder;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("ProjectBuilder.Web.csproj"); 
await builder.RunAbpModuleAsync<ProjectBuilderWebTestModule>(applicationName: "ProjectBuilder.Web");

public partial class Program
{
}
