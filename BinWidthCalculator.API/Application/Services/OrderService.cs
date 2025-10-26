using FluentValidation;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Application.Interfaces;

namespace BinWidthCalculator.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBinWidthCalculator _binWidthCalculator;
    private readonly IValidator<CreateOrderRequest> _validator;

    public OrderService(
        IOrderRepository orderRepository,
        IBinWidthCalculator binWidthCalculator,
        IValidator<CreateOrderRequest> validator)
    {
        _orderRepository = orderRepository;
        _binWidthCalculator = binWidthCalculator;
        _validator = validator;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var orderItems = request.Items.Select(item => 
            new OrderItem(item.ProductType, item.Quantity)).ToList();

        var requiredBinWidth = _binWidthCalculator.CalculateRequiredBinWidth(orderItems);

        var order = new Order(Guid.NewGuid(), orderItems, requiredBinWidth);

        var savedOrder = await _orderRepository.AddAsync(order);

        return MapToOrderResponse(savedOrder);
    }

    public async Task<OrderResponse?> GetOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order != null ? MapToOrderResponse(order) : null;
    }

    public async Task<List<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllOrdersAsync();
        return orders.Select(MapToOrderResponse).ToList();
    }

    private static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            OrderId = order.Id,
            Items = order.Items.Select(item => new OrderItemResponse
            {
                ProductType = item.ProductType,
                Quantity = item.Quantity
            }).ToList(),
            RequiredBinWidth = order.RequiredBinWidth
        };
    }
}