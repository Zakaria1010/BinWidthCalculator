namespace BinWidthCalculator.Domain.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Email, string Password, string Role = "User");