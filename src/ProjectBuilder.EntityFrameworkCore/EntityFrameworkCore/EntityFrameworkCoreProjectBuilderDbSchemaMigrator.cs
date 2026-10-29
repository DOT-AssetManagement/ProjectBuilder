using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectBuilder.Data;
using Volo.Abp.DependencyInjection;

namespace ProjectBuilder.EntityFrameworkCore;

public class EntityFrameworkCoreProjectBuilderDbSchemaMigrator
    : IProjectBuilderDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreProjectBuilderDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the ProjectBuilderDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<ProjectBuilderDbContext>()
            .Database
            .MigrateAsync();
    }
}
