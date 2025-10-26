using BinWidthCalculator.Application.Validators;
using BinWidthCalculator.Domain.DTOs;
using FluentAssertions;

namespace BinWidthCalculator.Tests.Unit.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "valid@example.com",
            Password = "ValidPassword123",
            Role = "User"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("thisusernameistoolongbecauseitexceedsfiftycharacterslimit")]
    [InlineData(null)]
    public void Validate_InvalidUsername_ReturnsInvalid(string username)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = username,
            Email = "valid@example.com",
            Password = "ValidPassword123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("missing@")]
    [InlineData("@domain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData(null)]
    public void Validate_InvalidEmail_ReturnsInvalid(string email)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = email,
            Password = "ValidPassword123"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("nouppercase123")]
    [InlineData("NOLOWERCASE123")]
    [InlineData("NoNumbersHere")]
    [InlineData(null)]
    public void Validate_InvalidPassword_ReturnsInvalid(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "valid@example.com",
            Password = password
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_InvalidRole_ReturnsInvalid(string role)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "valid@example.com",
            Password = "ValidPassword123",
            Role = role
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }
}