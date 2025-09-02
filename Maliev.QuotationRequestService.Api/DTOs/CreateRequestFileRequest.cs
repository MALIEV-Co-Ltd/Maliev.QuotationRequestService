using System.ComponentModel.DataAnnotations;

namespace Maliev.QuotationRequestService.Api.DTOs
{
    /// <summary>
    /// Represents a request to create a new request file.
    /// </summary>
    public class CreateRequestFileRequest
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
        public string Bucket { get; set; }

        /// <summary>
        /// Gets or sets the object name.
        /// </summary>
        [Required]
        public string ObjectName { get; set; }
    }
}