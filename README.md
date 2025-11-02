# MiniDashboard

A WPF desktop application with a Web API backend, demonstrating clean MVVM architecture and layered design principles.

## Project Structure

```
MiniDashboard/
├── MiniDashboard.Api/              # ASP.NET Core Web API Backend
│   ├── Models/                      # Domain Models Layer
│   │   └── Entities/                # Entity models
│   ├── Repository/                  # Data Access Layer
│   │   └── ItemRepository.cs        # Repository implementations
│   ├── Service/                     # Business Logic Layer
│   │   └── ItemService.cs           # Service implementations
│   ├── Migrations/                  # EF Core Migrations
│   └── Controllers/                 # API Controllers
├── MiniDashboard.Models/            # Shared Models Library
│   ├── DTOs/                        # Data Transfer Objects
│   └── Common/                      # Common models (WebApiResponse, PagedResponse)
├── MiniDashboard.App/              # WPF Desktop Application
│   ├── ViewModels/                  # MVVM ViewModels
│   ├── Views/                       # WPF Views (XAML)
│   ├── Services/                    # API Client Services
│   ├── Commands/                    # Command Implementations
│   └── Utils/                       # Value Converters
├── MiniDashboard.Scripts/          # Database Seeding Script
│   ├── Program.cs                   # Entry point
│   └── DatabaseSeeder.cs            # Database seeding logic
└── MiniDashboard.Tests/            # Test Projects
    ├── Integration/                 # Integration Tests
    └── Unit/                        # Unit Tests
```

## Architecture

### Backend (API)
The API follows a clean layered architecture:

- **Models Layer** (`MiniDashboard.Api.Models.Entities`): Entity models (domain entities)
- **Shared Models** (`MiniDashboard.Models`): Shared DTOs and common response models used by both API and App
- **Repository Layer** (`MiniDashboard.Api.Repository`): Data access using Entity Framework Core with SQLite
- **Service Layer** (`MiniDashboard.Api.Service`): Business logic and orchestration
- **API Layer** (`MiniDashboard.Api.Controllers`): Controllers and API endpoints

### Frontend (WPF Application)
The WPF application strictly follows the MVVM pattern:

- **ViewModels** (`MiniDashboard.App.ViewModels`): Business logic and state management (no code-behind)
- **Views** (`MiniDashboard.App.Views`): XAML-only views with data binding
- **Services** (`MiniDashboard.App.Services`): HTTP client services for API communication
- **Commands** (`MiniDashboard.App.Commands`): RelayCommand and AsyncRelayCommand implementations
- **Utils** (`MiniDashboard.App.Utils`): Value converters for data binding

### Shared Models Library
The `MiniDashboard.Models` project contains shared models used by both API and WPF App:

- **DTOs**: Data Transfer Objects (ItemDto)
- **Common**: Common response models (WebApiResponse, PagedResponse)

## Features

### Backend (API)
- RESTful API endpoints for CRUD operations
- **Server-side pagination** with customizable page size (default: 10, max: 100)
- SQLite database for data persistence
- Swagger/OpenAPI documentation
- Dependency Injection
- Global exception handling middleware
- **Serilog logging** with file and console output
- Comprehensive logging for all API endpoints (request start, success, error)
- Configuration-driven logging via `appsettings.json`
- Unit tests with xUnit and Moq
- Integration tests with WebApplicationFactory and real database

### Frontend (WPF Application)
- Full MVVM pattern implementation (no code-behind logic)
- CRUD operations: View, Add, Edit, Delete items
- Real-time search functionality with pagination
- **Toggle sort functionality**: Click once for ascending, click again for descending (Name, Created Date, Updated Date)
- **Pagination controls**: First, Previous, Next, Last buttons with page size selector (5, 10, 20, 50)
- **Page information display**: Shows current page, total pages, and total item count
- Loading indicators and error handling
- Responsive UI with modern design
- ObservableCollection for data binding
- Async/await patterns for API calls
- Dependency Injection configuration
- **Serilog logging** with file and console output
- **Logging for all user actions** (startup, shutdown, CRUD operations, pagination, sorting)
- **Configuration-driven settings** (API URL and logging via `appsettings.json`)

