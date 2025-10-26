using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using BinWidthCalculator.Infrastructure.Security;

namespace BinWidthCalculator.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Validate request
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Find user
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

        // Generate token
        var token = _tokenService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(
                Convert.ToDouble(_configuration["Jwt:ExpiresInHours"] ?? "12")),
            Role = user.Role,
            Username = user.Username
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        // Validate request
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Check if user exists
        if (await _userRepository.UsernameExistsAsync(request.Username))
            throw new ArgumentException("Username already exists");

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new ArgumentException("Email already exists");

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        return true;
    }

    public async Task<bool> UserExistsAsync(string username)
        => await _userRepository.UsernameExistsAsync(username);
}

