using System.ComponentModel.DataAnnotations;

namespace Maliev.QuotationRequestService.Api.Models;

public class AssignQuotationRequestRequest
{
    [Required(ErrorMessage = "Team member name is required")]
    [StringLength(100, ErrorMessage = "Team member name cannot exceed 100 characters")]
    public required string TeamMemberName { get; set; }

    [StringLength(500, ErrorMessage = "Assignment reason cannot exceed 500 characters")]
    public string? AssignmentReason { get; set; }
}