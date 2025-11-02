using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using MiniDashboard.App.Services;
using MiniDashboard.App.ViewModels;
using MiniDashboard.App.Views;
using Serilog;

namespace MiniDashboard.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Load configuration first
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Configure Serilog from configuration
        ConfigureSerilog(_configuration);

        Log.Information("=== MiniDashboard Application Starting ===");
        Log.Information("Application startup initiated");

        try
        {
            base.OnStartup(e);

            Log.Information("Configuration loaded successfully");

            var services = new ServiceCollection();
            ConfigureServices(services, _configuration);
            _serviceProvider = services.BuildServiceProvider();

            Log.Information("Dependency injection container configured");

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Log.Information("Main window displayed");
            Log.Information("=== Application Started Successfully ===");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    private void ConfigureSerilog(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Get API URL from configuration
        var baseUrl = configuration["ApiSettings:BaseUrl"] 
            ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured in appsettings.json");

        Log.Information("Configuring API client with base URL: {BaseUrl}", baseUrl);

        services.AddHttpClient<IItemApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddTypedClient<IItemApiService>((HttpClient httpClient, IServiceProvider _) => 
            new ItemApiService(httpClient));

        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddSerilog();
        });

        // Register ViewModels
        services.AddTransient<MainViewModel>();

        // Register Views
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("=== Application Shutting Down ===");
        Log.Information("Application exit initiated with exit code: {ExitCode}", e.ApplicationExitCode);

        try
        {
            _serviceProvider?.Dispose();
            Log.Information("Dependency injection container disposed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error disposing service provider: {ErrorMessage}", ex.Message);
        }

        base.OnExit(e);

        Log.Information("=== Application Exited ===");
        Log.CloseAndFlush();
    }
}

