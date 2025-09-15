using Maliev.QuotationRequestService.Api.Models;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Services;

public interface IQuotationRequestService
{
    // Public endpoints
    Task<QuotationRequestDto> CreateQuotationRequestAsync(CreateQuotationRequestRequest request, List<IFormFile>? files = null);

    // Admin endpoints
    Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsAsync(
        int page = 1,
        int pageSize = 20,
        QuotationRequestStatus? status = null,
        Priority? priority = null,
        string? assignedTo = null);

    Task<QuotationRequestDto?> GetQuotationRequestByIdAsync(int id);
    Task<QuotationRequestDto?> GetQuotationRequestByRequestNumberAsync(string requestNumber);
    Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsByStatusAsync(QuotationRequestStatus status);
    Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsByCustomerIdAsync(int customerId);
    Task<IEnumerable<QuotationRequestDto>> GetQuotationRequestsByCustomerEmailAsync(string email);

    Task<QuotationRequestDto> UpdateQuotationRequestStatusAsync(int id, UpdateQuotationRequestStatusRequest request, string changedBy);
    Task<QuotationRequestDto> AssignQuotationRequestAsync(int id, AssignQuotationRequestRequest request);
    Task DeleteQuotationRequestAsync(int id);

    // Comments
    Task<QuotationRequestCommentDto> AddCommentAsync(int quotationRequestId, CreateCommentRequest request, string authorName, string? authorEmail = null);
    Task<IEnumerable<QuotationRequestCommentDto>> GetCommentsAsync(int quotationRequestId);
    Task DeleteCommentAsync(int quotationRequestId, int commentId);

    // Files
    Task<IEnumerable<QuotationRequestFileDto>> GetFilesAsync(int quotationRequestId);
    Task<QuotationRequestFileDto?> GetFileByIdAsync(int quotationRequestId, int fileId);
    Task DeleteFileAsync(int quotationRequestId, int fileId);

    // Status History
    Task<IEnumerable<QuotationRequestStatusHistoryDto>> GetStatusHistoryAsync(int quotationRequestId);
}