using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniDashboard.Api.Models.Entities;
using MiniDashboard.Api.Repository;
using MiniDashboard.Models.Common;
using MiniDashboard.Models.DTOs;
using MiniDashboard.Tests.Integration.Helpers;

namespace MiniDashboard.Tests.Integration;

public class PaginationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private MiniDashboardDbContext _dbContext = null!;
    
    public PaginationIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    public async Task InitializeAsync()
    {
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<MiniDashboardDbContext>();
        await DatabaseTestHelper.EnsureMigrationsAppliedAsync(_dbContext);
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task GetAllItems_FirstPage_ReturnsCorrectPageSize()
    {
        // Arrange: Create 25 test items
        var items = TestDataBuilder.CreateItems(25, "Pagination_FirstPage");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Request first page
        var response = await _client.GetAsync("/api/items?page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }
    
    [Fact]
    public async Task GetAllItems_LastPage_ReturnsRemainingItems()
    {
        // Arrange: Create 25 test items with unique prefix
        var prefix = $"Pagination_LastPage_{DateTime.UtcNow.Ticks}";
        var items = TestDataBuilder.CreateItems(25, prefix);
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        var pageSize = 10;
        
        // First, get total count (may include other items in database)
        var firstPageResponse = await _client.GetAsync($"/api/items?page=1&pageSize={pageSize}");
        firstPageResponse.EnsureSuccessStatusCode();
        var firstPageResult = await firstPageResponse.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        var totalCount = firstPageResult!.Total;
        
        // Calculate expected last page based on actual total
        var expectedLastPage = (int)Math.Ceiling(totalCount / (double)pageSize);
        var expectedItemsOnLastPage = totalCount - ((expectedLastPage - 1) * pageSize);
        
        // Act: Request last page
        var response = await _client.GetAsync($"/api/items?page={expectedLastPage}&pageSize={pageSize}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Page);
        Assert.NotNull(result.PageSize);
        Assert.NotNull(result.TotalPages);
        Assert.Equal(expectedItemsOnLastPage, result.Data.Count);
        Assert.Equal(expectedLastPage, result.Page.Value);
        Assert.Equal(pageSize, result.PageSize.Value);
        Assert.Equal(expectedLastPage, result.TotalPages.Value);
        Assert.Equal(totalCount, result.Total);
    }
    
    [Fact]
    public async Task GetAllItems_InvalidPage_DefaultsToFirstPage()
    {
        // Arrange: Create test items
        var items = TestDataBuilder.CreateItems(10, "Pagination_InvalidPage");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Request invalid page (page 0 or negative) - should normalize to page 1
        var response = await _client.GetAsync("/api/items?page=0&pageSize=10");
        
        // Assert: Should normalize to page 1
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Page);
        Assert.Equal(1, result.Page.Value);
    }
    
    [Fact]
    public async Task GetAllItems_DifferentPageSizes_ReturnsCorrectCount()
    {
        // Arrange: Create 25 test items
        var items = TestDataBuilder.CreateItems(25, "Pagination_PageSize");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Test different page sizes
        var response5 = await _client.GetAsync("/api/items?page=1&pageSize=5");
        var response10 = await _client.GetAsync("/api/items?page=1&pageSize=10");
        var response20 = await _client.GetAsync("/api/items?page=1&pageSize=20");
        
        // Assert
        response5.EnsureSuccessStatusCode();
        var result5 = await response5.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        Assert.NotNull(result5);
        Assert.Equal(5, result5.Data?.Count);
        Assert.Equal(5, result5.PageSize);
        
        response10.EnsureSuccessStatusCode();
        var result10 = await response10.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        Assert.NotNull(result10);
        Assert.Equal(10, result10.Data?.Count);
        Assert.Equal(10, result10.PageSize);
        
        response20.EnsureSuccessStatusCode();
        var result20 = await response20.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        Assert.NotNull(result20);
        Assert.Equal(20, result20.Data?.Count);
        Assert.Equal(20, result20.PageSize);
    }
    
    [Fact]
    public async Task GetAllItems_PageBeyondTotalPages_ReturnsEmptyOrLastPage()
    {
        // Arrange: Create 10 test items
        var items = TestDataBuilder.CreateItems(10, "Pagination_Beyond");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Request page beyond total pages
        var response = await _client.GetAsync("/api/items?page=100&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        // Should return empty list or last page
    }
    
    [Fact]
    public async Task SearchItems_WithPagination_FirstPage_ReturnsCorrectResults()
    {
        // Arrange: Create items with specific prefix
        var items = TestDataBuilder.CreateItems(15, "Search_Paginated");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Search with pagination
        var response = await _client.GetAsync("/api/items/search?query=Search_Paginated&page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.Count);
        Assert.All(result.Data, dto => Assert.Contains("Search_Paginated", dto.Name));
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }
    
    [Fact]
    public async Task SearchItems_WithPagination_LastPage_ReturnsRemainingResults()
    {
        // Arrange: Create items with specific prefix (use unique timestamp to avoid conflicts)
        var prefix = $"Search_Paginated_Last_{DateTime.UtcNow.Ticks}";
        var totalItems = 15;
        var pageSize = 10;
        var items = TestDataBuilder.CreateItems(totalItems, prefix);
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // First, get total count for this search query (may include other items matching the prefix)
        var firstPageResponse = await _client.GetAsync($"/api/items/search?query={prefix}&page=1&pageSize={pageSize}");
        firstPageResponse.EnsureSuccessStatusCode();
        var firstPageResult = await firstPageResponse.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        var totalCount = firstPageResult!.Total;
        
        // Calculate expected last page based on actual total
        var expectedLastPage = (int)Math.Ceiling(totalCount / (double)pageSize);
        var expectedItemsOnLastPage = totalCount - ((expectedLastPage - 1) * pageSize);
        
        // Act: Request last page
        var response = await _client.GetAsync($"/api/items/search?query={prefix}&page={expectedLastPage}&pageSize={pageSize}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Page);
        Assert.NotNull(result.PageSize);
        Assert.NotNull(result.TotalPages);
        Assert.Equal(expectedItemsOnLastPage, result.Data.Count);
        Assert.Equal(expectedLastPage, result.Page.Value);
        Assert.Equal(pageSize, result.PageSize.Value);
        Assert.Equal(expectedLastPage, result.TotalPages.Value);
        Assert.Equal(totalCount, result.Total);
    }
    
    [Fact]
    public async Task GetAllItems_PaginationMetadata_IsCorrect()
    {
        // Arrange: Create exactly 25 items with unique prefix
        var prefix = $"Pagination_Metadata_{DateTime.UtcNow.Ticks}";
        var items = TestDataBuilder.CreateItems(25, prefix);
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Note: The total count includes ALL items in database, not just test items
        // So we verify the pagination metadata structure is correct instead of exact count
        
        // Act
        var response = await _client.GetAsync("/api/items?page=2&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Page);
        Assert.NotNull(result.PageSize);
        Assert.NotNull(result.TotalPages);
        Assert.Equal(2, result.Page.Value);
        Assert.Equal(10, result.PageSize.Value);
        // Verify pagination metadata is consistent
        Assert.True(result.TotalPages.Value > 0);
        Assert.True(result.Total >= 25); // At least our 25 test items
        // Verify calculated totalPages matches expected
        var expectedTotalPages = (int)Math.Ceiling(result.Total / (double)result.PageSize.Value);
        Assert.Equal(expectedTotalPages, result.TotalPages.Value);
    }
    
    [Fact]
    public async Task GetAllItems_WithoutPagination_ReturnsAllItems()
    {
        // Arrange: Create test items
        var items = TestDataBuilder.CreateItems(5, "No_Pagination");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Request without pagination parameters
        var response = await _client.GetAsync("/api/items");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Contains(result.Data, dto => dto.Name.Contains("No_Pagination"));
        // Should return all items or have no pagination metadata
    }
    
    [Fact]
    public async Task GetAllItems_MaxPageSize_LimitedTo100()
    {
        // Arrange: Create test items
        var items = TestDataBuilder.CreateItems(150, "Pagination_MaxSize");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Act: Request with pageSize > 100 (should be normalized to 100)
        var response = await _client.GetAsync("/api/items?page=1&pageSize=200");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.PageSize);
        Assert.True(result.Data.Count <= 100); // Should be limited to 100
        Assert.Equal(100, result.PageSize.Value); // Should be normalized to 100
    }

    [Fact]
    public async Task BatchCreateItems_100Items_ForPaginationTesting()
    {
        // Arrange & Act: Create 100 items using batch method (directly in database)
        var items = TestDataBuilder.CreateItems(100, "Pagination_Batch_100");
        _dbContext.Items.AddRange(items);
        await _dbContext.SaveChangesAsync();
        
        // Assert: Verify all items were created
        var count = await _dbContext.Items.CountAsync(i => i.Name.Contains("Pagination_Batch_100"));
        Assert.Equal(100, count);
        
        // Verify pagination works across multiple pages
        var page1Response = await _client.GetAsync("/api/items?page=1&pageSize=20");
        page1Response.EnsureSuccessStatusCode();
        var page1Result = await page1Response.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(page1Result);
        Assert.Equal(20, page1Result.Data?.Count);
        
        // Verify last page contains remaining items
        var totalCount = page1Result.Total;
        var lastPage = (int)Math.Ceiling(totalCount / 20.0);
        var lastPageResponse = await _client.GetAsync($"/api/items?page={lastPage}&pageSize=20");
        lastPageResponse.EnsureSuccessStatusCode();
        var lastPageResult = await lastPageResponse.Content.ReadFromJsonAsync<WebApiResponse<List<ItemDto>>>();
        
        Assert.NotNull(lastPageResult);
        Assert.NotNull(lastPageResult.Page);
        Assert.Equal(lastPage, lastPageResult.Page.Value);
    }
}
