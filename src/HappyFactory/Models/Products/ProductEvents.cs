namespace HappyFactory.Models.Products;

public class ProductEvents
{
    public sealed record ProductCreated(Guid ProductId, string Name, string Sku, DateTime Timestamp) : IEvent;
}