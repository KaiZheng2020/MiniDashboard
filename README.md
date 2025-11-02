# MiniDashboard

A WPF desktop application with a Web API backend, demonstrating clean MVVM architecture and layered design principles.

## Project Structure

```
MiniDashboard/
├── MiniDashboard.Api/              # ASP.NET Core Web API Backend
│   ├── Models/                      # Domain Models Layer
│   │   ├── Entities/                # Entity models
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   └── Common/                  # Common models (WebApiResponse)
│   ├── Repository/                  # Data Access Layer
│   │   └── ItemRepository.cs        # Repository implementations
│   ├── Service/                     # Business Logic Layer
│   │   └── ItemService.cs           # Service implementations
│   ├── Migrations/                  # EF Core Migrations
│   └── Controllers/                 # API Controllers
├── MiniDashboard.App/              # WPF Desktop Application
│   ├── Models/                      # View Models
│   ├── ViewModels/                  # MVVM ViewModels
│   ├── Views/                       # WPF Views (XAML)
│   ├── Services/                    # API Client Services
│   ├── Commands/                    # Command Implementations
│   └── Converters/                 # Value Converters
└── MiniDashboard.Tests/             # Unit Tests
```

## Architecture

### Backend (API)
The API follows a clean layered architecture:

- **Models Layer** (`MiniDashboard.Api.Models`): Contains entities, DTOs, and common models
- **Repository Layer** (`MiniDashboard.Api.Repository`): Data access using Entity Framework Core with SQLite
- **Service Layer** (`MiniDashboard.Api.Service`): Business logic and orchestration
- **API Layer** (`MiniDashboard.Api.Controllers`): Controllers and API endpoints

### Frontend (WPF Application)
The WPF application strictly follows the MVVM pattern:

- **Models** (`MiniDashboard.App.Models`): View models with INotifyPropertyChanged
- **ViewModels** (`MiniDashboard.App.ViewModels`): Business logic and state management (no code-behind)
- **Views** (`MiniDashboard.App.Views`): XAML-only views with data binding
- **Services** (`MiniDashboard.App.Services`): HTTP client services for API communication
- **Commands** (`MiniDashboard.App.Commands`): RelayCommand and AsyncRelayCommand implementations
- **Converters** (`MiniDashboard.App.Converters`): Value converters for data binding

## Features

### Backend (API)
- RESTful API endpoints for CRUD operations
- SQLite database for data persistence
- Swagger/OpenAPI documentation
- Dependency Injection
- Global exception handling middleware
- **Serilog logging** with file and console output
- Comprehensive logging for all API endpoints (request start, success, error)
- Configuration-driven logging via `appsettings.json`
- Unit tests with xUnit and Moq

### Frontend (WPF Application)
- Full MVVM pattern implementation (no code-behind logic)
- CRUD operations: View, Add, Edit, Delete items
- Real-time search functionality
- Sort by Name, Created Date, or Updated Date
- Loading indicators and error handling
- Responsive UI with modern design
- ObservableCollection for data binding
- Async/await patterns for API calls
- Dependency Injection configuration
- **Serilog logging** with file and console output
- **Logging for all user actions** (startup, shutdown, CRUD operations)
- **Configuration-driven settings** (API URL and logging via `appsettings.json`)

## API Endpoints

- `GET /api/items` - Get all items
- `GET /api/items/{id}` - Get item by ID
- `GET /api/items/search?query=xyz` - Search items
- `POST /api/items` - Create new item
- `PUT /api/items/{id}` - Update existing item
- `DELETE /api/items/{id}` - Delete item

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code

## Setup and Run

1. Restore packages:
```bash
dotnet restore
```

2. Build the solution:
```bash
dotnet build
```

3. Run the API:
```bash
cd MiniDashboard.Api
dotnet run
```

The API will start on `https://localhost:10133` (check `launchSettings.json` for the actual port).

4. Access Swagger UI:
- Navigate to `https://localhost:10133` or the URL shown in the console
- Swagger UI is available at the root path (configured in Program.cs)

5. Run the WPF Application:
```bash
cd MiniDashboard.App
dotnet run
```

**Important**: Make sure the API is running before starting the WPF application, as it depends on the API for data operations.

## Configuration

### API Configuration (`MiniDashboard.Api/appsettings.json`)

The API configuration file contains:

- **ConnectionStrings**: SQLite database connection string
- **Serilog**: Logging configuration (console and file output)

Example:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MiniDashboard.db"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/minidashboard-api-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760,
          "rollOnFileSizeLimit": true
        }
      }
    ]
  }
}
```

### WPF Application Configuration (`MiniDashboard.App/appsettings.json`)

The WPF application configuration file contains:

- **ApiSettings**: API base URL (configurable)
- **Serilog**: Logging configuration (console and file output)

Example:
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:10133"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/minidashboard-app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760,
          "rollOnFileSizeLimit": true
        }
      }
    ]
  }
}
```

