namespace Maliev.QuotationRequestService.Api.DTOs
{
    /// <summary>
    /// Represents a request file data transfer object.
    /// </summary>
    public class RequestFileDto
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// Gets or sets the bucket name.
        /// </summary>
        public required string Bucket { get; set; }

        /// <summary>
        /// Gets or sets the object name.
        /// </summary>
        public required string ObjectName { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the modified date.
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}