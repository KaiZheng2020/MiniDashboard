using MiniDashboard.Models.DTOs;
using MiniDashboard.Api.Models.Entities;
using MiniDashboard.Api.Repository;
using Microsoft.Extensions.Logging;

namespace MiniDashboard.Api.Service;

public class ItemService : IItemService
{
    private readonly IItemRepository _repository;
    private readonly ILogger<ItemService> _logger;

    public ItemService(IItemRepository repository, ILogger<ItemService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ItemDto>> GetAllAsync()
    {
        try
        {
            var items = await _repository.GetAllAsync();
            return items.Select(item => MapToDto(item, null)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all items");
            throw;
        }
    }

    public async Task<(List<ItemDto> Items, string nextCursor)> GetAllPagedAsync(string encodedCursor, int pageSize)
    {
        try
        {
            var (items, nextCursor) = await _repository.GetAllPagedAsync(encodedCursor, pageSize);
            var itemDtos = items.Select(item => MapToDto(item, nextCursor)).ToList();
            return (itemDtos, nextCursor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged items with cursor. Cursor: {Cursor}, PageSize: {PageSize}", encodedCursor, pageSize);
            throw;
        }
    }

    public async Task<(List<ItemDto> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize)
    {
        try
        {
            var (items, totalCount) = await _repository.GetAllPagedAsync(page, pageSize);
            return (items.Select(item => MapToDto(item, null)).ToList(), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged items. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            throw;
        }
    }

    public async Task<ItemDto?> GetByIdAsync(int id)
    {
        try
        {
            var item = await _repository.GetByIdAsync(id);
            return item == null ? null : MapToDto(item, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item with id: {ItemId}", id);
            throw;
        }
    }

    public async Task<List<ItemDto>> SearchAsync(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }

            var items = await _repository.SearchAsync(query);
            return items.Select(item => MapToDto(item, null)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching items with query: {Query}", query);
            throw;
        }
    }

    public async Task<(List<ItemDto> Items, int TotalCount)> SearchPagedAsync(string query, int page, int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            var (items, totalCount) = await _repository.SearchPagedAsync(query, page, pageSize);
            return (items.Select(item => MapToDto(item, null)).ToList(), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching paged items. Query: {Query}, Page: {Page}, PageSize: {PageSize}", query, page, pageSize);
            throw;
        }
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name is required", nameof(request));
            }

            var item = new Item
            {
                Name = request.Name,
                Description = request.Description
            };

            var createdItem = await _repository.AddAsync(item);
            return MapToDto(createdItem, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
            throw;
        }
    }

    public async Task<ItemDto> UpdateAsync(int id, UpdateItemRequestDto request)
    {
        try
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Item with id {id} not found");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name is required", nameof(request));
            }

            item.Name = request.Name;
            item.Description = request.Description;

            await _repository.UpdateAsync(item);
            return MapToDto(item, null);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item with id: {ItemId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Item with id {id} not found");
            }

            await _repository.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item with id: {ItemId}", id);
            throw;
        }
    }

    private static ItemDto MapToDto(Item item, string? nextCursor = null)
    {
        return new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            NextCursor = nextCursor
        };
    }
}

