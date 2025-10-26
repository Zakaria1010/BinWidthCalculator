using BinWidthCalculator.Domain.DTOs;
using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Domain.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<bool> UserExistsAsync(string username);
}

public interface ITokenService
{
    string GenerateToken(User user);
}