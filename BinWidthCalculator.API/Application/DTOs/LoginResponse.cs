namespace BinWidthCalculator.Application.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}