namespace BinWidthCalculator.Domain.Entities;

public class OrderItem
{
    public ProductType ProductType { get; set; }
    public int Quantity { get; set; }

    public OrderItem() { }

    public OrderItem(ProductType productType, int quantity)
    {
        ProductType = productType;
        Quantity = quantity;
    }
}