using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Application.DTOs;

public class OrderResponse
{
    public Guid OrderId { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public decimal RequiredBinWidth { get; set; }
}

public class OrderItemResponse
{
    public ProductType ProductType { get; set; }
    public int Quantity { get; set; }
}