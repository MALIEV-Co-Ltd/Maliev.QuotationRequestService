namespace Maliev.QuotationRequestService.Api.Models;

public class QuotationRequestFileDto
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string ObjectName { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? UploadServiceFileId { get; set; }
    public string? Description { get; set; }
    public string? FileCategory { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}