using Maliev.QuotationRequestService.Api.DTOs;

namespace Maliev.QuotationRequestService.Api.Services
{
    /// <summary>
    /// Defines the contract for the quotation request service.
    /// </summary>
    public interface IQuotationRequestServiceService
    {
        /// <summary>
        /// Gets all quotation requests asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="RequestDto"/>.</returns>
        Task<IEnumerable<RequestDto>> GetRequestsAsync();

        /// <summary>
        /// Gets a quotation request by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="RequestDto"/> if found, otherwise null.</returns>
        Task<RequestDto?> GetRequestByIdAsync(int id);

        /// <summary>
        /// Creates a new quotation request asynchronously.
        /// </summary>
        /// <param name="request">The request data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="RequestDto"/>.</returns>
        Task<RequestDto> CreateRequestAsync(CreateRequestRequest request);

        /// <summary>
        /// Updates an existing quotation request asynchronously.
        /// </summary>
        /// <param name="id">The ID of the request to update.</param>
        /// <param name="request">The updated request data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="RequestDto"/> if found, otherwise null.</returns>
        Task<RequestDto?> UpdateRequestAsync(int id, UpdateRequestRequest request);

        /// <summary>
        /// Deletes a quotation request asynchronously.
        /// </summary>
        /// <param name="id">The ID of the request to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the request was deleted, otherwise false.</returns>
        Task<bool> DeleteRequestAsync(int id);

        /// <summary>
        /// Gets all request files for a specific request asynchronously.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="RequestFileDto"/>.</returns>
        Task<IEnumerable<RequestFileDto>> GetRequestFilesAsync(int requestId);

        /// <summary>
        /// Gets a request file by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the request file.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="RequestFileDto"/> if found, otherwise null.</returns>
        Task<RequestFileDto?> GetRequestFileByIdAsync(int id);

        /// <summary>
        /// Creates a new request file asynchronously.
        /// </summary>
        /// <param name="requestFile">The request file data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="RequestFileDto"/>.</returns>
        Task<RequestFileDto> CreateRequestFileAsync(CreateRequestFileRequest requestFile);

        /// <summary>
        /// Updates an existing request file asynchronously.
        /// </summary>
        /// <param name="id">The ID of the request file to update.</param>
        /// <param name="requestFile">The updated request file data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="RequestFileDto"/> if found, otherwise null.</returns>
        Task<RequestFileDto?> UpdateRequestFileAsync(int id, UpdateRequestFileRequest requestFile);

        /// <summary>
        /// Deletes a request file asynchronously.
        /// </summary>
        /// <param name="id">The ID of the request file to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the request file was deleted, otherwise false.</returns>
        Task<bool> DeleteRequestFileAsync(int id);
    }
}