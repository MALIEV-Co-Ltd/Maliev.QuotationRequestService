using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Models;

public class QuotationRequestDto
{
    public int Id { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }
    public string? Requirements { get; set; }
    public string? Industry { get; set; }
    public string? ProjectTimeline { get; set; }
    public decimal? EstimatedBudget { get; set; }
    public string? PreferredContactMethod { get; set; }
    public QuotationRequestStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string? AssignedToTeamMember { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset? QuotedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int? CustomerId { get; set; }
    public List<QuotationRequestFileDto> Files { get; set; } = new();
    public List<QuotationRequestCommentDto> Comments { get; set; } = new();
    public List<QuotationRequestStatusHistoryDto> StatusHistory { get; set; } = new();
}