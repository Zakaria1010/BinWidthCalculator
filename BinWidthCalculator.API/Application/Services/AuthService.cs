using FluentValidation;
using BinWidthCalculator.Domain.DTOs;
using BinWidthCalculator.Domain.DTOs;
using BinWidthCalculator.Domain.Entities;
using Microsoft.Extensions.Configuration;
using BinWidthCalculator.Domain.Interfaces;

namespace BinWidthCalculator.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

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
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _userRepository.UsernameExistsAsync(request.Username))
            throw new ArgumentException("Username already exists");

        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new ArgumentException("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        return true;
    }

    public async Task<bool> UserExistsAsync(string username) => await _userRepository.UsernameExistsAsync(username);
}

