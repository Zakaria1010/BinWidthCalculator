using BinWidthCalculator.Infrastructure.Security;
using BinWidthCalculator.Application.Services;
using BinWidthCalculator.Domain.DTOs;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using FluentValidation;
using Moq;



namespace BinWidthCalculator.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
    private readonly Mock<IValidator<RegisterRequest>> _registerValidatorMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loginValidatorMock = new Mock<IValidator<LoginRequest>>();
        _registerValidatorMock = new Mock<IValidator<RegisterRequest>>();
        _configurationMock = new Mock<IConfiguration>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loginValidatorMock.Object,
            _registerValidatorMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest("testuser", "password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            Role = "User",
            IsActive = true
        };
        

        var validationResult = new FluentValidation.Results.ValidationResult();
        _loginValidatorMock.Setup(v => v.ValidateAsync(loginRequest, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword("password123", "hashed_password"))
            .Returns(true);

        _tokenServiceMock.Setup(t => t.GenerateToken(user))
            .Returns("jwt-token");

        _configurationMock.Setup(c => c["Jwt:ExpiresInHours"]).Returns("1");

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-token");
        result.Username.Should().Be("testuser");
        result.Role.Should().Be("User");
        result.Expires.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest("testuser", "wrongpassword");
        var user = new User
        {
            Username = "testuser",
            PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("correctpassword")),
            IsActive = true
        };

        var validationResult = new FluentValidation.Results.ValidationResult();
        _loginValidatorMock.Setup(v => v.ValidateAsync(loginRequest, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(loginRequest))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent", "password");

        var validationResult = new FluentValidation.Results.ValidationResult();
        _loginValidatorMock.Setup(v => v.ValidateAsync(loginRequest, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("nonexistent"))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(loginRequest))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password");
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginRequest = new LoginRequest("inactiveuser", "password");
        var user = new User
        {
            Username = "inactiveuser",
            IsActive = false
        };

        var validationResult = new FluentValidation.Results.ValidationResult();
        _loginValidatorMock.Setup(v => v.ValidateAsync(loginRequest, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("inactiveuser"))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword(loginRequest.Password, user.PasswordHash))
            .Returns(true);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(loginRequest))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is deactivated");
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var registerRequest = new RegisterRequest("newuser", "newuser@example.com", "Password123", "User");

        var validationResult = new FluentValidation.Results.ValidationResult();
        _registerValidatorMock.Setup(v => v.ValidateAsync(registerRequest, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("newuser"))
            .ReturnsAsync(false);

        _userRepositoryMock.Setup(r => r.EmailExistsAsync("newuser@example.com"))
            .ReturnsAsync(false);

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => 
            u.Username == "newuser" && 
            u.Email == "newuser@example.com" &&
            u.Role == "User" &&
            u.IsActive)), 
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_UsernameExists_ThrowsArgumentException()
    {
        // Arrange
        var registerRequest = new RegisterRequest("existinguser", "new@example.com", "Password123", "User");

        var validationResult = new FluentValidation.Results.ValidationResult();
        _registerValidatorMock.Setup(v => v.ValidateAsync(registerRequest, default))
            .ReturnsAsync(validationResult);

        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("existinguser"))
            .ReturnsAsync(true);

        // Act & Assert
        await _authService.Invoking(s => s.RegisterAsync(registerRequest))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Username already exists");
    }

    [Fact]
    public async Task UserExistsAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("existinguser"))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.UserExistsAsync("existinguser");

        // Assert
        result.Should().BeTrue();
    }
}