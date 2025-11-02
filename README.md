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

- Entity Framework Core 8.0.13
- SQLite database provider
- Swashbuckle.AspNetCore for Swagger
- xUnit for testing
- Moq for mocking

## Design Patterns

- **Repository Pattern**: Abstracts data access logic
- **Dependency Injection**: Used throughout the application
- **DTO Pattern**: Separates API contracts from entities
- **Service Layer Pattern**: Encapsulates business logic

