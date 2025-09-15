using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Maliev.QuotationRequestService.Api.Models;
using Maliev.QuotationRequestService.Api.Exceptions;
using Maliev.QuotationRequestService.Data.DbContexts;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Services;

public class QuotationRequestService : IQuotationRequestService
{
    private readonly QuotationRequestDbContext _context;
    private readonly IUploadServiceClient _uploadServiceClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<QuotationRequestService> _logger;

    public QuotationRequestService(
        QuotationRequestDbContext context,
        IUploadServiceClient uploadServiceClient,
        IMemoryCache cache,
        ILogger<QuotationRequestService> logger)
    {
        _context = context;
        _uploadServiceClient = uploadServiceClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<QuotationRequestDto> CreateQuotationRequestAsync(CreateQuotationRequestRequest request, List<IFormFile>? files = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var quotationRequest = new QuotationRequest
            {
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CompanyName = request.CompanyName,
                JobTitle = request.JobTitle,
                Subject = request.Subject,
                Description = request.Description,
                Requirements = request.Requirements,
                Industry = request.Industry,
                ProjectTimeline = request.ProjectTimeline,
                EstimatedBudget = request.EstimatedBudget,
                PreferredContactMethod = request.PreferredContactMethod,
                Priority = request.Priority,
                CustomerId = request.CustomerId,
                Status = QuotationRequestStatus.New
            };

            _context.QuotationRequests.Add(quotationRequest);
            await _context.SaveChangesAsync();

            // Handle file uploads
            if (files?.Count > 0)
            {
                var uploadedFiles = new List<QuotationRequestFile>();

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var fileRequest = request.Files?.ElementAtOrDefault(i);

                    // Generate storage path for quotation requests
                    var objectPath = $"quotation-requests/{quotationRequest.Id}/files/{DateTime.UtcNow:yyyyMMdd_HHmmss}_{file.FileName}";

                    try
                    {
                        var uploadResponse = await _uploadServiceClient.UploadFileToPathAsync(objectPath, file);

                        var quotationRequestFile = new QuotationRequestFile
                        {
                            QuotationRequestId = quotationRequest.Id,
                            FileName = file.FileName,
                            ObjectName = uploadResponse.ObjectName,
                            FileSize = file.Length,
                            ContentType = file.ContentType,
                            UploadServiceFileId = uploadResponse.ObjectName,
                            Description = fileRequest?.Description,
                            FileCategory = fileRequest?.FileCategory
                        };

                        uploadedFiles.Add(quotationRequestFile);
                        _context.QuotationRequestFiles.Add(quotationRequestFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload file {FileName} for quotation request {Id}", file.FileName, quotationRequest.Id);
                        // Continue with other files, don't fail the entire request
                    }
                }

                await _context.SaveChangesAsync();
            }

            // Create initial status history
            var statusHistory = new QuotationRequestStatusHistory
            {
                QuotationRequestId = quotationRequest.Id,
                FromStatus = QuotationRequestStatus.New,
                ToStatus = QuotationRequestStatus.New,
                ChangedByTeamMember = "System",
                ChangeReason = "Initial quotation request created"
            };

            _context.QuotationRequestStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Created quotation request {Id} for customer {CustomerEmail}", quotationRequest.Id, quotationRequest.CustomerEmail);

            return await MapToDto(quotationRequest);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create quotation request for {CustomerEmail}", request.CustomerEmail);
            throw;
        }
    }

    public async Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsAsync(
        int page = 1,
        int pageSize = 20,
        QuotationRequestStatus? status = null,
        Priority? priority = null,
        string? assignedTo = null)
    {
        var cacheKey = $"quotation_requests_p{page}_ps{pageSize}_s{status}_pr{priority}_a{assignedTo}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<QuotationRequestDto>? cachedResults))
        {
            return cachedResults!;
        }

        var query = _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments.Where(c => c.IsVisible))
            .Include(qr => qr.StatusHistory)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(qr => qr.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(qr => qr.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(assignedTo))
            query = query.Where(qr => qr.AssignedToTeamMember == assignedTo);

        var quotationRequests = await query
            .OrderByDescending(qr => qr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var results = new List<QuotationRequestDto>();
        foreach (var qr in quotationRequests)
        {
            results.Add(await MapToDto(qr));
        }

        _cache.Set(cacheKey, results, TimeSpan.FromMinutes(5));
        return results;
    }

    public async Task<QuotationRequestDto?> GetQuotationRequestByIdAsync(int id)
    {
        var cacheKey = $"quotation_request_{id}";

        if (_cache.TryGetValue(cacheKey, out QuotationRequestDto? cachedResult))
        {
            return cachedResult;
        }

        var quotationRequest = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments.Where(c => c.IsVisible))
            .Include(qr => qr.StatusHistory)
            .FirstOrDefaultAsync(qr => qr.Id == id);

        if (quotationRequest == null)
            return null;

        var result = await MapToDto(quotationRequest);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<QuotationRequestDto?> GetQuotationRequestByRequestNumberAsync(string requestNumber)
    {
        var quotationRequest = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments)
            .Include(qr => qr.StatusHistory)
            .FirstOrDefaultAsync(qr => qr.RequestNumber == requestNumber);

        return quotationRequest == null ? null : await MapToDto(quotationRequest);
    }

