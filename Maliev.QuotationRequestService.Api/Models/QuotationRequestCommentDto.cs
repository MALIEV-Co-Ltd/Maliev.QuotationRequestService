using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Models;

public class QuotationRequestCommentDto
{
    public int Id { get; set; }
    public required string AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    public required string Content { get; set; }
    public CommentType CommentType { get; set; }
    public bool IsVisible { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}