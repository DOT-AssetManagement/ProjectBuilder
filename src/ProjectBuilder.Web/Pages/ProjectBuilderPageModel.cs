using ProjectBuilder.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace ProjectBuilder.Web.Pages;

public abstract class ProjectBuilderPageModel : AbpPageModel
{
    protected ProjectBuilderPageModel()
    {
        LocalizationResourceType = typeof(ProjectBuilderResource);
    }
}
