using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Maliev.QuotationRequestService.Api.Models;
using Maliev.QuotationRequestService.Api.Services;
using Maliev.QuotationRequestService.Data.DbContexts;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Tests.Services;

public class QuotationRequestServiceTests : IDisposable
{
    private readonly QuotationRequestDbContext _context;
    private readonly Mock<IUploadServiceClient> _uploadServiceMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<Api.Services.QuotationRequestService>> _loggerMock;
    private readonly Api.Services.QuotationRequestService _service;

    public QuotationRequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuotationRequestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuotationRequestDbContext(options);
        _uploadServiceMock = new Mock<IUploadServiceClient>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<Api.Services.QuotationRequestService>>();

        _service = new Api.Services.QuotationRequestService(
            _context,
            _uploadServiceMock.Object,
            _memoryCache,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateQuotationRequestAsync_ValidRequest_ReturnsCreatedRequest()
    {
        // Arrange
        var request = new CreateQuotationRequestDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            CustomerPhone = "+1234567890",
            CompanyName = "Test Company",
            ProjectDescription = "Test project description",
            RequiredDeliveryDate = DateTime.UtcNow.AddDays(30),
            Budget = 5000,
            FileIds = new List<Guid>()
        };

        // Act
        var result = await _service.CreateQuotationRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerName.Should().Be(request.CustomerName);
        result.CustomerEmail.Should().Be(request.CustomerEmail);
        result.Status.Should().Be(QuotationRequestStatus.New);
        result.RequestNumber.Should().NotBeNullOrEmpty();
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateQuotationRequestAsync_WithFiles_AssociatesFiles()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var request = new CreateQuotationRequestDto
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            FileIds = new List<Guid> { fileId }
        };

        _uploadServiceMock
            .Setup(x => x.GetFileMetadataAsync(fileId))
            .ReturnsAsync(new FileMetadata
            {
                Id = fileId,
                FileName = "test.stl",
                FileSize = 1024,
                ContentType = "application/octet-stream"
            });

        // Act
        var result = await _service.CreateQuotationRequestAsync(request);

        // Assert
        result.Should().NotBeNull();

        var dbRequest = await _context.QuotationRequests
            .Include(x => x.Files)
            .FirstAsync(x => x.Id == result.Id);

        dbRequest.Files.Should().HaveCount(1);
        dbRequest.Files.First().FileId.Should().Be(fileId);
        dbRequest.Files.First().FileName.Should().Be("test.stl");
    }

    [Fact]
    public async Task GetQuotationRequestByIdAsync_ExistingId_ReturnsRequest()
    {
        // Arrange
        var quotationRequest = new QuotationRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Subject = "Test Subject",
            Description = "Test description",
            Status = QuotationRequestStatus.New,
            RequestNumber = "QR-2025-001"
        };

        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetQuotationRequestByIdAsync(quotationRequest.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(quotationRequest.Id);
        result.CustomerName.Should().Be(quotationRequest.CustomerName);
    }

    [Fact]
    public async Task GetQuotationRequestByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetQuotationRequestByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetQuotationRequestByRequestNumberAsync_ExistingNumber_ReturnsRequest()
    {
        // Arrange
        var quotationRequest = new QuotationRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Subject = "Test Subject",
            Description = "Test description",
            Status = QuotationRequestStatus.New,
            RequestNumber = "QR-2025-001"
        };

        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetQuotationRequestByRequestNumberAsync("QR-2025-001");

        // Assert
        result.Should().NotBeNull();
        result!.RequestNumber.Should().Be("QR-2025-001");
    }

    [Fact]
    public async Task UpdateQuotationRequestStatusAsync_ValidStatusChange_UpdatesStatus()
    {
        // Arrange
        var quotationRequest = new QuotationRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Subject = "Test Subject",
            Description = "Test description",
            Status = QuotationRequestStatus.New,
            RequestNumber = "QR-2025-001"
        };

        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateQuotationRequestStatusAsync(
            quotationRequest.Id,
            QuotationRequestStatus.InReview,
            "Test User",
            "Moving to review");

        // Assert
        result.Should().BeTrue();

        var updatedRequest = await _context.QuotationRequests
            .Include(x => x.StatusHistory)
            .FirstAsync(x => x.Id == quotationRequest.Id);

        updatedRequest.Status.Should().Be(QuotationRequestStatus.InReview);
        updatedRequest.StatusHistory.Should().HaveCount(2); // Initial + Update
        updatedRequest.StatusHistory.Last().ToStatus.Should().Be(QuotationRequestStatus.InReview);
        updatedRequest.StatusHistory.Last().ChangedByTeamMember.Should().Be("Test User");
        updatedRequest.StatusHistory.Last().ChangeReason.Should().Be("Moving to review");
    }

    [Fact]
    public async Task AddCommentAsync_ValidComment_AddsComment()
    {
        // Arrange
        var quotationRequest = new QuotationRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Subject = "Test Subject",
            Description = "Test description",
            Status = QuotationRequestStatus.New,
            RequestNumber = "QR-2025-001"
        };

        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Act
        var commentRequest = new CreateCommentRequest
        {
            Content = "This is a test comment"
        };
        var result = await _service.AddCommentAsync(
            quotationRequest.Id,
            commentRequest,
            "Test User");

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("This is a test comment");
        result.AuthorName.Should().Be("Test User");

        var updatedRequest = await _context.QuotationRequests
            .Include(x => x.Comments)
            .FirstAsync(x => x.Id == quotationRequest.Id);

        updatedRequest.Comments.Should().HaveCount(1);
        updatedRequest.Comments.First().Content.Should().Be("This is a test comment");
        updatedRequest.Comments.First().AuthorName.Should().Be("Test User");
    }

    [Theory]
    [InlineData(QuotationRequestStatus.New)]
    [InlineData(QuotationRequestStatus.InReview)]
    [InlineData(QuotationRequestStatus.Quoted)]
    public async Task GetQuotationRequestsByStatusAsync_WithCaching_UsesCache(QuotationRequestStatus status)
    {
        // Arrange
        var quotationRequest = new QuotationRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Subject = "Test Subject",
            Description = "Test Description",
            Status = status,
            RequestNumber = "QR-2025-001"
        };

        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Act - First call
        var result1 = await _service.GetQuotationRequestsByStatusAsync(status);

        // Act - Second call (should use cache)
        var result2 = await _service.GetQuotationRequestsByStatusAsync(status);

        // Assert
        result1.Should().HaveCount(1);
        result2.Should().HaveCount(1);
        result1.Should().BeEquivalentTo(result2);
    }

    public void Dispose()
    {
        _context.Dispose();
        _memoryCache.Dispose();
    }
}