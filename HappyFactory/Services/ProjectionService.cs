using HappyFactory.Models;
using HappyFactory.Models.InventoryItems;
using HappyFactory.Models.Products;

namespace HappyFactory.Services;

/// <summary>
/// Background service that subscribes to the in-memory event store and projects events into the read model (EF InMemory).
/// </summary>
public class ProjectionService(
    EventStore eventStore,
    IServiceScopeFactory scopeFactory,
    ILogger<ProjectionService> logger)
    : IHostedService
{
    private readonly EventStore _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly ILogger<ProjectionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Keep a reference to the delegate so we can unsubscribe.
    private Action<IEvent>? _onEvent;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ProjectionService starting and subscribing to EventStore.");

        _onEvent = ev =>
        {
            // Run projection asynchronously but do not await here (EventStore invokes subscribers synchronously).
            _ = HandleEventAsync(ev);
        };

        _eventStore.EventAppended += _onEvent;

        // Optionally: replay existing events on startup so projection can rebuild read model
        _ = ReplayExistingEventsAsync();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ProjectionService stopping and unsubscribing from EventStore.");
        
        if (_onEvent != null)
        {
            _eventStore.EventAppended -= _onEvent;
            _onEvent = null;
        }

        return Task.CompletedTask;
    }

    private async Task ReplayExistingEventsAsync()
    {
        try
        {
            var all = _eventStore.GetAll();
            _logger.LogInformation("Replaying {Count} existing events into read model.", all.Count);
            foreach (var ev in all)
            {
                await HandleEventAsync(ev);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while replaying existing events.");
        }
    }

    private async Task HandleEventAsync(IEvent ev)
    {
        try
        {
            // Create a scope for each event processing so we have a fresh DbContext.
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReadModelDbContext>();

            switch (ev)
            {
                case ProductEvents.ProductCreated pc:
                    await HandleProductCreatedAsync(db, pc);
                    break;

                case InventoryItemEvents.InventoryReserved ir:
                    await HandleInventoryReservedAsync(db, ir);
                    break;

                default:
                    _logger.LogDebug("ProjectionService received an unsupported event type: {Type}", ev.GetType().FullName);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log and swallow exceptions to avoid crashing the EventStore notification loop.
            _logger.LogError(ex, "Error projecting event of type {Type}", ev.GetType().Name);
        }
    }

    private static Task HandleProductCreatedAsync(ReadModelDbContext db, ProductEvents.ProductCreated ev)
    {
        // If the product already exists in the read model, ignore.
        var existing = db.Products.Find(ev.ProductId);
        if (existing != null)
        {
            return Task.CompletedTask;
        }

        var product = new Product(ev.ProductId, ev.Name, ev.Sku);
        db.Products.Add(product);

        // Ensure an InventoryItem exists for this product with quantity 0.
        var inventory = db.InventoryItems.Find(ev.ProductId);
        if (inventory == null)
        {
            db.InventoryItems.Add(new InventoryItem(ev.ProductId, 0));
        }

        return db.SaveChangesAsync();
    }

    private static async Task HandleInventoryReservedAsync(ReadModelDbContext db, InventoryItemEvents.InventoryReserved ev)
    {
        // Find inventory item; create if missing (with zero).
        var inventory = await db.InventoryItems.FindAsync(ev.ProductId);
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

        await db.SaveChangesAsync();
    }
}