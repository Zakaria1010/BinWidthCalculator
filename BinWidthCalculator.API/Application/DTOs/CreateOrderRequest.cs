using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Application.DTOs;

public record CreateOrderRequest(List<OrderItemRequest> Items);

public record OrderItemRequest(ProductType ProductType, int Quantity);
