using Microsoft.Extensions.Configuration;

namespace BinWidthCalculator.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddBinWidthCalculatorConfiguration(
        this IConfigurationBuilder configurationBuilder, 
        string environmentName)
    {
        return configurationBuilder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables();
    }

    public static string GetJwtSecretKey(this IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:Key"] 
            ?? Environment.GetEnvironmentVariable("Jwt__Key");

        if (string.IsNullOrWhiteSpace(jwtSecretKey))
        {
            throw new Exception("JWT Key is missing from configuration or environment variables.");
        }

        return jwtSecretKey;
    }

    public static string GetJwtIssuer(this IConfiguration configuration)
    {
        return configuration["Jwt:Issuer"] ?? "BinWidthCalculatorAPI";
    }

    public static string GetJwtAudience(this IConfiguration configuration)
    {
        return configuration["Jwt:Audience"] ?? "BinWidthCalculatorUsers";
    }

    public static string GetConnectionStringEnv(this IConfiguration configuration, bool isDevelopment)
    {
        return isDevelopment
            ? "Data Source=binwidthcalculator.db"
            : "Data Source=/data/binwidthcalculator.db";
    }
}