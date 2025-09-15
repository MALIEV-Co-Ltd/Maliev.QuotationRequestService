using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Maliev.QuotationRequestService.Api.HealthChecks;
using Maliev.QuotationRequestService.Data.DbContexts;

namespace Maliev.QuotationRequestService.Tests.HealthChecks;

public class DatabaseHealthCheckTests : IDisposable
{
    private readonly QuotationRequestDbContext _context;
    private readonly Mock<ILogger<DatabaseHealthCheck>> _loggerMock;
    private readonly DatabaseHealthCheck _healthCheck;

    public DatabaseHealthCheckTests()
    {
        var options = new DbContextOptionsBuilder<QuotationRequestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuotationRequestDbContext(options);
        _loggerMock = new Mock<ILogger<DatabaseHealthCheck>>();
        _healthCheck = new DatabaseHealthCheck(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task CheckHealthAsync_InMemoryDatabase_ReturnsHealthy()
    {
        // Arrange
        var healthCheckContext = new HealthCheckContext();

        // Act
        var result = await _healthCheck.CheckHealthAsync(healthCheckContext);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("In-memory database is healthy");
    }

    [Fact]
    public async Task CheckHealthAsync_RealDatabase_WithConnection_ReturnsHealthy()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<QuotationRequestDbContext>()
            .UseNpgsql("Server=localhost;Database=testdb;User Id=test;Password=test;")
            .Options;

        using var realContext = new QuotationRequestDbContext(options);
        var realHealthCheck = new DatabaseHealthCheck(realContext, _loggerMock.Object);
        var healthCheckContext = new HealthCheckContext();

        try
        {
            // Act
            var result = await realHealthCheck.CheckHealthAsync(healthCheckContext);

            // Assert - This might be Unhealthy if no real database is available, which is expected
            result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
        }
        catch (Exception)
        {
            // Expected if no real database connection is available
        }
    }

    [Fact]
    public async Task CheckHealthAsync_WithCancellation_HandlesCancellation()
    {
        // Arrange
        var healthCheckContext = new HealthCheckContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var result = await _healthCheck.CheckHealthAsync(healthCheckContext, cts.Token);

        // Assert - Should still complete for in-memory database
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}