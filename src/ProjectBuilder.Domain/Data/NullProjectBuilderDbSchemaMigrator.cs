using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace ProjectBuilder.Data;

/* This is used if database provider does't define
 * IProjectBuilderDbSchemaMigrator implementation.
 */
public class NullProjectBuilderDbSchemaMigrator : IProjectBuilderDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
