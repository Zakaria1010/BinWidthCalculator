using System;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BinWidthCalculator.Domain.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Infrastructure.Data;
using BinWidthCalculator.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using BinWidthCalculator.Application.Interfaces;
using BinWidthCalculator.Infrastructure.Security;
using BinWidthCalculator.Infrastructure.Repositories;

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
        Environment.SetEnvironmentVariable("Jwt__Key", "super-secret-key-that-is-32-characters!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("Jwt__ExpiresInHours", "1");
        
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


                if (!services.Any(s => s.ServiceType == typeof(IPasswordHasher)))
                {
                    services.AddScoped<IPasswordHasher, PasswordHasher>();
                }

                if (!services.Any(s => s.ServiceType == typeof(IUserRepository)))
                {
                    services.AddScoped<IUserRepository, UserRepository>();
                }

                if (!services.Any(s => s.ServiceType == typeof(IAuthService)))
                {
                    services.AddScoped<IAuthService, AuthService>();
                }
            });
        });
        
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Seed the database with a test user and login to get token
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        
        // Clear any existing data
        context.Users.RemoveRange(context.Users);
        context.Orders.RemoveRange(context.Orders);
        await context.SaveChangesAsync();
        // Add test user
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash =  passwordHasher.HashPassword("TestPassword123"),
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
                new() { ProductType = ProductType.PhotoBook, Quantity = 1 }
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
                new() { ProductType = ProductType.PhotoBook, Quantity = 1 }
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
                new() { ProductType = ProductType.Calendar, Quantity = 1 }
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

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder()
    {
        // Arrange - Create a valid order request with multiple products
        var validOrderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = ProductType.PhotoBook, Quantity = 1 },      // 19mm
                new() { ProductType = ProductType.Calendar, Quantity = 2 },       // 20mm (2 * 10mm)
                new() { ProductType = ProductType.Canvas, Quantity = 1 },         // 16mm
                new() { ProductType = ProductType.Cards, Quantity = 3 },          // 14.1mm (3 * 4.7mm)
                new() { ProductType = ProductType.Mug, Quantity = 5 }             // 188mm (2 stacks: 2 * 94mm)
                // Total: 19 + 20 + 16 + 14.1 + 188 = 257.1mm
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(validOrderRequest, _jsonOptions),
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
        var invalidOrderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = ProductType.PhotoBook, Quantity = 0 }, // Invalid quantity
                new() { ProductType = (ProductType)99, Quantity = 1 } // Invalid product type
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(invalidOrderRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

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
        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductType = ProductType.PhotoBook, Quantity = 2 },
                new() { ProductType = ProductType.Mug, Quantity = 3 }
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

        // Act - Get the order that was just created
        var getResponse = await _client.GetAsync($"/api/orders/{createdOrder!.OrderId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var orderResponse = JsonSerializer.Deserialize<OrderResponse>(getResponseContent, _jsonOptions);
        
        orderResponse.Should().NotBeNull();
        orderResponse!.OrderId.Should().Be(createdOrder.OrderId);
        orderResponse.Items.Should().HaveCount(2);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.PhotoBook && i.Quantity == 2);
        orderResponse.Items.Should().Contain(i => i.ProductType == ProductType.Mug && i.Quantity == 3);
        
        // Verify the bin width calculation is correct
        // 2 PhotoBooks (2 * 19mm = 38mm) + 3 Mugs (1 stack = 94mm) = 132mm
        orderResponse.RequiredBinWidth.Should().Be(132.0m);
    }

    [Fact]
    public async Task GetOrder_NonExistentOrder_ReturnsNotFound()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();
        
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

        // Act - Try to get an order that doesn't exist
        var response = await _client.GetAsync($"/api/orders/{nonExistentOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain($"Order with ID {nonExistentOrderId} not found");
    }
}