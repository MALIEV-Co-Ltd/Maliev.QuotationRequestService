using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Maliev.QuotationRequestService.Data.DbContexts;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Tests.Data;

public class QuotationRequestDbContextTests : IDisposable
{
    private readonly QuotationRequestDbContext _context;

    public QuotationRequestDbContextTests()
    {
        var options = new DbContextOptionsBuilder<QuotationRequestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new QuotationRequestDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsync_NewQuotationRequest_SetsAuditFields()
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

        // Act
        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Assert
        quotationRequest.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        quotationRequest.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        quotationRequest.CreatedAt.Should().Be(quotationRequest.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_UpdateQuotationRequest_UpdatesUpdatedAt()
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

        var originalCreatedAt = quotationRequest.CreatedAt;
        var originalUpdatedAt = quotationRequest.UpdatedAt;

        // Wait a bit to ensure timestamp difference
        await Task.Delay(10);

        // Act
        quotationRequest.Status = QuotationRequestStatus.InReview;
        await _context.SaveChangesAsync();

        // Assert
        quotationRequest.CreatedAt.Should().Be(originalCreatedAt); // Should not change
        quotationRequest.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task QuotationRequest_WithFiles_SavesCorrectly()
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

        var file = new QuotationRequestFile
        {
            QuotationRequest = quotationRequest,
            UploadServiceFileId = Guid.NewGuid(),
            ObjectName = "test-file",
            FileName = "test.stl",
            FileSize = 1024,
            ContentType = "application/octet-stream"
        };

        quotationRequest.Files.Add(file);

        // Act
        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Assert
        var savedRequest = await _context.QuotationRequests
            .Include(x => x.Files)
            .FirstAsync(x => x.Id == quotationRequest.Id);

        savedRequest.Files.Should().HaveCount(1);
        savedRequest.Files.First().FileName.Should().Be("test.stl");
        savedRequest.Files.First().FileSize.Should().Be(1024);
    }

    [Fact]
    public async Task QuotationRequest_WithStatusHistory_SavesCorrectly()
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

        var statusHistory = new QuotationRequestStatusHistory
        {
            QuotationRequest = quotationRequest,
            FromStatus = QuotationRequestStatus.New,
            ToStatus = QuotationRequestStatus.InReview,
            ChangedByTeamMember = "Test User",
            Notes = "Moving to review"
        };

        quotationRequest.StatusHistory.Add(statusHistory);

        // Act
        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Assert
        var savedRequest = await _context.QuotationRequests
            .Include(x => x.StatusHistory)
            .FirstAsync(x => x.Id == quotationRequest.Id);

        savedRequest.StatusHistory.Should().HaveCount(1);
        savedRequest.StatusHistory.First().ToStatus.Should().Be(QuotationRequestStatus.InReview);
        savedRequest.StatusHistory.First().ChangedByTeamMember.Should().Be("Test User");
        savedRequest.StatusHistory.First().ChangeReason.Should().Be("Moving to review");
    }

    [Fact]
    public async Task QuotationRequest_WithComments_SavesCorrectly()
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

        var comment = new QuotationRequestComment
        {
            QuotationRequest = quotationRequest,
            Content = "This is a test comment",
            AuthorName = "Test User"
        };

        quotationRequest.Comments.Add(comment);

        // Act
        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Assert
        var savedRequest = await _context.QuotationRequests
            .Include(x => x.Comments)
            .FirstAsync(x => x.Id == quotationRequest.Id);

        savedRequest.Comments.Should().HaveCount(1);
        savedRequest.Comments.First().Content.Should().Be("This is a test comment");
        savedRequest.Comments.First().AuthorName.Should().Be("Test User");
        savedRequest.Comments.First().CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task QuotationRequest_CascadeDelete_DeletesRelatedEntities()
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

        var file = new QuotationRequestFile
        {
            QuotationRequest = quotationRequest,
            UploadServiceFileId = Guid.NewGuid(),
            ObjectName = "test-file",
            FileName = "test.stl",
            FileSize = 1024,
            ContentType = "application/octet-stream"
        };

        var comment = new QuotationRequestComment
        {
            QuotationRequest = quotationRequest,
            Content = "Test comment",
            AuthorName = "Test User"
        };

        var statusHistory = new QuotationRequestStatusHistory
        {
            QuotationRequest = quotationRequest,
            FromStatus = QuotationRequestStatus.New,
            ToStatus = QuotationRequestStatus.InReview,
            ChangedByTeamMember = "Test User"
        };

        quotationRequest.Files.Add(file);
        quotationRequest.Comments.Add(comment);
        quotationRequest.StatusHistory.Add(statusHistory);

        _context.QuotationRequests.Add(quotationRequest);
        await _context.SaveChangesAsync();

        // Act
        _context.QuotationRequests.Remove(quotationRequest);
        await _context.SaveChangesAsync();

        // Assert
        var filesCount = await _context.Set<QuotationRequestFile>().CountAsync();
        var commentsCount = await _context.Set<QuotationRequestComment>().CountAsync();
        var statusHistoryCount = await _context.Set<QuotationRequestStatusHistory>().CountAsync();

        filesCount.Should().Be(0);
        commentsCount.Should().Be(0);
        statusHistoryCount.Should().Be(0);
    }

    [Fact]
    public async Task QuotationRequest_UniqueConstraints_EnforcesUniqueness()
    {
        // Arrange
        var request1 = new QuotationRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Subject = "Test Subject 1",
            Description = "Test description 1",
            Status = QuotationRequestStatus.New,
            RequestNumber = "QR-2025-UNIQUE"
        };

        var request2 = new QuotationRequest
        {
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            Subject = "Test Subject 2",
            Description = "Test project 2",
            Status = QuotationRequestStatus.New,
            RequestNumber = "QR-2025-UNIQUE" // Same request number
        };

        _context.QuotationRequests.Add(request1);
        await _context.SaveChangesAsync();

        _context.QuotationRequests.Add(request2);

        // Act & Assert
        var act = async () => await _context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}