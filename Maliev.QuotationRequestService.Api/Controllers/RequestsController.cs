using Asp.Versioning;

namespace Maliev.QuotationRequestService.Api.Controllers
{
    using Maliev.QuotationRequestService.Api.DTOs;
    using Maliev.QuotationRequestService.Api.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller for managing quotation requests.
    /// </summary>
    [ApiController]
    [Route("v{version:apiVersion}/quotationrequests")]
    [ApiVersion("1.0")]
    public class RequestsController : ControllerBase
    {
        private readonly IQuotationRequestServiceService _quotationRequestServiceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestsController"/> class.
        /// </summary>
        /// <param name="quotationRequestServiceService">The quotation request service.</param>
        public RequestsController(IQuotationRequestServiceService quotationRequestServiceService)
        {
            _quotationRequestServiceService = quotationRequestServiceService;
        }

        /// <summary>
        /// Gets all quotation requests.
        /// </summary>
        /// <returns>A list of quotation requests.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestDto>>> GetRequests()
        {
            var requests = await _quotationRequestServiceService.GetRequestsAsync();
            return Ok(requests);
        }

        /// <summary>
        /// Gets a specific quotation request by ID.
        /// </summary>
        /// <param name="id">The ID of the quotation request.</param>
        /// <returns>The quotation request.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestDto>> GetRequest(int id)
        {
            var request = await _quotationRequestServiceService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            return Ok(request);
        }

        /// <summary>
        /// Creates a new quotation request.
        /// </summary>
        /// <param name="requestDto">The quotation request data.</param>
        /// <returns>The created quotation request.</returns>
        [HttpPost]
        public async Task<ActionResult<RequestDto>> CreateRequest(CreateRequestRequest requestDto)
        {
            var request = await _quotationRequestServiceService.CreateRequestAsync(requestDto);
            return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
        }

        /// <summary>
        /// Updates an existing quotation request.
        /// </summary>
        /// <param name="id">The ID of the quotation request to update.</param>
        /// <param name="requestDto">The updated quotation request data.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, UpdateRequestRequest requestDto)
        {
            var request = await _quotationRequestServiceService.UpdateRequestAsync(id, requestDto);
            if (request == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes a quotation request.
        /// </summary>
        /// <param name="id">The ID of the quotation request to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var result = await _quotationRequestServiceService.DeleteRequestAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}