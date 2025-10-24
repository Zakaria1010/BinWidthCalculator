using BinWidthCalculator.Application.Services;
using BinWidthCalculator.Application.Interfaces;
using BinWidthCalculator.Application.Validators;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Infrastructure.Data;
using BinWidthCalculator.Infrastructure.Repositories;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database - Use SQLite in production, ensure directory exists
var dbPath = "/data/binwidthcalculator.db";
var connectionString = builder.Environment.IsDevelopment() 
    ? "Data Source=binwidthcalculator.db"
    : $"Data Source={dbPath}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<IBinWidthCalculator, BinWidthCalculatorService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Validators
builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Initialize database and ensure data directory exists
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure directory exists in production
        if (!app.Environment.IsDevelopment())
        {
            var dataDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
        }
        
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

app.Run();

public partial class Program { }