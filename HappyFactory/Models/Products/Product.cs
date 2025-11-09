namespace HappyFactory.Models.Products;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Sku { get; private set; } = null!;

    // Parameterless ctor for serializers/EF etc.
    protected Product() { }

    public Product(Guid id, string name, string sku)
    {
        Id = id != Guid.Empty ? id : throw new ArgumentException("Id must not be empty.", nameof(id));
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name must be provided.", nameof(name));
        Sku = !string.IsNullOrWhiteSpace(sku) ? sku : throw new ArgumentException("SKU must be provided.", nameof(sku));
    }

    public static Product Create(Guid id, string name, string sku) => new Product(id, name, sku);

    public override string ToString() => $"Product {{ Id = {Id}, Name = {Name}, SKU = {Sku} }}";
}