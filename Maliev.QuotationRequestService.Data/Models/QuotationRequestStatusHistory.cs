using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.QuotationRequestService.Data.Models;

public class QuotationRequestStatusHistory : IAuditable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int QuotationRequestId { get; set; }

    [Required]
    public QuotationRequestStatus FromStatus { get; set; }

    [Required]
    public QuotationRequestStatus ToStatus { get; set; }

    [StringLength(100)]
    public string? ChangedByTeamMember { get; set; }

    [StringLength(500)]
    public string? ChangeReason { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation property
    [ForeignKey(nameof(QuotationRequestId))]
    public virtual QuotationRequest QuotationRequest { get; set; } = null!;
}