using System.Net;
using System.Text;
using FluentAssertions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BinWidthCalculator.Tests.Integration;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace database with in-memory for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = ProductType.PhotoBook, Quantity = 1 },
                new() { ProductType = ProductType.Mug, Quantity = 2 }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(responseContent, _jsonOptions);
        
        orderResponse.Should().NotBeNull();
        orderResponse!.OrderId.Should().NotBeEmpty();
        orderResponse.Items.Should().HaveCount(2);
        orderResponse.RequiredBinWidth.Should().Be(113.0m); // 19 + 94 (1 stack of 2 mugs)
    }

    [Fact]
    public async Task CreateOrder_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = ProductType.PhotoBook, Quantity = 0 } // Invalid quantity
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrder_ExistingOrder_ReturnsOrder()
    {
        // Arrange - First create an order
        var createRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = ProductType.Calendar, Quantity = 1 }
            }
        };

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var createResponse = await _client.PostAsync("/api/orders", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<OrderResponse>(createResponseContent, _jsonOptions);

        // Act
        var getResponse = await _client.GetAsync($"/api/orders/{createdOrder!.OrderId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(getResponseContent, _jsonOptions);
        
        orderResponse.Should().NotBeNull();
        orderResponse!.OrderId.Should().Be(createdOrder.OrderId);
        orderResponse.Items.Should().HaveCount(1);
        orderResponse.RequiredBinWidth.Should().Be(10.0m);
    }

    [Fact]
    public async Task GetOrder_NonExistentOrder_ReturnsNotFound()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{nonExistentOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}