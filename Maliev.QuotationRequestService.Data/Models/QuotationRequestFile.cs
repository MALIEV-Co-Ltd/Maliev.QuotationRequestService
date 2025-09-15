using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.QuotationRequestService.Data.Models;

public class QuotationRequestFile : IAuditable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int QuotationRequestId { get; set; }

    [Required]
    [StringLength(255)]
    public required string FileName { get; set; }

    [Required]
    [StringLength(500)]
    public required string ObjectName { get; set; }

    public long? FileSize { get; set; }

    [StringLength(100)]
    public string? ContentType { get; set; }

    [StringLength(100)]
    public string? UploadServiceFileId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? FileCategory { get; set; } // e.g., "CAD", "Reference", "Specification"

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation property
    [ForeignKey(nameof(QuotationRequestId))]
    public virtual QuotationRequest QuotationRequest { get; set; } = null!;
}