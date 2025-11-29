using HappyFactory.Models.Products;
using HappyFactory.Services;
using Microsoft.EntityFrameworkCore;

namespace HappyFactory.Features.Products.Get;

/// <summary>
/// Query handler that reads from the read-model (EF InMemory).
/// </summary>
public class GetProductHandler(IDbContextFactory<ReadModelDbContext> dbContextFactory)
{
    /// <summary>
    /// Returns the product if found; otherwise null.
    /// </summary>
    public async Task<GetProductResponse?> HandleAsync(GetProductRequest req, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        
        var product = await db.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.Id, ct);

        return product == null ? null : new GetProductResponse(product.Id, product.Name, product.Sku);
    }
}