using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.QuotationRequestService.Data.Models;

public class QuotationRequestComment : IAuditable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int QuotationRequestId { get; set; }

    [Required]
    [StringLength(100)]
    public required string AuthorName { get; set; }

    [StringLength(254)]
    [EmailAddress]
    public string? AuthorEmail { get; set; }

    [Required]
    public required string Content { get; set; }

    [Required]
    public CommentType CommentType { get; set; } = CommentType.Internal;

    public bool IsVisible { get; set; } = true;

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation property
    [ForeignKey(nameof(QuotationRequestId))]
    public virtual QuotationRequest QuotationRequest { get; set; } = null!;
}

public enum CommentType
{
    Internal = 0,     // Internal team communication
    Customer = 1,     // Communication with customer
    System = 2        // System-generated comments
}