using MiniDashboard.Api.Models.Entities;
using MiniDashboard.Models.DTOs;

namespace MiniDashboard.Tests.Integration.Helpers;

public static class TestDataBuilder
{
    public static Item CreateItem(
        int? id = null,
        string name = "Test Item",
        string? description = "Test Description",
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new Item
        {
            Id = id ?? 0,
            Name = name,
            Description = description,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }
    
    public static List<Item> CreateItems(int count, string prefix = "Test Item")
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateItem(name: $"{prefix} {i}", description: $"Description for {prefix} {i}"))
            .ToList();
    }
    
    public static CreateItemRequestDto CreateItemRequestDto(
        string name = "New Test Item",
        string? description = "New Test Description")
    {
        return new CreateItemRequestDto
        {
            Name = name,
            Description = description
        };
    }
    
    public static UpdateItemRequestDto UpdateItemRequestDto(
        string name = "Updated Test Item",
        string? description = "Updated Test Description")
    {
        return new UpdateItemRequestDto
        {
            Name = name,
            Description = description
        };
    }
}
