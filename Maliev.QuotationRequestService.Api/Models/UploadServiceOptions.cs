using System.ComponentModel.DataAnnotations;

namespace Maliev.QuotationRequestService.Api.Models;

public class UploadServiceOptions
{
    public const string SectionName = "UploadService";

    [Required]
    public string BaseUrl { get; set; } = null!;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}