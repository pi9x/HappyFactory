namespace HappyFactory.Models.InventoryItems;

public class InventoryItemEvents
{
    public sealed record InventoryReserved(Guid ProductId, int Quantity, DateTime Timestamp) : IEvent;
}