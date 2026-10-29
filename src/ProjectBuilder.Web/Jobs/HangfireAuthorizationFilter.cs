using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using ProjectBuilder.Permissions;
using System;
using Volo.Abp.Authorization.Permissions;

namespace ProjectBuilder.Web.Jobs
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext.User.Identity?.IsAuthenticated != true)
                return false;

            var permissionChecker = httpContext.RequestServices.GetService(typeof(IPermissionChecker)) as IPermissionChecker;
            if (permissionChecker == null)
                return false;

            return permissionChecker
                .IsGrantedAsync(BackgroundJobsPermissions.Default)
                .GetAwaiter()
                .GetResult();
        }
    }
}
