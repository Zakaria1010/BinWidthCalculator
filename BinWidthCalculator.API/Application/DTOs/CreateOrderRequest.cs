using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Application.DTOs;

public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public ProductType ProductType { get; set; }
    public int Quantity { get; set; }
}