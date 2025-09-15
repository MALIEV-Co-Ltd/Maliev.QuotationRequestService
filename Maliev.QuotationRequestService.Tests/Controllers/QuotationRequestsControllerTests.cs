using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Maliev.QuotationRequestService.Api.Models;
using Maliev.QuotationRequestService.Data.DbContexts;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Tests.Controllers;

public class QuotationRequestsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public QuotationRequestsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateQuotationRequest_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateQuotationRequestRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            CustomerPhone = "+1234567890",
            CompanyName = "Test Company",
            Subject = "Test project subject",
            Description = "Test project description"
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

        // Act
        var response = await _client.PostAsync("/api/v1/quotation-requests", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<QuotationRequestDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.CustomerName.Should().Be(request.CustomerName);
        result.Status.Should().Be(QuotationRequestStatus.New);
        result.RequestNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateQuotationRequest_MissingRequiredField_ReturnsBadRequest()
    {
        // Arrange
        // Note: This should fail compilation due to missing required properties,
        // which is the intent - to test validation. Using dynamic object instead.
        var requestDict = new Dictionary<string, object>
        {
            ["customerEmail"] = "john@example.com" // Missing required CustomerName, Subject, Description
        };

        var json = JsonSerializer.Serialize(requestDict, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

        // Act
        var response = await _client.PostAsync("/api/v1/quotation-requests", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetQuotationRequestByRequestNumber_ExistingNumber_ReturnsOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuotationRequestDbContext>();

        var quotationRequest = new Data.Models.QuotationRequest
        {
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            ProjectDescription = "Test project",
            Status = QuotationRequestStatus.New,
            RequestNumber = "QR-2025-TEST"
        };

        context.QuotationRequests.Add(quotationRequest);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/quotation-requests/by-request-number/QR-2025-TEST");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<QuotationRequestDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.RequestNumber.Should().Be("QR-2025-TEST");
        result.CustomerName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetQuotationRequestByRequestNumber_NonExistingNumber_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/quotation-requests/by-request-number/NON-EXISTING");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQuotationRequestsByStatus_ValidStatus_ReturnsOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuotationRequestDbContext>();

        var quotationRequest = new Data.Models.QuotationRequest
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            ProjectDescription = "Test project",
            Status = QuotationRequestStatus.InReview,
            RequestNumber = "QR-2025-REVIEW"
        };

        context.QuotationRequests.Add(quotationRequest);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/quotation-requests/by-status/InReview");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<QuotationRequestDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Should().HaveCountGreaterOrEqualTo(1);
        result.Should().Contain(x => x.Status == QuotationRequestStatus.InReview);
    }

    [Fact]
    public async Task GetQuotationRequestsByStatus_InvalidStatus_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/quotation-requests/by-status/InvalidStatus");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HealthCheck_Liveness_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/quotation-requests/liveness");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthCheck_Readiness_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/quotation-requests/readiness");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RateLimiting_ExceedsQuotationRequestLimit_ReturnsTooManyRequests()
    {
        // Arrange
        var request = new CreateQuotationRequestRequest
        {
            CustomerName = "Rate Limit Test",
            CustomerEmail = "ratelimit@example.com",
            Subject = "Rate limit test subject",
            Description = "Rate limit test description",
            Files = new List<CreateQuotationRequestFileRequest>()
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Act - Make 6 requests quickly (limit is 5 per minute)
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 6; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
            tasks.Add(_client.PostAsync("/api/v1/quotation-requests", content));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - At least one should be rate limited
        var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        rateLimitedResponses.Should().NotBeEmpty();
    }
}