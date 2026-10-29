using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using ProjectBuilder.Localization;

namespace ProjectBuilder.Web;

[Dependency(ReplaceServices = true)]
public class ProjectBuilderBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<ProjectBuilderResource> _localizer;

    public ProjectBuilderBrandingProvider(IStringLocalizer<ProjectBuilderResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
