using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Maliev.QuotationRequestService.Api.Models;
using Maliev.QuotationRequestService.Api.Services;

namespace Maliev.QuotationRequestService.Tests.Services;

public class UploadServiceClientTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<UploadServiceClient>> _loggerMock;
    private readonly UploadServiceClient _uploadServiceClient;

    public UploadServiceClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _loggerMock = new Mock<ILogger<UploadServiceClient>>();

        _uploadServiceClient = new UploadServiceClient(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetFileMetadataAsync_ValidFileId_ReturnsMetadata()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var expectedMetadata = new FileMetadata
        {
            Id = fileId,
            FileName = "test.stl",
            FileSize = 1024,
            ContentType = "application/octet-stream"
        };

        var responseContent = JsonSerializer.Serialize(expectedMetadata, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith($"/api/v1/files/{fileId}/metadata")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _uploadServiceClient.GetFileMetadataAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(fileId);
        result.FileName.Should().Be("test.stl");
        result.FileSize.Should().Be(1024);
        result.ContentType.Should().Be("application/octet-stream");
    }

    [Fact]
    public async Task GetFileMetadataAsync_FileNotFound_ReturnsNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith($"/api/v1/files/{fileId}/metadata")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await _uploadServiceClient.GetFileMetadataAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFileMetadataAsync_ServiceUnavailable_ThrowsHttpRequestException()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith($"/api/v1/files/{fileId}/metadata")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act & Assert
        var act = async () => await _uploadServiceClient.GetFileMetadataAsync(fileId);
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetFileMetadataAsync_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith($"/api/v1/files/{fileId}/metadata")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        var act = async () => await _uploadServiceClient.GetFileMetadataAsync(fileId);
        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task GetFileMetadataAsync_WithCancellation_HandlesCancellation()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>((req, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            });

        cts.Cancel();

        // Act & Assert
        var act = async () => await _uploadServiceClient.GetFileMetadataAsync(fileId, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}