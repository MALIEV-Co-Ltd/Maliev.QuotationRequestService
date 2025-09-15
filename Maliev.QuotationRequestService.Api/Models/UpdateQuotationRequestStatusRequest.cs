using System.ComponentModel.DataAnnotations;
using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Models;

public class UpdateQuotationRequestStatusRequest
{
    [Required(ErrorMessage = "Status is required")]
    public QuotationRequestStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }
}