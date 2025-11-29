using HappyFactory.Events;
using HappyFactory.Projections.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyFactory.Projections;

/// <summary>
/// Background service that subscribes to the in-memory event store and projects events into the read model (EF InMemory).
/// </summary>
public class ProjectionService(
    EventStore eventStore,
    ILogger<ProjectionService> logger,
    IDbContextFactory<ReadModelDbContext> dbContextFactory)
    : IHostedService
{
    // Keep a reference to the delegate so we can unsubscribe.
    private Action<IEvent>? _onEvent;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ProjectionService starting and subscribing to EventStore.");

        _onEvent = ev =>
        {
            // Run projection asynchronously but do not await here (EventStore invokes subscribers synchronously).
            _ = HandleEventAsync(ev, cancellationToken);
        };

        eventStore.EventAppended += _onEvent;

        // Optionally: replay existing events on startup so projection can rebuild read model
        _ = ReplayExistingEventsAsync(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ProjectionService stopping and unsubscribing from EventStore.");
        
        if (_onEvent != null)
        {
            eventStore.EventAppended -= _onEvent;
            _onEvent = null;
        }

        return Task.CompletedTask;
    }

    private async Task ReplayExistingEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var all = eventStore.GetAll();
            logger.LogInformation("Replaying {Count} existing events into read model.", all.Count);
            foreach (var ev in all)
            {
                await HandleEventAsync(ev, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while replaying existing events.");
        }
    }

    private async Task HandleEventAsync(IEvent ev, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        try
        {
            switch (ev)
            {
                case ProductEvents.ProductCreated pc:
                    await HandleProductCreatedAsync(db, pc, cancellationToken);
                    break;

                case InventoryItemEvents.InventoryReserved ir:
                    await HandleInventoryReservedAsync(db, ir, cancellationToken);
                    break;

                default:
                    logger.LogDebug("ProjectionService received an unsupported event type: {Type}", ev.GetType().FullName);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log and swallow exceptions to avoid crashing the EventStore notification loop.
            logger.LogError(ex, "Error projecting event of type {Type}", ev.GetType().Name);
        }
    }

    private static async Task HandleProductCreatedAsync(ReadModelDbContext db, ProductEvents.ProductCreated ev, CancellationToken cancellationToken)
    {
        // If the product already exists in the read model, ignore.
        var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == ev.ProductId, cancellationToken);
        if (existing != null)
        {
            return;
        }

        var product = new Product(ev.ProductId, ev.Name, ev.Sku);
        db.Products.Add(product);

        // Ensure an InventoryItem exists for this product with quantity 0.
        var inventory = await db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == ev.ProductId, cancellationToken);
        if (inventory == null)
        {
            db.InventoryItems.Add(new InventoryItem(ev.ProductId, 0));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task HandleInventoryReservedAsync(ReadModelDbContext db, InventoryItemEvents.InventoryReserved ev, CancellationToken cancellationToken)
    {
        // Find inventory item; create if missing (with zero).
        var inventory = await db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == ev.ProductId, cancellationToken: cancellationToken);
        if (inventory == null)
        {
            inventory = new InventoryItem(ev.ProductId, 0);
            db.InventoryItems.Add(inventory);
        }

        // Reduce quantity but do not let it become negative in the read model.
        var newQuantity = Math.Max(0, inventory.EndingQuantity - ev.Quantity);

        // Replace tracked entity values with a new instance to avoid using reflection on private setters.
        var replacement = new InventoryItem(inventory.ProductId, newQuantity);
        db.Entry(inventory).CurrentValues.SetValues(replacement);

        await db.SaveChangesAsync(cancellationToken);
    }
}