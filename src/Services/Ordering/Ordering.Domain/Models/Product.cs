
namespace Ordering.Domain.Models;

public class Product : Entity<ProductId>
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }

    public static Product Create(ProductId id, string name, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        return new Product
        {
            Id = id,
            Name = name,
            Price = price
        };
    }
}
