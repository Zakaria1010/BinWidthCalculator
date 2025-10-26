using System.Text;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using BinWidthCalculator.Domain.DTOs;
using BinWidthCalculator.Extensions;
using Microsoft.IdentityModel.Tokens;
using BinWidthCalculator.Application;
using BinWidthCalculator.Infrastructure;
using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Application.DTOs;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Infrastructure.Data;
using BinWidthCalculator.Application.Services;
using BinWidthCalculator.Application.Interfaces;
using BinWidthCalculator.Application.Validators;
using BinWidthCalculator.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BinWidthCalculator.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration.AddBinWidthCalculatorConfiguration(builder.Environment.EnvironmentName);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddBinWidthCalculatorSwagger();

// Configure JWT authentication
builder.Services.AddBinWidthCalculatorAuthentication(builder.Configuration);

// Infrastructure dependency injection
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());

// Layer-specific dependency injection
builder.Services.AddApplication();

var app = builder.Build();

// Swagger
app.UseBinWidthCalculatorSwagger();

// Security middleware
app.UseBinWidthCalculatorSecurity();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbSeeder.SeedAsync(services, app.Environment.IsDevelopment());
}

app.Run();

public partial class Program { }
