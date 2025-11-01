using MiniDashboard.Api.Models.Common;
using MiniDashboard.Api.Middleware;
using MiniDashboard.Api.Repository;
using MiniDashboard.Api.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
var services = builder.Services;

// Database connection string
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=MiniDashboard.db";

// Services
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiniDashboard API",
        Version = "v1",
        Description = "A simple CRUD API for managing items"
    });
});

// Database context with SQLite
services.AddDbContext<MiniDashboardDbContext>(options =>
    options.UseSqlite(connectionString));

// Dependency injection
services.AddScoped<IItemRepository, ItemRepository>();
services.AddScoped<IItemService, ItemService>();

// Custom model validation error response
services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
        var response = WebApiResponse<string>.Fail("Validation failed: " + string.Join("; ", errors));
        return new BadRequestObjectResult(response);
    };
});

var app = builder.Build();

// Global exception handler middleware (should be first in the pipeline)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Development middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniDashboard API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

