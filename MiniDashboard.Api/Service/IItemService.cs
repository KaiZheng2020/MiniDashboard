using MiniDashboard.Models.DTOs;

namespace MiniDashboard.Api.Service;

public interface IItemService
{
    Task<List<ItemDto>> GetAllAsync();
    Task<(List<ItemDto> Items, string nextCursor)> GetAllPagedAsync(string encodedCursor, int pageSize);
    Task<(List<ItemDto> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);
    Task<ItemDto?> GetByIdAsync(int id);
    Task<List<ItemDto>> SearchAsync(string query);
    Task<(List<ItemDto> Items, int TotalCount)> SearchPagedAsync(string query, int page, int pageSize);
    Task<ItemDto> CreateAsync(CreateItemRequestDto request);
    Task<ItemDto> UpdateAsync(int id, UpdateItemRequestDto request);
    Task DeleteAsync(int id);
}

