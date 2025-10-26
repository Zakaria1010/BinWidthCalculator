using Microsoft.AspNetCore.Authorization;
using BinWidthCalculator.Application.Interfaces;
using BinWidthCalculator.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BinWidthCalculator.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("User {Username} creating order", username);
            
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { orderId = order.OrderId }, order);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for order creation: {Errors}", ex.Errors);
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { error = "An error occurred while creating the order" });
        }
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid orderId)
    {
        try
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("User {Username} retrieving order {OrderId}", username, orderId);
            
            var order = await _orderService.GetOrderAsync(orderId);
            
            if (order == null)
            {
                return NotFound(new { error = $"Order with ID {orderId} not found" });
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
            return StatusCode(500, new { error = "An error occurred while retrieving the order" });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<OrderResponse>>> GetAllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, new { error = "An error occurred while retrieving orders" });
        }
    }
}