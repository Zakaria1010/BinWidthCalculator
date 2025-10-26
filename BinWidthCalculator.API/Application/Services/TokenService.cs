using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace BinWidthCalculator.Application.Services;

public class TokenService : ITokenService
{
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly double _jwtExpiresInHours;

    public TokenService(IConfiguration configuration)
    {
        // Support environment variables (e.g., Jwt__Key) and appsettings.json
        _jwtKey = configuration["Jwt__Key"] ?? configuration["Jwt:Key"];
        _jwtIssuer = configuration["Jwt__Issuer"] ?? configuration["Jwt:Issuer"];
        _jwtAudience = configuration["Jwt__Audience"] ?? configuration["Jwt:Audience"];
        _jwtExpiresInHours = double.TryParse(configuration["Jwt__ExpiresInHours"] ?? configuration["Jwt:ExpiresInHours"], out var hours) 
            ? hours 
            : 12; // fallback to 12 hours

        if (string.IsNullOrEmpty(_jwtKey))
            throw new Exception("JWT key is missing. Set Jwt__Key environment variable or appsettings value.");
    }

    public string GenerateToken(User user)
    {
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtExpiresInHours),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}