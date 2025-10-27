using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BinWidthCalculator.Infrastructure.Data;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Infrastructure.Security;
using BinWidthCalculator.Infrastructure.Repositories;
using BinWidthCalculator.Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace BinWidthCalculator.Tests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program> 
{ 
    public string DbName { get; set; } = "DefaultTestDb";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(DbName));

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();
        });
    }
}
