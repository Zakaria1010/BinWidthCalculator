using Microsoft.EntityFrameworkCore;
using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.RequiredBinWidth)
                .HasPrecision(18, 2);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.OwnsMany(e => e.Items, owned =>
            {
                owned.WithOwner().HasForeignKey("OrderId");
                owned.Property<int>("Id").ValueGeneratedOnAdd();
                owned.HasKey("Id");
                
                owned.Property(oi => oi.ProductType)
                    .IsRequired()
                    .HasConversion<string>();
                
                owned.Property(oi => oi.Quantity)
                    .IsRequired();
            });

            entity.Navigation(e => e.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Property);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}