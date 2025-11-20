using HappyFactory.Models.InventoryItems;
using HappyFactory.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace HappyFactory.Services;

/// <summary>
/// EF Core read-model DbContext using InMemory provider.
/// Contains product and inventory read-model sets.
/// </summary>
public class ReadModelDbContext(DbContextOptions<ReadModelDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired();
            b.Property(p => p.Sku).IsRequired();
        });

        // InventoryItem
        modelBuilder.Entity<InventoryItem>(b =>
        {
            b.HasKey(i => i.ProductId);
            b.Property(i => i.EndingQuantity).IsRequired();
        });
    }
}