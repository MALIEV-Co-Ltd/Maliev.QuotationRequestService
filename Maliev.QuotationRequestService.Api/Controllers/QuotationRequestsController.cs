using Asp.Versioning;
using Maliev.QuotationRequestService.Api.Models;
using Maliev.QuotationRequestService.Api.Services;
using Maliev.QuotationRequestService.Api.Exceptions;
using Maliev.QuotationRequestService.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maliev.QuotationRequestService.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/quotation-requests")]
[ApiVersion("1.0")]
public class QuotationRequestsController : ControllerBase
{
    private readonly IQuotationRequestService _quotationRequestService;
    private readonly IUploadServiceClient _uploadService;
    private readonly ILogger<QuotationRequestsController> _logger;

    public QuotationRequestsController(
        IQuotationRequestService quotationRequestService,
        IUploadServiceClient uploadService,
        ILogger<QuotationRequestsController> logger)
    {
        _quotationRequestService = quotationRequestService;
        _uploadService = uploadService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new quotation request (public endpoint)
    /// </summary>
    /// <param name="request">The quotation request data</param>
    /// <param name="files">Optional file attachments</param>
    /// <returns>The created quotation request</returns>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("QuotationRequestPolicy")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<QuotationRequestDto>> CreateQuotationRequest(
        [FromForm] CreateQuotationRequestRequest request,
        [FromForm] List<IFormFile>? files = null)
    {
        try
        {
            var quotationRequest = await _quotationRequestService.CreateQuotationRequestAsync(request, files);
            return CreatedAtAction(nameof(GetQuotationRequest), new { id = quotationRequest.Id }, quotationRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quotation request from {CustomerEmail}", request?.CustomerEmail ?? "unknown");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get all quotation requests with filtering and pagination (admin only)
    /// </summary>
    /// <param name="pagination">Pagination parameters</param>
    /// <param name="status">Filter by status</param>
    /// <param name="priority">Filter by priority</param>
    /// <param name="assignedTo">Filter by assigned team member</param>
    /// <returns>List of quotation requests</returns>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<IEnumerable<QuotationRequestDto>>> GetQuotationRequests(
        [FromQuery] PaginationParameters pagination,
        [FromQuery] QuotationRequestStatus? status = null,
        [FromQuery] Priority? priority = null,
        [FromQuery] string? assignedTo = null)
    {
        try
        {
            var quotationRequests = await _quotationRequestService.GetQuotationRequestsAsync(
                pagination.Page, pagination.PageSize, status, priority, assignedTo);
            return Ok(quotationRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotation requests with filters");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get a specific quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <returns>The quotation request</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<QuotationRequestDto>> GetQuotationRequest(int id)
    {
        try
        {
            var quotationRequest = await _quotationRequestService.GetQuotationRequestByIdAsync(id);
            if (quotationRequest == null)
            {
                return NotFound();
            }

            return Ok(quotationRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotation request with id {Id}", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get quotation requests by customer ID (admin only)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of quotation requests for the customer</returns>
    [HttpGet("by-customer/{customerId}")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<IEnumerable<QuotationRequestDto>>> GetQuotationRequestsByCustomer(int customerId)
    {
        try
        {
            var quotationRequests = await _quotationRequestService.GetQuotationRequestsByCustomerIdAsync(customerId);
            return Ok(quotationRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotation requests for customer {CustomerId}", customerId);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get quotation requests by customer email (admin only)
    /// </summary>
    /// <param name="email">Customer email</param>
    /// <returns>List of quotation requests for the customer</returns>
    [HttpGet("by-email/{email}")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<IEnumerable<QuotationRequestDto>>> GetQuotationRequestsByEmail(string email)
    {
        try
        {
            var quotationRequests = await _quotationRequestService.GetQuotationRequestsByCustomerEmailAsync(email);
            return Ok(quotationRequests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quotation requests for email {Email}", email);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Update quotation request status (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated quotation request</returns>
    [HttpPut("{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<QuotationRequestDto>> UpdateQuotationRequestStatus(int id, UpdateQuotationRequestStatusRequest request)
    {
        try
        {
            var userEmail = User.FindFirst("email")?.Value ?? "unknown";
            var quotationRequest = await _quotationRequestService.UpdateQuotationRequestStatusAsync(id, request, userEmail);
            return Ok(quotationRequest);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating quotation request {Id} status", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Assign quotation request to team member (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <param name="request">Assignment request</param>
    /// <returns>Updated quotation request</returns>
    [HttpPut("{id}/assign")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<QuotationRequestDto>> AssignQuotationRequest(int id, AssignQuotationRequestRequest request)
    {
        try
        {
            var quotationRequest = await _quotationRequestService.AssignQuotationRequestAsync(id, request);
            return Ok(quotationRequest);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning quotation request {Id}", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Delete a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<IActionResult> DeleteQuotationRequest(int id)
    {
        try
        {
            await _quotationRequestService.DeleteQuotationRequestAsync(id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting quotation request {Id}", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Add a comment to a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <param name="request">Comment data</param>
    /// <returns>Created comment</returns>
    [HttpPost("{id}/comments")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<QuotationRequestCommentDto>> AddComment(int id, CreateCommentRequest request)
    {
        try
        {
            var authorName = User.FindFirst("name")?.Value ?? User.FindFirst("email")?.Value ?? "Unknown";
            var authorEmail = User.FindFirst("email")?.Value;
            var comment = await _quotationRequestService.AddCommentAsync(id, request, authorName, authorEmail);
            return CreatedAtAction(nameof(GetComments), new { id }, comment);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to quotation request {Id}", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get comments for a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <returns>List of comments</returns>
    [HttpGet("{id}/comments")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<IEnumerable<QuotationRequestCommentDto>>> GetComments(int id)
    {
        try
        {
            var comments = await _quotationRequestService.GetCommentsAsync(id);
            return Ok(comments);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for quotation request {Id}", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Delete a comment from a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <param name="commentId">Comment ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}/comments/{commentId}")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<IActionResult> DeleteComment(int id, int commentId)
    {
        try
        {
            await _quotationRequestService.DeleteCommentAsync(id, commentId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId} from quotation request {Id}", commentId, id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get files for a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <returns>List of files</returns>
    [HttpGet("{id}/files")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<IEnumerable<QuotationRequestFileDto>>> GetFiles(int id)
    {
        try
        {
            var files = await _quotationRequestService.GetFilesAsync(id);
            return Ok(files);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files for quotation request {Id}", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Download a file from a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <param name="fileId">File ID</param>
    /// <returns>The file content</returns>
    [HttpGet("{id}/files/{fileId}/download")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<IActionResult> DownloadFile(int id, int fileId)
    {
        try
        {
            var file = await _quotationRequestService.GetFileByIdAsync(id, fileId);
            if (file == null || string.IsNullOrEmpty(file.UploadServiceFileId))
            {
                return NotFound();
            }

            var downloadResponse = await _uploadService.DownloadFileByPathAsync(file.ObjectName);
            if (downloadResponse == null)
            {
                return NotFound();
            }

            return File(downloadResponse.Content, downloadResponse.ContentType, downloadResponse.FileName);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId} from quotation request {Id}", fileId, id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Delete a file from a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <param name="fileId">File ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}/files/{fileId}")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<IActionResult> DeleteFile(int id, int fileId)
    {
        try
        {
            await _quotationRequestService.DeleteFileAsync(id, fileId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId} from quotation request {Id}", fileId, id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get status history for a quotation request (admin only)
    /// </summary>
    /// <param name="id">Quotation request ID</param>
    /// <returns>List of status history entries</returns>
    [HttpGet("{id}/status-history")]
    [Authorize(Policy = "AdminOnly")]
    [EnableRateLimiting("GlobalPolicy")]
    public async Task<ActionResult<IEnumerable<QuotationRequestStatusHistoryDto>>> GetStatusHistory(int id)
    {
        try
        {
            var statusHistory = await _quotationRequestService.GetStatusHistoryAsync(id);
            return Ok(statusHistory);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status history for quotation request {Id}", id);
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }
}