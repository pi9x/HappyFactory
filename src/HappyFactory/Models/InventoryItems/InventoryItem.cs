namespace HappyFactory.Models.InventoryItems;

public class InventoryItem
{
    public Guid ProductId { get; private set; }
    public int EndingQuantity { get; private set; }
    
    // Parameterless ctor for serializers/EF etc.
    protected InventoryItem() { }

    public InventoryItem(Guid productId, int initialQuantity = 0)
    {
        ProductId = productId != Guid.Empty ? productId : throw new ArgumentException("ProductId must not be empty.", nameof(productId));
        if (initialQuantity < 0) throw new ArgumentOutOfRangeException(nameof(initialQuantity));
        EndingQuantity = initialQuantity;
    }
}