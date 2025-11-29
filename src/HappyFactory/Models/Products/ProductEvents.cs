namespace HappyFactory.Models.Products;

public class ProductEvents
{
    public sealed class ProductCreated : IEvent
    {
        public Guid ProductId { get; }
        public string Name { get; }
        public string Sku { get; }
        public DateTime Timestamp { get; }

        public ProductCreated(Guid productId, string name, string sku, DateTime? timestamp = null)
        {
            if (productId == Guid.Empty) throw new ArgumentException("productId must not be empty", nameof(productId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name must be provided", nameof(name));
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("sku must be provided", nameof(sku));

            ProductId = productId;
            Name = name;
            Sku = sku;
            Timestamp = timestamp ?? DateTime.UtcNow;
        }

        public override string ToString()
            => $"ProductCreated {{ ProductId = {ProductId}, Name = {Name}, SKU = {Sku}, Timestamp = {Timestamp:o} }}";
    }
}