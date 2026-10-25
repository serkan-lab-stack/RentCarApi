using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RentCar.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class RentCarDbContextFactory : IDesignTimeDbContextFactory<RentCarDbContext>
{
    public RentCarDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        RentCarEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<RentCarDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new RentCarDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../RentCar.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
