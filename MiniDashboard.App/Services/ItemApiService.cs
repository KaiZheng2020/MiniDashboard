using System.Net.Http;
using System.Net.Http.Json;
using MiniDashboard.Api.Models.Common;
using MiniDashboard.Api.Models.DTOs;
using MiniDashboard.App.Models;

namespace MiniDashboard.App.Services;

public class ItemApiService : IItemApiService
{
    private readonly HttpClient _httpClient;

    public ItemApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ItemViewModel>> GetAllItemsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<WebApiResponse<List<ItemDto>>>("/api/items");
            if (response?.Success == true && response.Data != null)
            {
                return response.Data.Select(ItemViewModel.FromDto).ToList();
            }
            return new List<ItemViewModel>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
    }

    public async Task<ItemViewModel?> GetItemByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<WebApiResponse<ItemDto>>($"/api/items/{id}");
            if (response?.Success == true && response.Data != null)
            {
                return ItemViewModel.FromDto(response.Data);
            }
            return null;
        }
        catch (HttpRequestException)
        {
            throw;
        }
    }

    public async Task<List<ItemViewModel>> SearchItemsAsync(string query)
    {
        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var response = await _httpClient.GetFromJsonAsync<WebApiResponse<List<ItemDto>>>($"/api/items/search?query={encodedQuery}");
            if (response?.Success == true && response.Data != null)
            {
                return response.Data.Select(ItemViewModel.FromDto).ToList();
            }
            return new List<ItemViewModel>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
    }

    public async Task<ItemViewModel> CreateItemAsync(string name, string? description)
    {
        try
        {
            var request = new CreateItemRequestDto
            {
                Name = name,
                Description = description
            };

            var response = await _httpClient.PostAsJsonAsync("/api/items", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
            if (result?.Success == true && result.Data != null)
            {
                return ItemViewModel.FromDto(result.Data);
            }

            throw new InvalidOperationException("Failed to create item: Invalid response from server");
        }
        catch (HttpRequestException)
        {
            throw;
        }
    }

    public async Task<ItemViewModel> UpdateItemAsync(int id, string name, string? description)
    {
        try
        {
            var request = new UpdateItemRequestDto
            {
                Name = name,
                Description = description
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/items/{id}", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<WebApiResponse<ItemDto>>();
            if (result?.Success == true && result.Data != null)
            {
                return ItemViewModel.FromDto(result.Data);
            }

            throw new InvalidOperationException("Failed to update item: Invalid response from server");
        }
        catch (HttpRequestException)
        {
            throw;
        }
    }

    public async Task<bool> DeleteItemAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/items/{id}");
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException)
        {
            throw;
        }
    }
}

