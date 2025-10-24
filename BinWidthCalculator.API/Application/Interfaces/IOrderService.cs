using BinWidthCalculator.Application.DTOs;

namespace BinWidthCalculator.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> GetOrderAsync(Guid orderId);
}