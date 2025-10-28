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
        var request = new RegisterRequest("validuser", "valid@example.com", "ValidPassword123", "User");

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
        var request = new RegisterRequest(username, "valid@example.com", "ValidPassword123");

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
        var request = new RegisterRequest("validuser", email, "ValidPassword123");

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
        var request = new RegisterRequest("validuser", "valid@example.com", password);

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
        var request = new RegisterRequest("validuser", "valid@example.com", "ValidPassword123", role);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }
}