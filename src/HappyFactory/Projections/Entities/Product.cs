namespace HappyFactory.Projections.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;

    // Parameterless ctor for serializers/EF etc.
    protected Product() { }

    public Product(Guid id, string name, string sku)
    {
        Id = id;
        Name = name;
        Sku = sku;
    }
}