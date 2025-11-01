# MiniDashboard API

A simple CRUD API application built with ASP.NET Core 8.0, following clean architecture principles with layered design.

## Project Structure

```
MiniDashboard/
├── MiniDashboard.Api/              # Main API Project
│   ├── Models/                      # Domain Models Layer
│   │   ├── Entities/                # Entity models
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   └── Common/                  # Common models (WebApiResponse)
│   ├── Repository/                  # Data Access Layer
│   │   └── ItemRepository.cs        # Repository implementations
│   ├── Service/                     # Business Logic Layer
│   │   └── ItemService.cs           # Service implementations
│   └── Controllers/                 # API Controllers
└── MiniDashboard.Tests/             # Unit Tests
```

## Architecture

The project follows a clean layered architecture, with all code organized within the MiniDashboard.Api project using different namespaces and folders:

- **Models Layer** (`MiniDashboard.Api.Models`): Contains entities, DTOs, and common models
- **Repository Layer** (`MiniDashboard.Api.Repository`): Data access using Entity Framework Core with SQLite
- **Service Layer** (`MiniDashboard.Api.Service`): Business logic and orchestration
- **API Layer** (`MiniDashboard.Api.Controllers`): Controllers and API endpoints
- **Tests Layer**: Unit tests for services and controllers

## Features

- RESTful API endpoints for CRUD operations
- SQLite database for data persistence
- Swagger/OpenAPI documentation
- Dependency Injection
- Unit tests with xUnit and Moq

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

4. Access Swagger UI:
- Navigate to `https://localhost:5001` or `http://localhost:5000`
- Swagger UI is available at the root path (configured in Program.cs)

## Database

The application uses SQLite database. The database file `MiniDashboard.db` will be created automatically when the application runs for the first time (using `EnsureCreated()`).

Connection string is configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MiniDashboard.db"
  }
}
```

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

