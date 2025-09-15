using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.QuotationRequestService.Data.Models;

public class QuotationRequest : IAuditable
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public required string CustomerName { get; set; }

    [Required]
    [StringLength(254)]
    [EmailAddress]
    public required string CustomerEmail { get; set; }

    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(100)]
    public string? JobTitle { get; set; }

    [Required]
    [StringLength(500)]
    public required string Subject { get; set; }

    [Required]
    public required string Description { get; set; }

    [StringLength(500)]
    public string? Requirements { get; set; }

    [StringLength(50)]
    public string? Industry { get; set; }

    [StringLength(100)]
    public string? ProjectTimeline { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedBudget { get; set; }

    [StringLength(50)]
    public string? PreferredContactMethod { get; set; }

    [Required]
    public QuotationRequestStatus Status { get; set; } = QuotationRequestStatus.New;

    [Required]
    public Priority Priority { get; set; } = Priority.Medium;

    [StringLength(100)]
    public string? AssignedToTeamMember { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset? QuotedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Customer identification (optional - for signed-in customers)
    public int? CustomerId { get; set; }

    // Navigation properties
    public virtual ICollection<QuotationRequestFile> Files { get; set; } = new List<QuotationRequestFile>();
    public virtual ICollection<QuotationRequestComment> Comments { get; set; } = new List<QuotationRequestComment>();
    public virtual ICollection<QuotationRequestStatusHistory> StatusHistory { get; set; } = new List<QuotationRequestStatusHistory>();
}

public enum QuotationRequestStatus
{
    New = 0,
    InReview = 1,
    AdditionalInfoRequired = 2,
    UnderEvaluation = 3,
    QuotationPreparing = 4,
    Quoted = 5,
    Accepted = 6,
    Rejected = 7,
    Cancelled = 8,
    OnHold = 9
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}