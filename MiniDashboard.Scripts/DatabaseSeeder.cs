using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniDashboard.Api.Repository;
using MiniDashboard.Tests.Integration.Helpers;

namespace MiniDashboard.Scripts;

public class DatabaseSeeder : IDisposable
{
    private readonly MiniDashboardDbContext _context;
    
    public string DbPath { get; }

    private DatabaseSeeder(MiniDashboardDbContext context, string dbPath)
    {
        _context = context;
        DbPath = dbPath;
    }

    public static async Task<DatabaseSeeder> CreateAsync()
    {
        // Load configuration from API project
        // Find API project directory by looking for appsettings.json
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        DirectoryInfo? apiProjectDir = null;
        
        // Try to find MiniDashboard.Api directory by going up the directory tree
        var searchDir = currentDir;
        while (searchDir != null && apiProjectDir == null)
        {
            var apiDir = Path.Combine(searchDir.FullName, "MiniDashboard.Api");
            if (Directory.Exists(apiDir) && File.Exists(Path.Combine(apiDir, "appsettings.json")))
            {
                apiProjectDir = new DirectoryInfo(apiDir);
                break;
            }
            searchDir = searchDir.Parent;
        }
        
        if (apiProjectDir == null)
        {
            throw new DirectoryNotFoundException(
                "Cannot find MiniDashboard.Api directory with appsettings.json. " +
                $"Searched from: {currentDir.FullName}");
        }
        
        var apiProjectPath = apiProjectDir.FullName;
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=MiniDashboard.db";

        // Resolve database path (relative paths are resolved relative to API project)
        var dbPath = connectionString.Replace("Data Source=", "").Trim();
        if (!Path.IsPathRooted(dbPath))
        {
            dbPath = Path.Combine(apiProjectPath, dbPath);
        }

        // Create DbContext
        var optionsBuilder = new DbContextOptionsBuilder<MiniDashboardDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        var context = new MiniDashboardDbContext(optionsBuilder.Options);

        // Ensure database exists and schema is ready
        // If database file doesn't exist, create it and apply migrations
        // If database exists, check if migrations need to be applied
        if (!File.Exists(dbPath))
        {
            // Database doesn't exist, apply migrations to create it
            await context.Database.MigrateAsync();
        }
        else
        {
            // Database exists, check if there are pending migrations
            // Only apply migrations if needed to avoid "table already exists" errors
            try
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    // Only apply migrations if there are pending ones
                    await context.Database.MigrateAsync();
                }
            }
            catch
            {
                // If GetPendingMigrationsAsync fails (e.g., no __EFMigrationsHistory table),
                // try to apply migrations anyway, but catch table exists errors
                try
                {
                    await context.Database.MigrateAsync();
                }
                catch (Exception ex) when (ex.Message.Contains("already exists"))
                {
                    // Table already exists, database schema is fine, continue
                }
            }
        }

        return new DatabaseSeeder(context, dbPath);
    }

    public async Task<int> GetCurrentItemCountAsync()
    {
        return await _context.Items.CountAsync();
    }

    public async Task<int> ClearAllItemsAsync()
    {
        var deletedCount = await _context.Items.ExecuteDeleteAsync();
        return deletedCount;
    }

    public async Task<int> SeedItemsAsync(int count, string prefix = "Seed Item", bool showProgress = true)
    {
        // Generate test data
        if (showProgress)
        {
            Console.WriteLine($"Generating {count} test items...");
        }
        var items = TestDataBuilder.CreateItems(count, prefix);
        
        if (showProgress)
        {
            Console.WriteLine("✓ Test data generated");
            Console.WriteLine();
        }

        // Insert data in batches for better performance
        const int batchSize = 100;
        var insertedCount = 0;
        
        for (int i = 0; i < items.Count; i += batchSize)
        {
            var batch = items.Skip(i).Take(batchSize).ToList();
            _context.Items.AddRange(batch);
            await _context.SaveChangesAsync();
            insertedCount += batch.Count;
            
            if (showProgress)
            {
                Console.Write($"\rInserted {insertedCount}/{count} items...");
            }
        }
        
        if (showProgress)
        {
            Console.WriteLine();
            Console.WriteLine($"✓ Successfully inserted {insertedCount} items");
            Console.WriteLine();
        }

        return insertedCount;
    }

    public async Task<int> GetTotalItemCountAsync()
    {
        return await _context.Items.CountAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