**Note**: To change the API URL, update `ApiSettings:BaseUrl` in `MiniDashboard.App/appsettings.json`.

## Logging

Both the API and WPF application use **Serilog** for structured logging with configuration-driven setup.

### Log Output

**API Logs**:
- Console output: Real-time logging during development
- File output: `MiniDashboard.Api/logs/minidashboard-api-YYYYMMDD.log`

**WPF Application Logs**:
- Console output: Real-time logging during development
- File output: `MiniDashboard.App/bin/Debug/net8.0-windows/logs/minidashboard-app-YYYYMMDD.log`

### Log Features

- **Log Levels**: Information, Warning, Error, Fatal
- **File Rolling**: Daily rotation with 30-day retention
- **File Size Limit**: 10MB per file with automatic rollover
- **Structured Logging**: JSON-friendly format with timestamps and context
- **API Logging**: All endpoints log request start, success, and error states
- **WPF Logging**: Application lifecycle (startup, shutdown) and all user actions

### Configuring Log Levels

Edit the `Serilog.MinimumLevel` section in `appsettings.json` to adjust logging verbosity:

```json
"MinimumLevel": {
  "Default": "Information",  // Change to "Debug" for more details
  "Override": {
    "Microsoft": "Warning",
    "System": "Warning"
  }
}
```

Available levels: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`

## Database

The application uses SQLite database with Entity Framework Core Code First approach and Migrations. You need to manually apply migrations to create or update the database.

Connection string is configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MiniDashboard.db"
  }
}
```

### Database Migrations

This project uses EF Core Migrations to manage database schema changes. When you modify entity models, you need to create and apply migrations manually.

**Initial Setup (First Time):**

1. Apply all pending migrations to create the database:
```bash
cd MiniDashboard.Api
dotnet ef database update --context MiniDashboardDbContext
```

**Creating a new migration:**

When you modify entity models, create a new migration:
```bash
cd MiniDashboard.Api
dotnet ef migrations add <MigrationName> --context MiniDashboardDbContext
```

Example:
```bash
dotnet ef migrations add AddPriceToItem
```

**Applying migrations:**

After creating a migration, manually apply it to update the database:
```bash
cd MiniDashboard.Api
dotnet ef database update --context MiniDashboardDbContext
```

**Other useful migration commands:**
- List all migrations: `dotnet ef migrations list`
- Revert to a specific migration: `dotnet ef database update <PreviousMigrationName>`
- Remove the last migration (if not applied): `dotnet ef migrations remove`

**Note:** Make sure you have the EF Core tools installed. If not, install them with:
```bash
dotnet tool install --global dotnet-ef
```

**Important:** The application does NOT automatically apply migrations. You must manually run `dotnet ef database update` after creating or modifying migrations.

## Running Tests

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## Project References

### Backend (API)
- Entity Framework Core 8.0.13
- SQLite database provider
- Swashbuckle.AspNetCore for Swagger
- Serilog for logging
- Serilog.Settings.Configuration for configuration-driven logging
- Serilog.Sinks.Console and Serilog.Sinks.File for log output
- xUnit for testing
- Moq for mocking

### Frontend (WPF Application)
- Microsoft.Extensions.DependencyInjection for DI
- Microsoft.Extensions.Http for HTTP client
- Microsoft.Extensions.Configuration for configuration management
- Microsoft.Extensions.Logging for logging abstraction
- Serilog for structured logging
- Serilog.Extensions.Logging for ILogger integration
- Serilog.Settings.Configuration for configuration-driven logging
- Serilog.Sinks.Console and Serilog.Sinks.File for log output

## Design Patterns

- **Repository Pattern**: Abstracts data access logic
- **Dependency Injection**: Used throughout the application
- **DTO Pattern**: Separates API contracts from entities
- **Service Layer Pattern**: Encapsulates business logic
- **MVVM Pattern**: Strict separation of concerns in WPF application
- **Configuration Pattern**: Externalized configuration via `appsettings.json`

## Development Notes

### Logging Best Practices

1. **API Logging**: All controllers use `ILogger<T>` to log:
   - Request initiation (Information level)
   - Successful operations with context data
   - Errors with full exception details

2. **WPF Logging**: ViewModels use `ILogger<T>` to log:
   - Application lifecycle events (startup, shutdown)
   - All user actions (LoadItems, AddItem, EditItem, SaveItem, DeleteItem)
   - Errors with full exception details

3. **Configuration**: Both projects use configuration-driven logging, allowing log levels and outputs to be changed without recompiling.

### Configuration Management

- **API URL**: Configured in `MiniDashboard.App/appsettings.json` under `ApiSettings:BaseUrl`
- **Database Connection**: Configured in `MiniDashboard.Api/appsettings.json` under `ConnectionStrings:DefaultConnection`
- **Logging**: Configured in both `appsettings.json` files under `Serilog` section

All configurations support environment-specific overrides (e.g., `appsettings.Development.json`).

