using MiniDashboard.Api.Models.Entities;

namespace MiniDashboard.Api.Repository;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<(List<Item> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize);
    Task<Item?> GetByIdAsync(int id);
    Task<List<Item>> SearchAsync(string query);
    Task<(List<Item> Items, int TotalCount)> SearchPagedAsync(string query, int page, int pageSize);
    Task<Item> AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
}

