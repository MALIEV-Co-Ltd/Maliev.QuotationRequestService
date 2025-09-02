using Maliev.QuotationRequestService.Api.DTOs;
using Maliev.QuotationRequestService.Api.Services.Implementations;
using Maliev.QuotationRequestService.Data.Database;
using Maliev.QuotationRequestService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.QuotationRequestService.Tests.Services
{
    public class QuotationRequestServiceServiceTests : IDisposable
    {
        private readonly QuotationRequestContext _context;
        private readonly QuotationRequestServiceService _service;

        public QuotationRequestServiceServiceTests()
        {
            var options = new DbContextOptionsBuilder<QuotationRequestContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new QuotationRequestContext(options);
            _service = new QuotationRequestServiceService(_context);

            // Ensure the database is clean for each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetRequestsAsync_ReturnsAllRequests()
        {
            // Arrange
            _context.Requests.AddRange(
                new Request { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Country = "USA", Message = "Test Message 1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
                new Request { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Country = "Canada", Message = "Test Message 2", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetRequestsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetRequestByIdAsync_ReturnsRequest_WhenFound()
        {
            // Arrange
            var request = new Request { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Country = "USA", Message = "Test Message 1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetRequestByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetRequestByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Act
            var result = await _service.GetRequestByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateRequestAsync_CreatesAndReturnsRequest()
        {
            // Arrange
            var createRequest = new CreateRequestRequest { FirstName = "New", LastName = "User", Email = "new.user@example.com", Country = "UK", Message = "New Request" };

            // Act
            var result = await _service.CreateRequestAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createRequest.FirstName, result.FirstName);
            Assert.Equal(1, _context.Requests.Count());
        }

        [Fact]
        public async Task UpdateRequestAsync_UpdatesAndReturnsRequest_WhenFound()
        {
            // Arrange
            var existingRequest = new Request { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Country = "USA", Message = "Test Message 1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _context.Requests.Add(existingRequest);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateRequestRequest { FirstName = "Updated", LastName = "User", Email = "updated.user@example.com", Country = "Germany", Message = "Updated Message" };

            // Act
            var result = await _service.UpdateRequestAsync(1, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateRequest.FirstName, result.FirstName);
            Assert.Equal("Updated", _context.Requests.Find(1)?.FirstName);
        }

        [Fact]
        public async Task UpdateRequestAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var updateRequest = new UpdateRequestRequest { FirstName = "Updated", LastName = "User", Email = "updated.user@example.com", Country = "Germany", Message = "Updated Message" };

            // Act
            var result = await _service.UpdateRequestAsync(99, updateRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteRequestAsync_ReturnsTrue_WhenFoundAndDeleted()
        {
            // Arrange
            var request = new Request { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Country = "USA", Message = "Test Message 1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteRequestAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal(0, _context.Requests.Count());
        }

        [Fact]
        public async Task DeleteRequestAsync_ReturnsFalse_WhenNotFound()
        {
            // Act
            var result = await _service.DeleteRequestAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetRequestFilesAsync_ReturnsAllRequestFilesForRequestId()
        {
            // Arrange
            _context.RequestFiles.AddRange(
                new RequestFile { Id = 1, RequestId = 1, Bucket = "bucket1", ObjectName = "object1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
                new RequestFile { Id = 2, RequestId = 1, Bucket = "bucket1", ObjectName = "object2", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
                new RequestFile { Id = 3, RequestId = 2, Bucket = "bucket2", ObjectName = "object3", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetRequestFilesAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetRequestFileByIdAsync_ReturnsRequestFile_WhenFound()
        {
            // Arrange
            var requestFile = new RequestFile { Id = 1, RequestId = 1, Bucket = "bucket1", ObjectName = "object1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _context.RequestFiles.Add(requestFile);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetRequestFileByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetRequestFileByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Act
            var result = await _service.GetRequestFileByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateRequestFileAsync_CreatesAndReturnsRequestFile()
        {
            // Arrange
            var createRequestFile = new CreateRequestFileRequest { RequestId = 1, Bucket = "newbucket", ObjectName = "newobject" };

            // Act
            var result = await _service.CreateRequestFileAsync(createRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createRequestFile.ObjectName, result.ObjectName);
            Assert.Equal(1, _context.RequestFiles.Count());
        }

        [Fact]
        public async Task UpdateRequestFileAsync_UpdatesAndReturnsRequestFile_WhenFound()
        {
            // Arrange
            var existingRequestFile = new RequestFile { Id = 1, RequestId = 1, Bucket = "bucket1", ObjectName = "object1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _context.RequestFiles.Add(existingRequestFile);
            await _context.SaveChangesAsync();

            var updateRequestFile = new UpdateRequestFileRequest { RequestId = 2, Bucket = "updatedbucket", ObjectName = "updatedobject" };

            // Act
            var result = await _service.UpdateRequestFileAsync(1, updateRequestFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateRequestFile.ObjectName, result.ObjectName);
            Assert.Equal("updatedobject", _context.RequestFiles.Find(1)?.ObjectName);
        }

        [Fact]
        public async Task UpdateRequestFileAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var updateRequestFile = new UpdateRequestFileRequest { RequestId = 2, Bucket = "updatedbucket", ObjectName = "updatedobject" };

            // Act
            var result = await _service.UpdateRequestFileAsync(99, updateRequestFile);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteRequestFileAsync_ReturnsTrue_WhenFoundAndDeleted()
        {
            // Arrange
            var requestFile = new RequestFile { Id = 1, RequestId = 1, Bucket = "bucket1", ObjectName = "object1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _context.RequestFiles.Add(requestFile);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteRequestFileAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal(0, _context.RequestFiles.Count());
        }

        [Fact]
        public async Task DeleteRequestFileAsync_ReturnsFalse_WhenNotFound()
        {
            // Act
            var result = await _service.DeleteRequestFileAsync(99);

            // Assert
            Assert.False(result);
        }
    }
}