    public async Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsByStatusAsync(QuotationRequestStatus status)
    {
        var cacheKey = $"quotation_requests_status_{status}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<QuotationRequestDto>? cachedResult))
        {
            return cachedResult!;
        }

        var quotationRequests = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments)
            .Include(qr => qr.StatusHistory)
            .Where(qr => qr.Status == status)
            .OrderByDescending(qr => qr.CreatedAt)
            .ToListAsync();

        var result = new List<QuotationRequestDto>();
        foreach (var request in quotationRequests)
        {
            result.Add(await MapToDto(request));
        }

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsByCustomerIdAsync(int customerId)
    {
        var quotationRequests = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments.Where(c => c.IsVisible))
            .Include(qr => qr.StatusHistory)
            .Where(qr => qr.CustomerId == customerId)
            .OrderByDescending(qr => qr.CreatedAt)
            .ToListAsync();

        var results = new List<QuotationRequestDto>();
        foreach (var qr in quotationRequests)
        {
            results.Add(await MapToDto(qr));
        }

        return results;
    }

    public async Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsByCustomerEmailAsync(string email)
    {
        var quotationRequests = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments.Where(c => c.IsVisible))
            .Include(qr => qr.StatusHistory)
            .Where(qr => qr.CustomerEmail.ToLower() == email.ToLower())
            .OrderByDescending(qr => qr.CreatedAt)
            .ToListAsync();

        var results = new List<QuotationRequestDto>();
        foreach (var qr in quotationRequests)
        {
            results.Add(await MapToDto(qr));
        }

        return results;
    }

    // Additional methods will be added in the next part due to length constraints...

    private Task<QuotationRequestDto> MapToDto(QuotationRequest quotationRequest)
    {
        return Task.FromResult(new QuotationRequestDto
        {
            Id = quotationRequest.Id,
            CustomerName = quotationRequest.CustomerName,
            CustomerEmail = quotationRequest.CustomerEmail,
            CustomerPhone = quotationRequest.CustomerPhone,
            CompanyName = quotationRequest.CompanyName,
            JobTitle = quotationRequest.JobTitle,
            Subject = quotationRequest.Subject,
            Description = quotationRequest.Description,
            Requirements = quotationRequest.Requirements,
            Industry = quotationRequest.Industry,
            ProjectTimeline = quotationRequest.ProjectTimeline,
            EstimatedBudget = quotationRequest.EstimatedBudget,
            PreferredContactMethod = quotationRequest.PreferredContactMethod,
            Status = quotationRequest.Status,
            Priority = quotationRequest.Priority,
            AssignedToTeamMember = quotationRequest.AssignedToTeamMember,
            ReviewedAt = quotationRequest.ReviewedAt,
            QuotedAt = quotationRequest.QuotedAt,
            CompletedAt = quotationRequest.CompletedAt,
            CreatedAt = quotationRequest.CreatedAt,
            UpdatedAt = quotationRequest.UpdatedAt,
            CustomerId = quotationRequest.CustomerId,
            Files = quotationRequest.Files.Select(f => new QuotationRequestFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                ObjectName = f.ObjectName,
                FileSize = f.FileSize,
                ContentType = f.ContentType,
                UploadServiceFileId = f.UploadServiceFileId,
                Description = f.Description,
                FileCategory = f.FileCategory,
                CreatedAt = f.CreatedAt
            }).ToList(),
            Comments = quotationRequest.Comments.Select(c => new QuotationRequestCommentDto
            {
                Id = c.Id,
                AuthorName = c.AuthorName,
                AuthorEmail = c.AuthorEmail,
                Content = c.Content,
                CommentType = c.CommentType,
                IsVisible = c.IsVisible,
                CreatedAt = c.CreatedAt
            }).ToList(),
            StatusHistory = quotationRequest.StatusHistory.Select(sh => new QuotationRequestStatusHistoryDto
            {
                Id = sh.Id,
                FromStatus = sh.FromStatus,
                ToStatus = sh.ToStatus,
                ChangedByTeamMember = sh.ChangedByTeamMember,
                ChangeReason = sh.ChangeReason,
                CreatedAt = sh.CreatedAt
            }).OrderBy(sh => sh.CreatedAt).ToList()
        });
    }

    private void InvalidateCache(int quotationRequestId)
    {
        _cache.Remove($"quotation_request_{quotationRequestId}");
        // Remove paginated cache entries - this is a simple approach
        // In production, consider using cache tags or more sophisticated invalidation
    }

    public async Task<QuotationRequestDto> UpdateQuotationRequestStatusAsync(int id, UpdateQuotationRequestStatusRequest request, string changedBy)
    {
        var quotationRequest = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments)
            .Include(qr => qr.StatusHistory)
            .FirstOrDefaultAsync(qr => qr.Id == id);

        if (quotationRequest == null)
            throw new NotFoundException($"Quotation request with ID {id} not found");

        var oldStatus = quotationRequest.Status;
        quotationRequest.Status = request.Status;
        quotationRequest.UpdatedAt = DateTimeOffset.UtcNow;

        // Update status-specific timestamps
        switch (request.Status)
        {
            case QuotationRequestStatus.InReview:
                quotationRequest.ReviewedAt = DateTimeOffset.UtcNow;
                break;
            case QuotationRequestStatus.Quoted:
                quotationRequest.QuotedAt = DateTimeOffset.UtcNow;
                break;
            case QuotationRequestStatus.Accepted:
            case QuotationRequestStatus.Rejected:
            case QuotationRequestStatus.Cancelled:
                quotationRequest.CompletedAt = DateTimeOffset.UtcNow;
                break;
        }

        // Add status history
        var statusHistory = new QuotationRequestStatusHistory
        {
            QuotationRequestId = id,
            FromStatus = oldStatus,
            ToStatus = request.Status,
            ChangedByTeamMember = changedBy,
            ChangeReason = request.Reason
        };

        _context.QuotationRequestStatusHistories.Add(statusHistory);
        await _context.SaveChangesAsync();

        InvalidateCache(id);
        _logger.LogInformation("Updated quotation request {Id} status from {OldStatus} to {NewStatus} by {ChangedBy}", id, oldStatus, request.Status, changedBy);

        return await MapToDto(quotationRequest);
    }

    public async Task<QuotationRequestDto> AssignQuotationRequestAsync(int id, AssignQuotationRequestRequest request)
    {
        var quotationRequest = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .Include(qr => qr.Comments)
            .Include(qr => qr.StatusHistory)
            .FirstOrDefaultAsync(qr => qr.Id == id);

        if (quotationRequest == null)
            throw new NotFoundException($"Quotation request with ID {id} not found");

        quotationRequest.AssignedToTeamMember = request.TeamMemberName;
        quotationRequest.UpdatedAt = DateTimeOffset.UtcNow;

        // Add system comment about assignment
        var assignmentComment = new QuotationRequestComment
        {
            QuotationRequestId = id,
            AuthorName = "System",
            Content = $"Assigned to {request.TeamMemberName}. {request.AssignmentReason}",
            CommentType = CommentType.System,
            IsVisible = true
        };

        _context.QuotationRequestComments.Add(assignmentComment);
        await _context.SaveChangesAsync();

        InvalidateCache(id);
        _logger.LogInformation("Assigned quotation request {Id} to {TeamMember}", id, request.TeamMemberName);

        return await MapToDto(quotationRequest);
    }

    public async Task DeleteQuotationRequestAsync(int id)
    {
        var quotationRequest = await _context.QuotationRequests
            .Include(qr => qr.Files)
            .FirstOrDefaultAsync(qr => qr.Id == id);

        if (quotationRequest == null)
            throw new NotFoundException($"Quotation request with ID {id} not found");

        // Delete files from storage service
        foreach (var file in quotationRequest.Files)
        {
            try
            {
                await _uploadServiceClient.DeleteFileByPathAsync(file.ObjectName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {ObjectName} from storage service", file.ObjectName);
            }
        }

        _context.QuotationRequests.Remove(quotationRequest);
        await _context.SaveChangesAsync();

        InvalidateCache(id);
        _logger.LogInformation("Deleted quotation request {Id}", id);
    }

    public async Task<QuotationRequestCommentDto> AddCommentAsync(int quotationRequestId, CreateCommentRequest request, string authorName, string? authorEmail = null)
    {
        var quotationRequest = await _context.QuotationRequests.FindAsync(quotationRequestId);
        if (quotationRequest == null)
            throw new NotFoundException($"Quotation request with ID {quotationRequestId} not found");

        var comment = new QuotationRequestComment
        {
            QuotationRequestId = quotationRequestId,
            AuthorName = authorName,
            AuthorEmail = authorEmail,
            Content = request.Content,
            CommentType = request.CommentType,
            IsVisible = request.IsVisible
        };

        _context.QuotationRequestComments.Add(comment);
        await _context.SaveChangesAsync();

        InvalidateCache(quotationRequestId);
        _logger.LogInformation("Added comment to quotation request {Id} by {Author}", quotationRequestId, authorName);

        return new QuotationRequestCommentDto
        {
            Id = comment.Id,
            AuthorName = comment.AuthorName,
            AuthorEmail = comment.AuthorEmail,
            Content = comment.Content,
            CommentType = comment.CommentType,
            IsVisible = comment.IsVisible,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<IEnumerable<QuotationRequestCommentDto>> GetCommentsAsync(int quotationRequestId)
    {
        var quotationRequest = await _context.QuotationRequests.FindAsync(quotationRequestId);
        if (quotationRequest == null)
            throw new NotFoundException($"Quotation request with ID {quotationRequestId} not found");

        var comments = await _context.QuotationRequestComments
            .Where(c => c.QuotationRequestId == quotationRequestId && c.IsVisible)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => new QuotationRequestCommentDto
        {
            Id = c.Id,
            AuthorName = c.AuthorName,
            AuthorEmail = c.AuthorEmail,
            Content = c.Content,
            CommentType = c.CommentType,
            IsVisible = c.IsVisible,
            CreatedAt = c.CreatedAt
        });
    }

    public async Task DeleteCommentAsync(int quotationRequestId, int commentId)
    {
        var comment = await _context.QuotationRequestComments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.QuotationRequestId == quotationRequestId);

        if (comment == null)
            throw new NotFoundException($"Comment with ID {commentId} not found for quotation request {quotationRequestId}");

        _context.QuotationRequestComments.Remove(comment);
        await _context.SaveChangesAsync();

        InvalidateCache(quotationRequestId);
        _logger.LogInformation("Deleted comment {CommentId} from quotation request {Id}", commentId, quotationRequestId);
    }

    public async Task<IEnumerable<QuotationRequestFileDto>> GetFilesAsync(int quotationRequestId)
    {
        var quotationRequest = await _context.QuotationRequests.FindAsync(quotationRequestId);
        if (quotationRequest == null)
            throw new NotFoundException($"Quotation request with ID {quotationRequestId} not found");

        var files = await _context.QuotationRequestFiles
            .Where(f => f.QuotationRequestId == quotationRequestId)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync();

        return files.Select(f => new QuotationRequestFileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            ObjectName = f.ObjectName,
            FileSize = f.FileSize,
            ContentType = f.ContentType,
            UploadServiceFileId = f.UploadServiceFileId,
            Description = f.Description,
            FileCategory = f.FileCategory,
            CreatedAt = f.CreatedAt
        });
    }

    public async Task<QuotationRequestFileDto?> GetFileByIdAsync(int quotationRequestId, int fileId)
    {
        var file = await _context.QuotationRequestFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.QuotationRequestId == quotationRequestId);

        if (file == null)
            return null;

        return new QuotationRequestFileDto
        {
            Id = file.Id,
            FileName = file.FileName,
            ObjectName = file.ObjectName,
            FileSize = file.FileSize,
            ContentType = file.ContentType,
            UploadServiceFileId = file.UploadServiceFileId,
            Description = file.Description,
            FileCategory = file.FileCategory,
            CreatedAt = file.CreatedAt
        };
    }

    public async Task DeleteFileAsync(int quotationRequestId, int fileId)
    {
        var file = await _context.QuotationRequestFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.QuotationRequestId == quotationRequestId);

        if (file == null)
            throw new NotFoundException($"File with ID {fileId} not found for quotation request {quotationRequestId}");

        try
        {
            await _uploadServiceClient.DeleteFileByPathAsync(file.ObjectName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file {ObjectName} from storage service", file.ObjectName);
        }

        _context.QuotationRequestFiles.Remove(file);
        await _context.SaveChangesAsync();

        InvalidateCache(quotationRequestId);
        _logger.LogInformation("Deleted file {FileId} from quotation request {Id}", fileId, quotationRequestId);
    }

    public async Task<IEnumerable<QuotationRequestStatusHistoryDto>> GetStatusHistoryAsync(int quotationRequestId)
    {
        var quotationRequest = await _context.QuotationRequests.FindAsync(quotationRequestId);
        if (quotationRequest == null)
            throw new NotFoundException($"Quotation request with ID {quotationRequestId} not found");

        var statusHistory = await _context.QuotationRequestStatusHistories
            .Where(sh => sh.QuotationRequestId == quotationRequestId)
            .OrderBy(sh => sh.CreatedAt)
            .ToListAsync();

        return statusHistory.Select(sh => new QuotationRequestStatusHistoryDto
        {
            Id = sh.Id,
            FromStatus = sh.FromStatus,
            ToStatus = sh.ToStatus,
            ChangedByTeamMember = sh.ChangedByTeamMember,
            ChangeReason = sh.ChangeReason,
            CreatedAt = sh.CreatedAt
        });
    }
}