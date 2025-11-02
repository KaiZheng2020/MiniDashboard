using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniDashboard.Api.Repository;

namespace MiniDashboard.Tests.Integration.Helpers;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly string _testDbPath;
    
    public CustomWebApplicationFactory()
    {
        // Test database file path: located in the run directory (test output directory)
        var runDirectory = AppContext.BaseDirectory;
        _testDbPath = Path.Combine(runDirectory, "TestMiniDashboard.db");
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        
        // Ensure the database directory exists
        var dbDir = Path.GetDirectoryName(_testDbPath);
        if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }
        
        // Directly configure to use test database without reading default config files
        // Override DbContext configuration in ConfigureServices to ensure test database is used
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MiniDashboardDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            // Re-register DbContext to use test database (database file in run directory)
            services.AddDbContext<MiniDashboardDbContext>(options =>
                options.UseSqlite($"Data Source={_testDbPath}"));
        });
    }
    
    public MiniDashboardDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<MiniDashboardDbContext>();
    }
    
    public async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MiniDashboardDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    
    public string GetTestDbPath() => _testDbPath;
}
