using System.ComponentModel.DataAnnotations;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Models;

public class CreateCommentRequest
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(2000, ErrorMessage = "Content cannot exceed 2000 characters")]
    public required string Content { get; set; }

    public CommentType CommentType { get; set; } = CommentType.Internal;

    public bool IsVisible { get; set; } = true;
}