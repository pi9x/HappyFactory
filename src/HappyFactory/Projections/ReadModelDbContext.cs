using HappyFactory.Projections.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyFactory.Projections;

/// <summary>
/// EF Core read-model DbContext using InMemory provider.
/// Contains product and inventory read-model sets.
/// </summary>
public class ReadModelDbContext(DbContextOptions<ReadModelDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; private set; } = null!;
    public DbSet<InventoryItem> InventoryItems { get; private set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(255);
            b.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        });

        // InventoryItem
        modelBuilder.Entity<InventoryItem>(b =>
        {
            b.HasKey(i => i.ProductId);
            b.Property(i => i.EndingQuantity).IsRequired();
        });
    }
}