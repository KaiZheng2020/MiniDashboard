using MiniDashboard.Models.DTOs;

namespace MiniDashboard.Api.Service;

public interface IItemService
{
    Task<List<ItemDto>> GetAllAsync();
    Task<ItemDto?> GetByIdAsync(int id);
    Task<List<ItemDto>> SearchAsync(string query);
    Task<ItemDto> CreateAsync(CreateItemRequestDto request);
    Task<ItemDto> UpdateAsync(int id, UpdateItemRequestDto request);
    Task DeleteAsync(int id);
}

