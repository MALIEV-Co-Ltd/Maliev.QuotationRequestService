using Maliev.QuotationRequestService.Api.DTOs;
using Maliev.QuotationRequestService.Data.Database;
using Maliev.QuotationRequestService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.QuotationRequestService.Api.Services.Implementations
{
    /// <summary>
    /// Service for managing quotation requests and request files.
    /// </summary>
    public class QuotationRequestServiceService : IQuotationRequestServiceService
    {
        private readonly QuotationRequestContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotationRequestServiceService"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public QuotationRequestServiceService(QuotationRequestContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all quotation requests.
        /// </summary>
        /// <returns>A collection of <see cref="RequestDto"/>.</returns>
        public async Task<IEnumerable<RequestDto>> GetRequestsAsync()
        {
            return await _context.Requests
                .Include(r => r.RequestFiles)
                .Select(r => new RequestDto
                {
                    Id = r.Id,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Email = r.Email,
                    TelephoneNumber = r.TelephoneNumber,
                    Country = r.Country,
                    CompanyName = r.CompanyName,
                    TaxIdentification = r.TaxIdentification,
                    Message = r.Message,
                    InternalComment = r.InternalComment,
                    Done = r.Done,
                    CreatedDate = r.CreatedDate,
                    ModifiedDate = r.ModifiedDate,
                    RequestFiles = r.RequestFiles.Select(rf => new RequestFileDto
                    {
                        Id = rf.Id,
                        RequestId = rf.RequestId,
                        Bucket = rf.Bucket,
                        ObjectName = rf.ObjectName,
                        CreatedDate = rf.CreatedDate,
                        ModifiedDate = rf.ModifiedDate
                    }).ToList()
                })
                .ToListAsync();
        }

        /// <summary>
        /// Gets a quotation request by its ID.
        /// </summary>
        /// <param name="id">The ID of the request.</param>
        /// <returns>The <see cref="RequestDto"/> if found, otherwise null.</returns>
        public async Task<RequestDto?> GetRequestByIdAsync(int id)
        {
            var request = await _context.Requests
                .Include(r => r.RequestFiles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return null;
            }

            return new RequestDto
            {
                Id = request.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                TelephoneNumber = request.TelephoneNumber,
                Country = request.Country,
                CompanyName = request.CompanyName,
                TaxIdentification = request.TaxIdentification,
                Message = request.Message,
                InternalComment = request.InternalComment,
                Done = request.Done,
                CreatedDate = request.CreatedDate,
                ModifiedDate = request.ModifiedDate,
                RequestFiles = request.RequestFiles.Select(rf => new RequestFileDto
                {
                    Id = rf.Id,
                    RequestId = rf.RequestId,
                    Bucket = rf.Bucket,
                    ObjectName = rf.ObjectName,
                    CreatedDate = rf.CreatedDate,
                    ModifiedDate = rf.ModifiedDate
                }).ToList()
            };
        }

        /// <summary>
        /// Creates a new quotation request.
        /// </summary>
        /// <param name="requestDto">The request data.</param>
        /// <returns>The created <see cref="RequestDto"/>.</returns>
        public async Task<RequestDto> CreateRequestAsync(CreateRequestRequest requestDto)
        {
            var request = new Request
            {
                FirstName = requestDto.FirstName,
                LastName = requestDto.LastName,
                Email = requestDto.Email,
                TelephoneNumber = requestDto.TelephoneNumber,
                Country = requestDto.Country,
                CompanyName = requestDto.CompanyName,
                TaxIdentification = requestDto.TaxIdentification,
                Message = requestDto.Message,
                InternalComment = requestDto.InternalComment,
                Done = requestDto.Done,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            return new RequestDto
            {
                Id = request.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                TelephoneNumber = request.TelephoneNumber,
                Country = request.Country,
                CompanyName = request.CompanyName,
                TaxIdentification = request.TaxIdentification,
                Message = request.Message,
                InternalComment = request.InternalComment,
                Done = request.Done,
                CreatedDate = request.CreatedDate,
                ModifiedDate = request.ModifiedDate
            };
        }

        /// <summary>
        /// Updates an existing quotation request.
        /// </summary>
        /// <param name="id">The ID of the request to update.</param>
        /// <param name="requestDto">The updated request data.</param>
        /// <returns>The updated <see cref="RequestDto"/> if found, otherwise null.</returns>
        public async Task<RequestDto?> UpdateRequestAsync(int id, UpdateRequestRequest requestDto)
        {
            var request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return null;
            }

            request.FirstName = requestDto.FirstName;
            request.LastName = requestDto.LastName;
            request.Email = requestDto.Email;
            request.TelephoneNumber = requestDto.TelephoneNumber;
            request.Country = requestDto.Country;
            request.CompanyName = requestDto.CompanyName;
            request.TaxIdentification = requestDto.TaxIdentification;
            request.Message = requestDto.Message;
            request.InternalComment = requestDto.InternalComment;
            request.Done = requestDto.Done;
            request.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RequestDto
            {
                Id = request.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                TelephoneNumber = request.TelephoneNumber,
                Country = request.Country,
                CompanyName = request.CompanyName,
                TaxIdentification = request.TaxIdentification,
                Message = request.Message,
                InternalComment = request.InternalComment,
                Done = request.Done,
                CreatedDate = request.CreatedDate,
                ModifiedDate = request.ModifiedDate
            };
        }

        /// <summary>
        /// Deletes a quotation request.
        /// </summary>
        /// <param name="id">The ID of the request to delete.</param>
        /// <returns>True if the request was deleted, otherwise false.</returns>
        public async Task<bool> DeleteRequestAsync(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return false;
            }

            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gets all request files for a specific request.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <returns>A collection of <see cref="RequestFileDto"/>.</returns>
        public async Task<IEnumerable<RequestFileDto>> GetRequestFilesAsync(int requestId)
        {
            return await _context.RequestFiles
                .Where(rf => rf.RequestId == requestId)
                .Select(rf => new RequestFileDto
                {
                    Id = rf.Id,
                    RequestId = rf.RequestId,
                    Bucket = rf.Bucket,
                    ObjectName = rf.ObjectName,
                    CreatedDate = rf.CreatedDate,
                    ModifiedDate = rf.ModifiedDate
                })
                .ToListAsync();
        }

        /// <summary>
        /// Gets a request file by its ID.
        /// </summary>
        /// <param name="id">The ID of the request file.</param>
        /// <returns>The <see cref="RequestFileDto"/> if found, otherwise null.</returns>
        public async Task<RequestFileDto?> GetRequestFileByIdAsync(int id)
        {
            var requestFile = await _context.RequestFiles.FindAsync(id);

            if (requestFile == null)
            {
                return null;
            }

            return new RequestFileDto
            {
                Id = requestFile.Id,
                RequestId = requestFile.RequestId,
                Bucket = requestFile.Bucket,
                ObjectName = requestFile.ObjectName,
                CreatedDate = requestFile.CreatedDate,
                ModifiedDate = requestFile.ModifiedDate
            };
        }

        /// <summary>
        /// Creates a new request file.
        /// </summary>
        /// <param name="requestFileDto">The request file data.</param>
        /// <returns>The created <see cref="RequestFileDto"/>.</returns>
        public async Task<RequestFileDto> CreateRequestFileAsync(CreateRequestFileRequest requestFileDto)
        {
            var requestFile = new RequestFile
            {
                RequestId = requestFileDto.RequestId,
                Bucket = requestFileDto.Bucket,
                ObjectName = requestFileDto.ObjectName,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.RequestFiles.Add(requestFile);
            await _context.SaveChangesAsync();

            return new RequestFileDto
            {
                Id = requestFile.Id,
                RequestId = requestFile.RequestId,
                Bucket = requestFile.Bucket,
                ObjectName = requestFile.ObjectName,
                CreatedDate = requestFile.CreatedDate,
                ModifiedDate = requestFile.ModifiedDate
            };
        }

        /// <summary>
        /// Updates an existing request file.
        /// </summary>
        /// <param name="id">The ID of the request file to update.</param>
        /// <param name="requestFileDto">The updated request file data.</param>
        /// <returns>The updated <see cref="RequestFileDto"/> if found, otherwise null.</returns>
        public async Task<RequestFileDto?> UpdateRequestFileAsync(int id, UpdateRequestFileRequest requestFileDto)
        {
            var requestFile = await _context.RequestFiles.FindAsync(id);

            if (requestFile == null)
            {
                return null;
            }

            requestFile.RequestId = requestFileDto.RequestId;
            requestFile.Bucket = requestFileDto.Bucket;
            requestFile.ObjectName = requestFileDto.ObjectName;
            requestFile.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RequestFileDto
            {
                Id = requestFile.Id,
                RequestId = requestFile.RequestId,
                Bucket = requestFile.Bucket,
                ObjectName = requestFile.ObjectName,
                CreatedDate = requestFile.CreatedDate,
                ModifiedDate = requestFile.ModifiedDate
            };
        }

        /// <summary>
        /// Deletes a request file.
        /// </summary>
        /// <param name="id">The ID of the request file to delete.</param>
        /// <returns>True if the request file was deleted, otherwise false.</returns>
        public async Task<bool> DeleteRequestFileAsync(int id)
        {
            var requestFile = await _context.RequestFiles.FindAsync(id);
            if (requestFile == null)
            {
                return false;
            }

            _context.RequestFiles.Remove(requestFile);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}