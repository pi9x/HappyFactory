using HappyFactory.Models.Products;
using HappyFactory.Services;
using Microsoft.EntityFrameworkCore;

namespace HappyFactory.Features.Products.Get;

/// <summary>
/// Query handler that reads from the read-model (EF InMemory).
/// </summary>
public class GetProductHandler(ReadModelDbContext db)
{
    private readonly ReadModelDbContext _db = db ?? throw new ArgumentNullException(nameof(db));

    /// <summary>
    /// Returns the product if found; otherwise null.
    /// </summary>
    public async Task<GetProductResponse?> HandleAsync(GetProductRequest req, CancellationToken ct = default)
    {
        if (req.Id == Guid.Empty) throw new ArgumentException("Id must not be empty.", nameof(req.Id));

        var product = await _db.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.Id, ct);

        return product == null ? null : new GetProductResponse(product.Id, product.Name, product.Sku);
    }
}