## API Endpoints

- `GET /api/items` - Get all items
  - Optional query parameters: `?page=1&pageSize=10` (pagination)
- `GET /api/items/{id}` - Get item by ID
- `GET /api/items/search?query=xyz` - Search items
  - Optional query parameters: `?query=xyz&page=1&pageSize=10` (pagination)
- `POST /api/items` - Create new item
- `PUT /api/items/{id}` - Update existing item
- `DELETE /api/items/{id}` - Delete item

**Pagination Notes:**
- `page`: Page number (default: 1, minimum: 1)
- `pageSize`: Items per page (default: 10, minimum: 1, maximum: 100)
- Response includes pagination metadata: `TotalCount`, `Page`, `PageSize`, `TotalPages`

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

## Database Seeding Script

The `MiniDashboard.Scripts` project provides a command-line tool to seed the database with test data.

### Usage

**Generate items:**
```bash
cd MiniDashboard.Scripts
dotnet run -- -generate 100
# Or use short form:
dotnet run -- -g 100
```

**Clear existing data and generate new items:**
```bash
dotnet run -- -generate 500 --clear
# Or use short forms:
dotnet run -- -g 500 -c
```

**Command-line Arguments:**
- `-generate <count>` or `-g <count>`: Generate the specified number of items
- `--clear` or `-c`: Clear all existing data before generating new items

**How it works:**
1. The script automatically locates the `MiniDashboard.Api` project directory
2. Reads the database connection string from `appsettings.json`
3. Applies migrations if needed (handles existing databases gracefully)
4. Clears existing data if `--clear` flag is used
5. Generates and inserts test items in batches

**Note:** The script uses the same database configuration as the API, so generated data will be visible in both the API and WPF application.

## Running Tests

**Run all tests:**
```bash
dotnet test
```

**Run only unit tests:**
```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

**Run only integration tests:**
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

**Run tests with coverage:**
```bash
dotnet test /p:CollectCoverage=true
```

**Test Structure:**
- **Unit Tests** (`MiniDashboard.Tests/Unit`): Test individual components in isolation using mocks
- **Integration Tests** (`MiniDashboard.Tests/Integration`): Test the full API stack with a real SQLite test database
  - Uses `WebApplicationFactory<T>` for in-memory testing
  - Each test gets a fresh database instance
  - Tests pagination, CRUD operations, and error scenarios

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

### Shared Models Library
- Shared DTOs and response models
- Used by both API and WPF App projects
- Reduces code duplication and ensures consistency

### Testing
- xUnit for test framework
- Moq for mocking dependencies
- Microsoft.AspNetCore.Mvc.Testing for integration testing
- Entity Framework Core Design tools for migrations in tests

## Design Patterns

- **Repository Pattern**: Abstracts data access logic
- **Dependency Injection**: Used throughout the application
- **DTO Pattern**: Separates API contracts from entities, shared between projects
- **Service Layer Pattern**: Encapsulates business logic
- **MVVM Pattern**: Strict separation of concerns in WPF application
- **Configuration Pattern**: Externalized configuration via `appsettings.json`
- **Shared Library Pattern**: Common models in `MiniDashboard.Models` for reuse
- **Test Factory Pattern**: `WebApplicationFactory<T>` for integration testing

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

All configurations support environment-specific overrides (e.g., `appsettings.Development.json`, `appsettings.Test.json`).

### UI Features

**Pagination:**
- Page size selector: 5, 10, 20, 50 items per page
- Navigation buttons: First, Previous, Next, Last
- Page information display showing current page, total pages, and total item count
- Pagination controls automatically hide when there's only one page

**Sorting:**
- Toggle sorting: Click once for ascending order, click again for descending order
- Sortable fields: Name, Created Date, Updated Date
- Sort state persists when switching between different sort buttons
- Clicking a different sort button resets to ascending for that field

**Column Widths:**
- Name and Description columns are wider for better readability
- Date columns (Created At, Updated At) are compact
- Responsive layout adapts to window size

