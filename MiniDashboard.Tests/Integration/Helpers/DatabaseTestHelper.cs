using Microsoft.EntityFrameworkCore;
using MiniDashboard.Api.Models.Entities;
using MiniDashboard.Api.Repository;

namespace MiniDashboard.Tests.Integration.Helpers;

public static class DatabaseTestHelper
{
    /// <summary>
    /// Deletes SQLite WAL and SHM files from file system
    /// These files are created by SQLite for Write-Ahead Logging and may not be deleted by EnsureDeletedAsync
    /// </summary>
    public static void DeleteTestDatabase(string dbPath)
    {
        // Delete SQLite WAL and SHM files if they exist (ignore errors)
        // Note: EnsureDeletedAsync handles the main .db file, but may leave WAL/SHM files
        var walPath = dbPath + "-wal";
        var shmPath = dbPath + "-shm";
        
        if (File.Exists(walPath))
        {
            try
            {
                File.Delete(walPath);
            }
            catch { /* Ignore errors when deleting WAL file */ }
        }
        
        if (File.Exists(shmPath))
        {
            try
            {
                File.Delete(shmPath);
            }
            catch { /* Ignore errors when deleting SHM file */ }
        }
    }
    
    /// <summary>
    /// Ensures the test database is ready for testing.
    /// If the database file exists, clears the Items table.
    /// If the database file doesn't exist, creates the database and applies migrations.
    /// </summary>
    public static async Task RecreateDatabaseAsync(MiniDashboardDbContext context, string dbPath)
    {
        // Ensure migrations are applied first
        await context.Database.MigrateAsync();
        
        // Check if database file exists (after migration, it should exist now)
        if (File.Exists(dbPath))
        {
            // Database exists: clear Items table using SQL for efficiency
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Items");
        }
    }
    
    /// <summary>
    /// Ensures database migrations are applied
    /// </summary>
    public static async Task EnsureMigrationsAppliedAsync(MiniDashboardDbContext context)
    {
        await context.Database.MigrateAsync();
    }
}

