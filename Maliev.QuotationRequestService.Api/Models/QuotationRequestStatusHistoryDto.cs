using Maliev.QuotationRequestService.Data.Models;

namespace Maliev.QuotationRequestService.Api.Models;

public class QuotationRequestStatusHistoryDto
{
    public int Id { get; set; }
    public QuotationRequestStatus FromStatus { get; set; }
    public QuotationRequestStatus ToStatus { get; set; }
    public string? ChangedByTeamMember { get; set; }
    public string? ChangeReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}