using BinWidthCalculator.Application.Services;
using BinWidthCalculator.Application.Interfaces;
using BinWidthCalculator.Application.Validators;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Domain.DTOs;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using BinWidthCalculator.Application.DTOs;

namespace BinWidthCalculator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<IBinWidthCalculator, BinWidthCalculatorService>();
        services.AddScoped<ITokenService, TokenService>();

        // Application Services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IAuthService, AuthService>();

        // Validators (Application-specific validation)
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();

        return services;
    }
}