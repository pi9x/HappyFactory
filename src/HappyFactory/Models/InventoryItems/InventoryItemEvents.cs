namespace HappyFactory.Models.InventoryItems;

public class InventoryItemEvents
{
    public sealed class InventoryReserved : IEvent
    {
        /// <summary>
        /// The id of the product for which inventory was reserved.
        /// </summary>
        public Guid ProductId { get; }
        
        /// <summary>
        /// The quantity reserved.
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// UTC timestamp when the reservation happened.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Creates a new InventoryReserved event.
        /// </summary>
        /// <param name="productId">Product identifier (must not be empty).</param>
        /// <param name="quantity">Quantity reserved (must be > 0).</param>
        /// <param name="timestamp">Optional timestamp; if null, DateTime.UtcNow will be used.</param>
        public InventoryReserved(Guid productId, int quantity, DateTime? timestamp = null)
        {
            if (productId == Guid.Empty) throw new ArgumentException("productId must not be empty.", nameof(productId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "quantity must be positive.");

            ProductId = productId;
            Quantity = quantity;
            Timestamp = timestamp ?? DateTime.UtcNow;
        }

        public override string ToString()
            => $"InventoryReserved {{ ProductId = {ProductId}, Quantity = {Quantity}, Timestamp = {Timestamp:o} }}";
    }
}