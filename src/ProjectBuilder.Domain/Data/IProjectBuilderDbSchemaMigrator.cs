using System.Threading.Tasks;

namespace ProjectBuilder.Data;

public interface IProjectBuilderDbSchemaMigrator
{
    Task MigrateAsync();
}
