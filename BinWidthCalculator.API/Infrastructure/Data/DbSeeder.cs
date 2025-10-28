using Microsoft.Extensions.Logging;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, bool isDevelopment)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userRepository = services.GetRequiredService<IUserRepository>();
            var orderRepository = services.GetRequiredService<IOrderRepository>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();
            var binWidthCalculator = services.GetRequiredService<IBinWidthCalculator>();

            // Ensure /data directory exists in production
            if (!isDevelopment)
            {
                var dataDir = "/data";
                if (!Directory.Exists(dataDir))
                    Directory.CreateDirectory(dataDir);
            }

            context.Database.EnsureCreated();

            // Seed default admin user
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@binwidthcalculator.com",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await userRepository.AddAsync(admin);
                logger.LogInformation("✅ Default admin user created.");
            }

            // ✅ Seed sample orders
            if (!context.Orders.Any())
            {
                var sampleOrders = new List<Order>
                {
                    new Order(
                        Guid.NewGuid(),
                        new List<OrderItem>
                        {
                            new OrderItem(ProductType.PhotoBook, 5),
                            new OrderItem(ProductType.Calendar, 2),
                            new OrderItem(ProductType.Cards, 10)
                        },
                        0 // computed below
                    ),
                    new Order(
                        Guid.NewGuid(),
                        new List<OrderItem>
                        {
                            new OrderItem(ProductType.Canvas, 3),
                            new OrderItem(ProductType.Mug, 4)
                        },
                        0
                    )
                };

                foreach (var order in sampleOrders)
                {
                    order.RequiredBinWidth = binWidthCalculator.CalculateRequiredBinWidth(order.Items);
                    await orderRepository.AddAsync(order);
                }

                logger.LogInformation("Sample orders seeded successfully.");
            }

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding.");
        }
    }
}
