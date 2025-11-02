using MiniDashboard.App.ViewModels;

namespace MiniDashboard.App.Services;

public interface IItemApiService
{
    Task<List<ItemViewModel>> GetAllItemsAsync();
    Task<(List<ItemViewModel> Items, int TotalCount, int Page, int PageSize, int TotalPages)> GetAllItemsPagedAsync(int page, int pageSize);
    Task<ItemViewModel?> GetItemByIdAsync(int id);
    Task<List<ItemViewModel>> SearchItemsAsync(string query);
    Task<(List<ItemViewModel> Items, int TotalCount, int Page, int PageSize, int TotalPages)> SearchItemsPagedAsync(string query, int page, int pageSize);
    Task<ItemViewModel> CreateItemAsync(string name, string? description);
    Task<ItemViewModel> UpdateItemAsync(int id, string name, string? description);
    Task<bool> DeleteItemAsync(int id);
}

