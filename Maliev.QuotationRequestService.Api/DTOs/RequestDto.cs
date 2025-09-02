namespace Maliev.QuotationRequestService.Api.DTOs
{
    /// <summary>
/// Represents a request data transfer object.
/// </summary>
public class RequestDto
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the telephone number.
    /// </summary>
    public string? TelephoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public required string Country { get; set; }

    /// <summary>
    /// Gets or sets the company name.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the tax identification.
    /// </summary>
    public string? TaxIdentification { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the internal comment.
    /// </summary>
    public string? InternalComment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request is done.
    /// </summary>
    public bool? Done { get; set; }

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the collection of request files.
    /// </summary>
    public ICollection<RequestFileDto> RequestFiles { get; set; } = new List<RequestFileDto>();
}
}