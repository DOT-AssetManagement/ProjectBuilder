using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProjectBuilder.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class ProjectBuilderDbContextFactory : IDesignTimeDbContextFactory<ProjectBuilderDbContext>
{
    public ProjectBuilderDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        ProjectBuilderEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<ProjectBuilderDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"), sql =>
            {
                sql.CommandTimeout(120);
            });
        
        return new ProjectBuilderDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ProjectBuilder.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
