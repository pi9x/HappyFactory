namespace HappyFactory.Events;

public class InventoryItemEvents
{
    public sealed record InventoryReserved(Guid ProductId, int Quantity, DateTime Timestamp) : IEvent;
}