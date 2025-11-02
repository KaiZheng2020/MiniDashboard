using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniDashboard.Api.Models.Entities;
using MiniDashboard.Api.Repository;
using MiniDashboard.Models.Common;
using MiniDashboard.Models.DTOs;
using MiniDashboard.Tests.Integration.Helpers;

namespace MiniDashboard.Tests.Integration;

public class ItemsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private MiniDashboardDbContext _dbContext = null!;
    private readonly string _testDbPath;
    
    public ItemsApiIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _testDbPath = factory.GetTestDbPath();
    }
    
    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<MiniDashboardDbContext>();
        
        // Recreate database before each test to ensure clean state
        await DatabaseTestHelper.RecreateDatabaseAsync(_dbContext, _testDbPath);
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task GetAllItems_ReturnsOkResult_WithItemsList()
    {
        // Arrange: Create test items
        var items = TestDataBuilder.CreateItems(3, "GetAllItems_Test");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync("/api/items");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Contains(result.Data, dto => dto.Name.Contains("GetAllItems_Test"));
    }
    
    [Fact]
    public async Task GetAllItems_WithPagination_ReturnsPagedResults()
    {
        // Arrange: Create 25 test items
        var items = TestDataBuilder.CreateItems(25, "Pagination_Test");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Request first page with pageSize 10
        var response = await _client.GetAsync("/api/items?page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.Count);
        Assert.NotNull(result.Page);
        Assert.NotNull(result.PageSize);
        Assert.NotNull(result.TotalPages);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.True(result.Total >= 25);
        Assert.True(result.TotalPages >= 3);
    }
    
    [Fact]
    public async Task GetItemById_WhenItemExists_ReturnsOkResult()
    {
        // Arrange: Create a test item
        var item = TestDataBuilder.CreateItem(name: "GetById_Test_Item");
        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();
        var itemId = item.Id;
        
        // Act
        var response = await _client.GetAsync($"/api/items/{itemId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(itemId, result.Data.Id);
        Assert.Equal("GetById_Test_Item", result.Data.Name);
    }
    
    [Fact]
    public async Task GetItemById_WhenItemNotExists_ReturnsNotFound()
    {
        // Act: Request non-existent item
        var response = await _client.GetAsync("/api/items/99999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
        
        Assert.NotNull(result);
        Assert.False(result.Success);
    }
    
    [Fact]
    public async Task CreateItem_ValidData_ReturnsCreatedItem()
    {
        // Arrange
        var request = TestDataBuilder.CreateItemRequestDto(
            name: "New_Integration_Test_Item",
            description: "Integration test description");
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/items", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/api/items/", location, StringComparison.OrdinalIgnoreCase);
        
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("New_Integration_Test_Item", result.Data.Name);
        Assert.Equal("Integration test description", result.Data.Description);
        Assert.True(result.Data.Id > 0);
        
        // Verify item exists in database
        var dbItem = await _dbContext.Items.FindAsync(result.Data.Id);
        Assert.NotNull(dbItem);
        Assert.Equal("New_Integration_Test_Item", dbItem.Name);
    }
    
    [Fact]
    public async Task CreateItem_InvalidData_ReturnsBadRequest()
    {
        // Arrange: Empty name (invalid)
        var request = TestDataBuilder.CreateItemRequestDto(name: "");
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/items", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
        
        Assert.NotNull(result);
        Assert.False(result.Success);
    }
    
    [Fact]
    public async Task UpdateItem_ExistingItem_ReturnsUpdatedItem()
    {
        // Arrange: Create a test item
        var item = TestDataBuilder.CreateItem(
            name: "Original_Name",
            description: "Original Description");
        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();
        var itemId = item.Id;
        var originalUpdatedAt = item.UpdatedAt;
        
        // Wait a bit to ensure UpdatedAt changes
        await Task.Delay(100);
        
        // Act: Update the item
        var request = TestDataBuilder.UpdateItemRequestDto(
            name: "Updated_Name",
            description: "Updated Description");
        
        var response = await _client.PutAsJsonAsync($"/api/items/{itemId}", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(itemId, result.Data.Id);
        Assert.Equal("Updated_Name", result.Data.Name);
        Assert.Equal("Updated Description", result.Data.Description);
        
        // Verify database was updated - use a new context to ensure we get fresh data
        using var scope = _factory.Services.CreateScope();
        var verifyContext = scope.ServiceProvider.GetRequiredService<MiniDashboardDbContext>();
        var dbItem = await verifyContext.Items.FindAsync(itemId);
        Assert.NotNull(dbItem);
        Assert.Equal("Updated_Name", dbItem.Name);
        Assert.True(dbItem.UpdatedAt > originalUpdatedAt);
    }
    
    [Fact]
    public async Task UpdateItem_NonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var request = TestDataBuilder.UpdateItemRequestDto(name: "Updated Name");
        
        // Act
        var response = await _client.PutAsJsonAsync("/api/items/99999", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
        
        Assert.NotNull(result);
        Assert.False(result.Success);
    }
    
    [Fact]
    public async Task UpdateItem_InvalidData_ReturnsBadRequest()
    {
        // Arrange: Create a test item
        var item = TestDataBuilder.CreateItem(name: "Test Item");
        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();
        var itemId = item.Id;
        
        // Arrange: Invalid request (empty name)
        var request = TestDataBuilder.UpdateItemRequestDto(name: "");
        
        // Act
        var response = await _client.PutAsJsonAsync($"/api/items/{itemId}", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteItem_ExistingItem_DeletesSuccessfully()
    {
        // Arrange: Create a test item
        var item = TestDataBuilder.CreateItem(name: "Item_To_Delete");
        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();
        var itemId = item.Id;
        
        // Act
        var response = await _client.DeleteAsync($"/api/items/{itemId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<string>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        
        // Verify item was deleted from database - use a new context with AsNoTracking to ensure we get fresh data
        using var scope = _factory.Services.CreateScope();
        var verifyContext = scope.ServiceProvider.GetRequiredService<MiniDashboardDbContext>();
        // Use AsNoTracking and FirstOrDefaultAsync instead of FindAsync to bypass EF cache
        var dbItem = await verifyContext.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId);
        Assert.Null(dbItem);
    }
    
    [Fact]
    public async Task DeleteItem_NonExistentItem_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/items/99999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task SearchItems_WithQuery_ReturnsMatchingItems()
    {
        // Arrange: Create test items
        var items = new List<Item>
        {
            TestDataBuilder.CreateItem(name: "Apple Product", description: "Red fruit"),
            TestDataBuilder.CreateItem(name: "Banana Product", description: "Yellow fruit"),
            TestDataBuilder.CreateItem(name: "Orange Product", description: "Orange fruit"),
            TestDataBuilder.CreateItem(name: "Grape Product", description: "Purple fruit")
        };
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Search for "Apple"
        var response = await _client.GetAsync("/api/items/search?query=Apple");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Contains(result.Data, dto => dto.Name.Contains("Apple"));
    }
    
    [Fact]
    public async Task SearchItems_WithPagination_ReturnsPagedResults()
    {
        // Arrange: Create 25 test items
        var items = TestDataBuilder.CreateItems(25, "Search_Pagination_Test");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Search with pagination
        var response = await _client.GetAsync("/api/items/search?query=Search_Pagination_Test&page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.Count);
        Assert.NotNull(result.Page);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.True(result.Total >= 25);
    }
    
    [Fact]
    public async Task SearchItems_EmptyQuery_ReturnsAllItems()
    {
        // Arrange: Create test items
        var items = TestDataBuilder.CreateItems(5, "EmptyQuery_Test");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Search with empty query (explicitly pass empty query parameter)
        var response = await _client.GetAsync("/api/items/search?query=");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Should return all items or paginated results
        Assert.Contains(result.Data, dto => dto.Name.Contains("EmptyQuery_Test"));
    }

    [Fact]
    public async Task BatchCreateItems_DirectlyInDatabase_Creates100Items()
    {
        // Arrange: Prepare 100 items
        var items = TestDataBuilder.CreateItems(100, "Batch_Create_Test");
        
        // Act: Add all items directly to database
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Assert: Verify items were created
        var count = await _dbContext.Items.CountAsync(i => i.Name.Contains("Batch_Create_Test"));
        Assert.Equal(100, count);
        
        // Verify we can retrieve them via API
        var response = await _client.GetAsync("/api/items?page=1&pageSize=100");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Contains(result.Data, dto => dto.Name.Contains("Batch_Create_Test"));
    }

    [Fact]
    public async Task BatchCreateItems_ViaApi_Creates100Items()
    {
        // Arrange: Prepare 100 create requests
        var requests = Enumerable.Range(1, 100)
            .Select(i => TestDataBuilder.CreateItemRequestDto(
                name: $"Batch_API_Test_{i}",
                description: $"Batch created via API - Item {i}"))
            .ToList();
        
        // Act: Create items one by one via API
        var createdItems = new List<ItemDto>();
        foreach (var request in requests)
        {
            var response = await _client.PostAsJsonAsync("/api/items", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
            if (result?.Data != null)
            {
                createdItems.Add(result.Data);
            }
        }
        
        // Assert: Verify all items were created
        Assert.Equal(100, createdItems.Count);
        Assert.All(createdItems, item => Assert.True(item.Id > 0));
        Assert.All(createdItems, item => Assert.Contains("Batch_API_Test_", item.Name));
        
        // Verify they exist in database
        var dbCount = await _dbContext.Items.CountAsync(i => i.Name.Contains("Batch_API_Test_"));
        Assert.Equal(100, dbCount);
    }

    [Fact]
    public async Task BatchCreateItems_ForPaginationTesting_CreatesItems()
    {
        // Arrange & Act: Create 100 items for pagination testing
        var items = TestDataBuilder.CreateItems(100, "Pagination_Batch_Test");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Assert: Verify pagination works correctly with batch created items
        var response = await _client.GetAsync("/api/items?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Page);
        Assert.NotNull(result.PageSize);
        Assert.NotNull(result.TotalPages);
        Assert.True(result.Total >= 100); // At least our 100 items
        Assert.Equal(10, result.Data.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }
}
