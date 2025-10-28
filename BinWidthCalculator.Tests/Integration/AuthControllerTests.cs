using System.Net;
using System.Text;
using FluentAssertions;
using System.Text.Json;
using BinWidthCalculator.Domain.DTOs;
using BinWidthCalculator.Tests.Integration.Infrastructure;

namespace BinWidthCalculator.Tests.Integration;

public class AuthControllerTests : TestBase
{
    public AuthControllerTests() : base("AuthTestDb") { }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
        
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.Username.Should().Be("testuser");
        loginResponse.Role.Should().Be("User");
        loginResponse.Expires.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "nonexistent",
            Password = "password"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsSuccess()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "NewPassword123",
            Role = "User"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("User registered successfully");
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsBadRequest()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "testuser", 
            Email = "new@example.com",
            Password = "NewPassword123"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Username already exists");
    }

    [Fact]
    public async Task Register_InvalidRequest_ReturnsBadRequest()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "ab",
            Email = "invalid-email",
            Password = "123"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(registerRequest, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("errors");
    }
}