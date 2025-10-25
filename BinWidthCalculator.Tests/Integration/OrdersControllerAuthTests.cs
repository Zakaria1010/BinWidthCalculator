using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace BinWidthCalculator.Tests.Integration;

public class OrdersControllerAuthTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private string _authToken = string.Empty;

    public OrdersControllerAuthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("OrdersAuthTestDatabase");
                });
            });
        });
        
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Seed the database with a test user and login to get token
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Clear any existing data
        context.Users.RemoveRange(context.Users);
        context.Orders.RemoveRange(context.Orders);
        await context.SaveChangesAsync();

        // Add test user
        var testUser = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("TestPassword123")),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        context.Users.Add(testUser);
        await context.SaveChangesAsync();

        // Login to get token
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "TestPassword123"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
        
        _authToken = loginResponse?.Token ?? string.Empty;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateOrder_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = Domain.Entities.ProductType.PhotoBook, Quantity = 1 }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(orderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act - Don't set authorization header
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithValidToken_ReturnsCreated()
    {
        // Arrange
        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = Domain.Entities.ProductType.PhotoBook, Quantity = 1 }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(orderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

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
        // Arrange - First create an order
        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = Domain.Entities.ProductType.Calendar, Quantity = 1 }
            }
        };

        var createContent = new StringContent(
            JsonSerializer.Serialize(orderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

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
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

        // Act - Regular user trying to access admin endpoint
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}