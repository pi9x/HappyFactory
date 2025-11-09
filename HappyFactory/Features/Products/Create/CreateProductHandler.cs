using HappyFactory.Models.Products;
using HappyFactory.Services;

namespace HappyFactory.Features.Products.Create;

/// <summary>
/// Command handler for creating products.
/// - Emits a <see cref="ProductEvents.ProductCreated"/> event into the event store.
/// </summary>
public class CreateProductHandler(EventStore eventStore)
{
    private readonly EventStore _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));

    /// <summary>
    /// Handles the create command. Returns the id of the created product.
    /// </summary>
    public async Task<Guid> HandleAsync(CreateProductRequest req, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(req);
        if (string.IsNullOrWhiteSpace(req.Name)) throw new ArgumentException("Name must be provided.", nameof(req));
        if (string.IsNullOrWhiteSpace(req.Sku)) throw new ArgumentException("SKU must be provided.", nameof(req));

        var id = Guid.CreateVersion7();

        // Create domain aggregate
        var product = new Product(id, req.Name, req.Sku);

        // Emit domain event to the in-memory event store so projections can pick it up.
        var ev = new ProductEvents.ProductCreated(product.Id, product.Name, product.Sku);
        _eventStore.Append(ev);

        return product.Id;
    }
}