namespace MiniDashboard.Scripts;

class Program
{
    static async Task Main(string[] args)
    {
        // Parse command line arguments
        var count = 100;
        var clear = false;
        
        // Support formats: -generate 100, --generate 100, -g 100
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            if ((arg.Equals("-generate", StringComparison.OrdinalIgnoreCase) ||
                 arg.Equals("--generate", StringComparison.OrdinalIgnoreCase) ||
                 arg.Equals("-g", StringComparison.OrdinalIgnoreCase)) &&
                i + 1 < args.Length &&
                int.TryParse(args[i + 1], out var parsedCount))
            {
                count = parsedCount;
                i++; // Skip the next argument as we've processed it
            }
            else if (arg.Equals("--clear", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("-c", StringComparison.OrdinalIgnoreCase))
            {
                clear = true;
            }
            else if (int.TryParse(arg, out var directCount))
            {
                // Support direct number: 100
                count = directCount;
            }
        }

        Console.WriteLine("=== MiniDashboard Database Seed Script ===");
        Console.WriteLine($"Count: {count}");
        Console.WriteLine($"Clear existing data: {clear}");
        Console.WriteLine();

        DatabaseSeeder? seeder = null;
        try
        {
            // Create seeder (loads config and ensures database is ready)
            seeder = await DatabaseSeeder.CreateAsync();
            
            Console.WriteLine($"Database path: {seeder.DbPath}");
            Console.WriteLine("✓ Database ready");
            Console.WriteLine();

            // Clear existing data if requested
            if (clear)
            {
                Console.WriteLine("Clearing existing data...");
                var deletedCount = await seeder.ClearAllItemsAsync();
                Console.WriteLine($"✓ Deleted {deletedCount} existing items");
                Console.WriteLine();
            }
            else
            {
                var existingCount = await seeder.GetCurrentItemCountAsync();
                Console.WriteLine($"Current items in database: {existingCount}");
                Console.WriteLine();
            }

            // Generate and insert test data
            await seeder.SeedItemsAsync(count);

            // Verify
            var totalCount = await seeder.GetTotalItemCountAsync();
            Console.WriteLine($"Total items in database: {totalCount}");
            Console.WriteLine();
            Console.WriteLine("=== Seed completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
        finally
        {
            seeder?.Dispose();
        }
    }
}