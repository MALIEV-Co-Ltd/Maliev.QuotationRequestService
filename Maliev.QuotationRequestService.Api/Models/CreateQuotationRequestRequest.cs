using System.ComponentModel.DataAnnotations;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Models;

public class CreateQuotationRequestRequest
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
    public required string CustomerName { get; set; }

    [Required(ErrorMessage = "Customer email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [StringLength(254, ErrorMessage = "Email cannot exceed 254 characters")]
    public required string CustomerEmail { get; set; }

    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? CustomerPhone { get; set; }

    [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string? CompanyName { get; set; }

    [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
    public string? JobTitle { get; set; }

    [Required(ErrorMessage = "Subject is required")]
    [StringLength(500, ErrorMessage = "Subject cannot exceed 500 characters")]
    public required string Subject { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public required string Description { get; set; }

    [StringLength(500, ErrorMessage = "Requirements cannot exceed 500 characters")]
    public string? Requirements { get; set; }

    [StringLength(50, ErrorMessage = "Industry cannot exceed 50 characters")]
    public string? Industry { get; set; }

    [StringLength(100, ErrorMessage = "Project timeline cannot exceed 100 characters")]
    public string? ProjectTimeline { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "Estimated budget must be between 0 and 999,999,999.99")]
    public decimal? EstimatedBudget { get; set; }

    [StringLength(50, ErrorMessage = "Preferred contact method cannot exceed 50 characters")]
    public string? PreferredContactMethod { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    // Optional customer ID for signed-in customers
    public int? CustomerId { get; set; }

    // File attachments (handled via multipart/form-data)
    public List<CreateQuotationRequestFileRequest>? Files { get; set; }
}

public class CreateQuotationRequestFileRequest
{
    [Required(ErrorMessage = "File name is required")]
    [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters")]
    public required string FileName { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "File category cannot exceed 50 characters")]
    public string? FileCategory { get; set; }

    // File content is handled separately via IFormFile in the controller
}