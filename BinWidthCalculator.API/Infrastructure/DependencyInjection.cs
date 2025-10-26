using BinWidthCalculator.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using BinWidthCalculator.Infrastructure.Data;
using BinWidthCalculator.Infrastructure.Security;
using BinWidthCalculator.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using BinWidthCalculator.Extensions;

namespace BinWidthCalculator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var connectionString = configuration.GetConnectionStringEnv(isDevelopment: true) 
            ?? "Data Source=binwidthcalculator.db";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}