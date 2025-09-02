namespace Maliev.QuotationRequestService.Api.DTOs
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a request to update an existing request file.
    /// </summary>
    public class UpdateRequestFileRequest
    {
        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        [Required]
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the bucket name.
        /// </summary>
        [Required]
        public required string Bucket { get; set; }

        /// <summary>
        /// Gets or sets the object name.
        /// </summary>
        [Required]
        public required string ObjectName { get; set; }
    }
}