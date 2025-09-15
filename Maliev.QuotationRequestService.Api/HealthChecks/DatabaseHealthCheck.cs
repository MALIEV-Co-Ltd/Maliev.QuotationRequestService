using Maliev.QuotationRequestService.Data.DbContexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace Maliev.QuotationRequestService.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly QuotationRequestDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(QuotationRequestDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // For in-memory databases (testing), just return healthy
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                return HealthCheckResult.Healthy("In-memory database is healthy");
            }

            // For real databases, check connectivity
            await _context.Database.CanConnectAsync(cancellationToken);

            // Optional: Check if we can actually query the database
            var canQuery = await _context.QuotationRequests.AnyAsync(cancellationToken);

            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}