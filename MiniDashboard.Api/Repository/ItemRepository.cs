using MiniDashboard.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniDashboard.Api.Repository;

public class ItemRepository : IItemRepository
{
    private readonly MiniDashboardDbContext _context;

    public ItemRepository(MiniDashboardDbContext context)
    {
        _context = context;
    }

    public async Task<List<Item>> GetAllAsync()
    {
        return await _context.Items
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _context.Items.FindAsync(id);
    }

    public async Task<List<Item>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();

        var lowerQuery = query.ToLowerInvariant();
        return await _context.Items
            .Where(i => i.Name.ToLower().Contains(lowerQuery) ||
                       (i.Description != null && i.Description.ToLower().Contains(lowerQuery)))
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<Item> AddAsync(Item item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(Item item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

