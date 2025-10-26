using BinWidthCalculator.Domain.DTOs;
using FluentValidation;

namespace BinWidthCalculator.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Valid email address is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.Role)
            .Must(role => role == "User" || role == "Admin")
            .WithMessage("Role must be either 'User' or 'Admin'");
    }
}