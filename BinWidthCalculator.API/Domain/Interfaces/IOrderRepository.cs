using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid orderId);
    Task<Order> AddAsync(Order order);
    Task<bool> ExistsAsync(Guid orderId);
    Task<List<Order>> GetAllOrdersAsync();
}