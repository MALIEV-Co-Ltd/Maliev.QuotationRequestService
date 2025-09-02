namespace Maliev.QuotationRequestService.Api.DTOs
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a request to update an existing request.
    /// </summary>
    public class UpdateRequestRequest
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        [Required]
        public required string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        [Required]
        public required string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the telephone number.
        /// </summary>
        public string? TelephoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        [Required]
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
        [Required]
        public required string Message { get; set; }

        /// <summary>
        /// Gets or sets the internal comment.
        /// </summary>
        public string? InternalComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request is done.
        /// </summary>
        public bool? Done { get; set; }
    }
}