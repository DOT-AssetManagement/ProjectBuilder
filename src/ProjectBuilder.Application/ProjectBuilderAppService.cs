using ProjectBuilder.Localization;
using Volo.Abp.Application.Services;

namespace ProjectBuilder;

/* Inherit your application services from this class.
 */
public abstract class ProjectBuilderAppService : ApplicationService
{
    protected ProjectBuilderAppService()
    {
        LocalizationResource = typeof(ProjectBuilderResource);
    }
}
