using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Zira.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class ZiraDbContextFactory : IDesignTimeDbContextFactory<ZiraDbContext>
{
    public ZiraDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        ZiraEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<ZiraDbContext>()
            .UseSqlite(configuration.GetConnectionString("Default"));
        
        return new ZiraDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Zira.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
