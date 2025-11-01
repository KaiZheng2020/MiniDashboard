using MiniDashboard.Api.Models.DTOs;
using MiniDashboard.Api.Models.Entities;
using MiniDashboard.Api.Repository;
using MiniDashboard.Api.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MiniDashboard.Tests;

public class ItemServiceTests
{
    private readonly Mock<IItemRepository> _mockRepository;
    private readonly Mock<ILogger<ItemService>> _mockLogger;
    private readonly ItemService _service;

    public ItemServiceTests()
    {
        _mockRepository = new Mock<IItemRepository>();
        _mockLogger = new Mock<ILogger<ItemService>>();
        _service = new ItemService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = 1, Name = "Item 1", Description = "Description 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Item { Id = 2, Name = "Item 2", Description = "Description 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Item 1", result[0].Name);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemExists_ReturnsItem()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Item 1", Description = "Description 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Item 1", result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemNotFound_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Item?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithQuery_ReturnsMatchingItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = 1, Name = "Test Item", Description = "Description", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        _mockRepository.Setup(r => r.SearchAsync("Test")).ReturnsAsync(items);

        // Act
        var result = await _service.SearchAsync("Test");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Item", result[0].Name);
        _mockRepository.Verify(r => r.SearchAsync("Test"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesItem()
    {
        // Arrange
        var request = new CreateItemRequestDto { Name = "New Item", Description = "New Description" };
        var createdItem = new Item { Id = 1, Name = "New Item", Description = "New Description", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Item>())).ReturnsAsync(createdItem);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Item", result.Name);
        Assert.Equal("New Description", result.Description);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateItemRequestDto { Name = "", Description = "Description" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _service.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesItem()
    {
        // Arrange
        var existingItem = new Item { Id = 1, Name = "Old Name", Description = "Old Description", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var request = new UpdateItemRequestDto { Name = "New Name", Description = "New Description" };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingItem);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Item>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Description", result.Description);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateItemRequestDto { Name = "New Name", Description = "New Description" };
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Item?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.UpdateAsync(999, request));
    }

    [Fact]
    public async Task DeleteAsync_WhenItemExists_DeletesItem()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Item 1", Description = "Description", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(item);
        _mockRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Item?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _service.DeleteAsync(999));
    }
}

