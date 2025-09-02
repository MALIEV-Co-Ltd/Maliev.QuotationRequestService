using Asp.Versioning;

namespace Maliev.QuotationRequestService.Api.Controllers
{
    using Maliev.QuotationRequestService.Api.DTOs;
    using Maliev.QuotationRequestService.Api.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller for managing request files.
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/quotationrequests/{requestId}/files")]
    [ApiVersion("1.0")]
    public class RequestFilesController : ControllerBase
    {
        private readonly IQuotationRequestServiceService _quotationRequestServiceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFilesController"/> class.
        /// </summary>
        /// <param name="quotationRequestServiceService">The quotation request service.</param>
        public RequestFilesController(IQuotationRequestServiceService quotationRequestServiceService)
        {
            _quotationRequestServiceService = quotationRequestServiceService;
        }

        /// <summary>
        /// Gets all request files for a specific request.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <returns>A list of request files.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestFileDto>>> GetRequestFiles(int requestId)
        {
            var requestFiles = await _quotationRequestServiceService.GetRequestFilesAsync(requestId);
            return Ok(requestFiles);
        }

        /// <summary>
        /// Gets a specific request file by ID.
        /// </summary>
        /// <param name="id">The ID of the request file.</param>
        /// <returns>The request file.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestFileDto>> GetRequestFile(int id)
        {
            var requestFile = await _quotationRequestServiceService.GetRequestFileByIdAsync(id);
            if (requestFile == null)
            {
                return NotFound();
            }
            return Ok(requestFile);
        }

        /// <summary>
        /// Creates a new request file.
        /// </summary>
        /// <param name="requestFileDto">The request file data.</param>
        /// <returns>The created request file.</returns>
        [HttpPost]
        public async Task<ActionResult<RequestFileDto>> CreateRequestFile(CreateRequestFileRequest requestFileDto)
        {
            var requestFile = await _quotationRequestServiceService.CreateRequestFileAsync(requestFileDto);
            return CreatedAtAction(nameof(GetRequestFile), new { id = requestFile.Id }, requestFile);
        }

        /// <summary>
        /// Updates an existing request file.
        /// </summary>
        /// <param name="id">The ID of the request file to update.</param>
        /// <param name="requestFileDto">The updated request file data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequestFile(int id, UpdateRequestFileRequest requestFileDto)
        {
            var requestFile = await _quotationRequestServiceService.UpdateRequestFileAsync(id, requestFileDto);
            if (requestFile == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes a request file.
        /// </summary>
        /// <param name="id">The ID of the request file to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequestFile(int id)
        {
            var result = await _quotationRequestServiceService.DeleteRequestFileAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}