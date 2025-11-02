using MiniDashboard.App.Models;

namespace MiniDashboard.App.Services;

public interface IItemApiService
{
    Task<List<ItemViewModel>> GetAllItemsAsync();
    Task<ItemViewModel?> GetItemByIdAsync(int id);
    Task<List<ItemViewModel>> SearchItemsAsync(string query);
    Task<ItemViewModel> CreateItemAsync(string name, string? description);
    Task<ItemViewModel> UpdateItemAsync(int id, string name, string? description);
    Task<bool> DeleteItemAsync(int id);
}

