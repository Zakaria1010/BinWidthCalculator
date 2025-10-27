using System.Text;
using System.Text.Json;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.DTOs;
using Microsoft.Extensions.DependencyInjection;
using BinWidthCalculator.Infrastructure.Data;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Tests.Fixtures;

namespace BinWidthCalculator.Tests.Integration.Infrastructure;

public abstract class TestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;
    protected string _authToken = string.Empty;

    protected TestBase(string databaseName)
    {
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "super-secret-key-that-is-32-characters!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("Jwt__ExpiresInHours", "1");

        _factory = new CustomWebApplicationFactory{ DbName = databaseName };

        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
        await SeedTestUserAsync();
        await LoginTestUserAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.RemoveRange(context.Users);
        context.Orders.RemoveRange(context.Orders);
        await context.SaveChangesAsync();
    }

    private async Task SeedTestUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = passwordHasher.HashPassword("TestPassword123"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Users.Add(testUser);
        await context.SaveChangesAsync();
    }

    private async Task LoginTestUserAsync()
    {
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
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
        _authToken = loginResponse?.Token ?? string.Empty;
    }
}