using BinWidthCalculator.Application.Services;
using Microsoft.Extensions.Configuration;
using BinWidthCalculator.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;

namespace BinWidthCalculator.Tests.Unit.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        var configValues = new Dictionary<string, string>
        {
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:SecretKey", "super-secret-key-that-is-32-characters!" },
            {"Jwt:ExpiresInHours", "1"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _tokenService = new TokenService(_configuration);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Role = "User"
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
        
        var claims = jwtToken.Claims.ToList();
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.Username);
        claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        claims.Should().Contain(c => (c.Type == ClaimTypes.Role ||
              c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
              c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
             && c.Value == user.Role);
    }

    [Fact]
    public void GenerateToken_UserWithAdminRole_IncludesAdminRoleInClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "adminuser",
            Email = "admin@example.com",
            Role = "Admin"
        };

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role ||
            c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
            c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase));

        roleClaim.Should().NotBeNull("the JWT should contain a role claim");
        roleClaim.Value.Should().Be("Admin");
    }
}