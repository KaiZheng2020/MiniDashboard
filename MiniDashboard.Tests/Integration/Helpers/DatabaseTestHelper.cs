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
    /// Recreates the test database (deletes old database and applies migrations)
    /// If the database exists, it will be completely deleted and recreated.
    /// </summary>
    public static async Task RecreateDatabaseAsync(MiniDashboardDbContext context, string dbPath)
    {
        // Step 1: Delete database via EF Core (deletes the .db file)
        await context.Database.EnsureDeletedAsync();
        
        // Step 2: Delete SQLite WAL and SHM files if they exist
        // EnsureDeletedAsync may not delete these auxiliary files
        DeleteTestDatabase(dbPath);
        
        // Step 3: Recreate database and apply migrations (creates new database)
        await context.Database.MigrateAsync();
    }
    
    /// <summary>
    /// Ensures database migrations are applied
    /// </summary>
    public static async Task EnsureMigrationsAppliedAsync(MiniDashboardDbContext context)
    {
        await context.Database.MigrateAsync();
    }
}

