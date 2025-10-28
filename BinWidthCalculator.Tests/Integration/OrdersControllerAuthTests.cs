using System;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using System.Net.Http.Headers;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Tests.Integration.Infrastructure;

namespace BinWidthCalculator.Tests.Integration;

public class OrdersControllerAuthTests : TestBase
{
    public OrdersControllerAuthTests(): base("OrdersTestDb") { }

    [Fact]
    public async Task CreateOrder_WithoutAuthentication_ReturnsUnauthorized()
    {
        var items  =  new List<OrderItemRequest>()  { new (ProductType.PhotoBook, 1) };
        var orderRequest = new CreateOrderRequest(items);

        var content = new StringContent(
            JsonSerializer.Serialize(orderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/orders", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task CreateOrder_WithValidToken_ReturnsCreated()
    {
        var items  =  new List<OrderItemRequest>()  { new (ProductType.PhotoBook, 1) };
        var orderRequest = new CreateOrderRequest(items);

        var content = new StringContent(
            JsonSerializer.Serialize(orderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(responseContent, _jsonOptions);
        
        orderResponse.Should().NotBeNull();
        orderResponse!.OrderId.Should().NotBeEmpty();
        orderResponse.RequiredBinWidth.Should().Be(19.0m);
    }

    [Fact]
    public async Task GetOrder_WithValidToken_ReturnsOrder()
    {
        var items  =  new List<OrderItemRequest>()  { new (ProductType.Calendar, 1) };
        var orderRequest = new CreateOrderRequest(items);

        var createContent = new StringContent(
            JsonSerializer.Serialize(orderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        var createResponse = await _client.PostAsync("/api/orders", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<OrderResponse>(createResponseContent, _jsonOptions);

        // Act - Get the order
        var getResponse = await _client.GetAsync($"/api/orders/{createdOrder!.OrderId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(getResponseContent, _jsonOptions);
        
        orderResponse.Should().NotBeNull();
        orderResponse!.OrderId.Should().Be(createdOrder.OrderId);
    }

    [Fact]
    public async Task GetOrder_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act - Don't set authorization header
        var response = await _client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllOrders_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        // Act - Regular user trying to access admin endpoint
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
    {
        var validItems  =  new List<OrderItemRequest>()  
        { 
            new (ProductType.PhotoBook, 1),  // 19mm
            new (ProductType.Calendar, 2),   // 20mm (2 * 10mm)
            new (ProductType.Canvas, 1),     // 16mm
            new (ProductType.Cards, 3),      // 14.1mm (3 * 4.7mm)
            new (ProductType.Mug, 5)        // 188mm (2 stacks: 2 * 94mm)
            // Total: 19 + 20 + 16 + 14.1 + 188 = 257.1mm
        };

        // Arrange - Create a valid order request with multiple products
        var validOrderRequest = new CreateOrderRequest(validItems);

        var content = new StringContent(
            JsonSerializer.Serialize(validOrderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(responseContent, _jsonOptions);
        
        orderResponse.Should().NotBeNull();
        orderResponse!.OrderId.Should().NotBeEmpty();
        orderResponse.Items.Should().HaveCount(5);
        
        // Verify all items are correctly returned
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.PhotoBook && i.Quantity == 1);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.Calendar && i.Quantity == 2);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.Canvas && i.Quantity == 1);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.Cards && i.Quantity == 3);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.Mug && i.Quantity == 5);
        
        // Verify the bin width calculation is correct
        // 1 PhotoBook: 19mm
        // 2 Calendars: 20mm (2 * 10mm)
        // 1 Canvas: 16mm
        // 3 Cards: 14.1mm (3 * 4.7mm)
        // 5 Mugs: 188mm (2 stacks: ceil(5/4) = 2 stacks * 94mm = 188mm)
        // Total: 19 + 20 + 16 + 14.1 + 188 = 257.1mm
        orderResponse.RequiredBinWidth.Should().Be(257.1m);
        
        // Verify the response headers contain location
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
                .ToLower()
                .Should().Contain($"/api/orders/{orderResponse.OrderId}".ToLower());
    }

    [Fact]
    public async Task CreateOrder_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange - Create an invalid order request
        var invalidItems  =  new List<OrderItemRequest>()  
        { 
            new (ProductType.PhotoBook, 0), // Invalid quantity
            new ((ProductType)99, 1) // Invalid product type
        };

        var invalidOrderRequest = new CreateOrderRequest(invalidItems);

        var content = new StringContent(
            JsonSerializer.Serialize(invalidOrderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("errors");
        
        // Verify specific validation errors
        responseContent.Should().Contain("Quantity must be greater than 0");
        responseContent.Should().Contain("Invalid product type");
    }

    [Fact]
    public async Task GetOrder_ExistingOrder_ReturnsOrder()
    {
        // Arrange - First create an order
        var items = new List<OrderItemRequest>
        {
            new(ProductType.PhotoBook, 2 ),
            new(ProductType.Mug, 3)
        };
        var orderRequest = new CreateOrderRequest(items);

        var createContent = new StringContent(
            JsonSerializer.Serialize(orderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        var createResponse = await _client.PostAsync("/api/orders", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<OrderResponse>(createResponseContent, _jsonOptions);

        var getResponse = await _client.GetAsync($"/api/orders/{createdOrder!.OrderId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(getResponseContent, _jsonOptions);
        
        orderResponse.Should().NotBeNull();
        orderResponse!.OrderId.Should().Be(createdOrder.OrderId);
        orderResponse.Items.Should().HaveCount(2);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.PhotoBook && i.Quantity == 2);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.Mug && i.Quantity == 3);
        
        orderResponse.RequiredBinWidth.Should().Be(132.0m);
    }

    [Fact]
    public async Task GetOrder_NonExistentOrder_ReturnsNotFound()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        // Act - Try to get an order that doesn't exist
        var response = await _client.GetAsync($"/api/orders/{nonExistentOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Order with ID {nonExistentOrderId} not found");
    }

    [Fact]
    public async Task GetAllOrders_AsAdminUser_ReturnsOrders()
    {
        await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        // Test admin-only endpoint
        var response = await _client.GetAsync("/api/orders");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}