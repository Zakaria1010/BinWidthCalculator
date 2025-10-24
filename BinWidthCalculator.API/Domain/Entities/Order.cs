namespace BinWidthCalculator.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal RequiredBinWidth { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order() { }

    public Order(Guid id, List<OrderItem> items, decimal requiredBinWidth)
    {
        Id = id;
        Items = items;
        RequiredBinWidth = requiredBinWidth;
        CreatedAt = DateTime.UtcNow;
    }
}