namespace BinWidthCalculator.Domain.DTOs;

public record LoginResponse(string Token, DateTime Expires, string Role, string Username);