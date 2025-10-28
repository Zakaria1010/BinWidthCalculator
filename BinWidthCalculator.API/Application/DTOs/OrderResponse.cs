using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Application.DTOs;

public record OrderResponse(Guid OrderId, List<OrderItemResponse> Items, decimal RequiredBinWidth);

public record OrderItemResponse(ProductType ProductType, int Quantity);