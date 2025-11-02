using MiniDashboard.Api.Controllers;
using MiniDashboard.Models.Common;
using MiniDashboard.Models.DTOs;
using MiniDashboard.Api.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MiniDashboard.Tests;

public class ItemsControllerTests
{
    private readonly Mock<IItemService> _mockService;
    private readonly Mock<ILogger<ItemsController>> _mockLogger;
    private readonly ItemsController _controller;

    public ItemsControllerTests()
    {
        _mockService = new Mock<IItemService>();
        _mockLogger = new Mock<ILogger<ItemsController>>();
        _controller = new ItemsController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllItems_ReturnsOkResult_WithItems()
    {
        // Arrange
        var items = new List<ItemDto>
        {
            new ItemDto { Id = 1, Name = "Item 1", Description = "Description 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new ItemDto { Id = 2, Name = "Item 2", Description = "Description 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _controller.GetAllItems();

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<List<ItemDto>>>>(result);
        var actionResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<List<ItemDto>>>(actionResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task GetItemById_WhenItemExists_ReturnsOkResult()
    {
        // Arrange
        var item = new ItemDto { Id = 1, Name = "Item 1", Description = "Description 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _controller.GetItemById(1);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<ItemDto>>>(result);
        var actionResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<ItemDto>>(actionResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.Id);
    }

    [Fact]
    public async Task GetItemById_WhenItemNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ItemDto?)null);

        // Act
        var result = await _controller.GetItemById(999);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<ItemDto>>>(result);
        var actionResult = Assert.IsType<NotFoundObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<ItemDto>>(actionResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task SearchItems_WithQuery_ReturnsOkResult()
    {
        // Arrange
        var items = new List<ItemDto>
        {
            new ItemDto { Id = 1, Name = "Test Item", Description = "Description", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _mockService.Setup(s => s.SearchAsync("Test")).ReturnsAsync(items);

        // Act
        var result = await _controller.SearchItems("Test");

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<List<ItemDto>>>>(result);
        var actionResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<List<ItemDto>>>(actionResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task CreateItem_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateItemRequestDto { Name = "New Item", Description = "New Description" };
        var createdItem = new ItemDto { Id = 1, Name = "New Item", Description = "New Description", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.CreateAsync(request)).ReturnsAsync(createdItem);

        // Act
        var result = await _controller.CreateItem(request);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<ItemDto>>>(result);
        var actionResult = Assert.IsType<CreatedAtActionResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<ItemDto>>(actionResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("New Item", response.Data.Name);
    }

    [Fact]
    public async Task CreateItem_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateItemRequestDto { Name = "", Description = "Description" };
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateItem(request);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<ItemDto>>>(result);
        var actionResult = Assert.IsType<BadRequestObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<ItemDto>>(actionResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateItem_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new UpdateItemRequestDto { Name = "Updated Item", Description = "Updated Description" };
        var updatedItem = new ItemDto { Id = 1, Name = "Updated Item", Description = "Updated Description", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _mockService.Setup(s => s.UpdateAsync(1, request)).ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateItem(1, request);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<ItemDto>>>(result);
        var actionResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<ItemDto>>(actionResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Updated Item", response.Data.Name);
    }

    [Fact]
    public async Task UpdateItem_WhenItemNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        var request = new UpdateItemRequestDto { Name = "Updated Item", Description = "Updated Description" };
        _mockService.Setup(s => s.UpdateAsync(999, request))
            .ThrowsAsync(new KeyNotFoundException("Item with id 999 not found"));

        // Act
        var result = await _controller.UpdateItem(999, request);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<ItemDto>>>(result);
        var actionResult = Assert.IsType<NotFoundObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<ItemDto>>(actionResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteItem_WhenItemExists_ReturnsOkResult()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteItem(1);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<string>>>(result);
        var actionResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<string>>(actionResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteItem_WhenItemNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(999))
            .ThrowsAsync(new KeyNotFoundException("Item with id 999 not found"));

        // Act
        var result = await _controller.DeleteItem(999);

        // Assert
        var okResult = Assert.IsType<ActionResult<WebApiResponse<string>>>(result);
        var actionResult = Assert.IsType<NotFoundObjectResult>(okResult.Result);
        var response = Assert.IsType<WebApiResponse<string>>(actionResult.Value);
        Assert.False(response.Success);
    }
}

