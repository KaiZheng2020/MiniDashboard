using Microsoft.Extensions.DependencyInjection;
using MiniDashboard.Api.Repository;
using MiniDashboard.Tests.Integration.Helpers;

namespace MiniDashboard.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory<Program> Factory;
    protected MiniDashboardDbContext DbContext { get; private set; } = null!;
    private readonly string _testDbPath;
    
    protected IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        _testDbPath = factory.GetTestDbPath();
    }
    
    public virtual async Task InitializeAsync()
    {
        var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<MiniDashboardDbContext>();
        
        // Recreate database before each test
        await DatabaseTestHelper.RecreateDatabaseAsync(DbContext, _testDbPath);
    }
    
    public virtual async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}
