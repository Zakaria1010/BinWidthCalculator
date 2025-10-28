using BinWidthCalculator.Application.Validators;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Domain.DTOs;
using FluentAssertions;

namespace BinWidthCalculator.Tests.Unit.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new LoginRequest("validuser", "validpassword123");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData(null)]
    public void Validate_InvalidUsername_ReturnsInvalid(string username)
    {
        // Arrange
        var request = new LoginRequest(username, "validpassword123");
        
        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData(null)]
    public void Validate_InvalidPassword_ReturnsInvalid(string password)
    {
        // Arrange
        var request = new LoginRequest("validuser", password);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}