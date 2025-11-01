using MiniDashboard.Api.Models.Entities;

namespace MiniDashboard.Api.Repository;

public interface IItemRepository
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<List<Item>> SearchAsync(string query);
    Task<Item> AddAsync(Item item);
    Task UpdateAsync(Item item);
    Task DeleteAsync(int id);
}

