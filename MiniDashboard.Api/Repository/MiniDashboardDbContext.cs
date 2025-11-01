using MiniDashboard.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniDashboard.Api.Repository;

public class MiniDashboardDbContext : DbContext
{
    public MiniDashboardDbContext(DbContextOptions<MiniDashboardDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}

