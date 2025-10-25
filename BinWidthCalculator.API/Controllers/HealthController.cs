using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BinWidthCalculator.Infrastructure.Data;

namespace BinWidthCalculator.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            var healthStatus = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                database = canConnect ? "Connected" : "Disconnected",
                version = "1.0.0"
            };

            return canConnect ? Ok(healthStatus) : StatusCode(503, healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new 
            { 
                status = "Unhealthy", 
                timestamp = DateTime.UtcNow,
                error = ex.Message 
            });
        }
    }
}