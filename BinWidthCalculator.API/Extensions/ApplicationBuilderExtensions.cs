using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.Interfaces;
using BinWidthCalculator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BinWidthCalculator.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseBinWidthCalculatorSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "Bin Width Calculator API v2");
            c.RoutePrefix = string.Empty;
        });

        return app;
    }

    public static IApplicationBuilder UseBinWidthCalculatorSecurity(this IApplicationBuilder app)
    {
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}