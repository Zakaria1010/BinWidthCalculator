using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Application.Interfaces;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.Interfaces;
using FluentValidation;

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
        // Validate request
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Convert to domain entities
        var orderItems = request.Items.Select(item => 
            new OrderItem(item.ProductType, item.Quantity)).ToList();

        // Calculate required bin width
        var requiredBinWidth = _binWidthCalculator.CalculateRequiredBinWidth(orderItems);

        // Create order
        var order = new Order(Guid.NewGuid(), orderItems, requiredBinWidth);

        // Save order
        var savedOrder = await _orderRepository.AddAsync(order);

        // Convert to response
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