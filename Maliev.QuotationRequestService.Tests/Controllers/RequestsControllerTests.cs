using Maliev.QuotationRequestService.Api.Controllers;
using Maliev.QuotationRequestService.Api.DTOs;
using Maliev.QuotationRequestService.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Maliev.QuotationRequestService.Tests.Controllers
{
    public class RequestsControllerTests
    {
        private readonly Mock<IQuotationRequestServiceService> _mockService;
        private readonly RequestsController _controller;

        public RequestsControllerTests()
        {
            _mockService = new Mock<IQuotationRequestServiceService>();
            _controller = new RequestsController(_mockService.Object);
        }

        [Fact]
        public async Task GetRequests_ReturnsOkResult_WithListOfRequests()
        {
            // Arrange
            var requests = new List<RequestDto>
            {
                new RequestDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Country = "USA", Message = "Test Message 1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
                new RequestDto { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Country = "Canada", Message = "Test Message 2", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetRequestsAsync()).ReturnsAsync(requests);

            // Act
            var result = await _controller.GetRequests();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRequests = Assert.IsType<List<RequestDto>>(okResult.Value);
            Assert.Equal(2, returnedRequests.Count);
        }

        [Fact]
        public async Task GetRequest_ReturnsOkResult_WithRequest_WhenFound()
        {
            // Arrange
            var request = new RequestDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Country = "USA", Message = "Test Message 1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _mockService.Setup(s => s.GetRequestByIdAsync(1)).ReturnsAsync(request);

            // Act
            var result = await _controller.GetRequest(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRequest = Assert.IsType<RequestDto>(okResult.Value);
            Assert.Equal(1, returnedRequest.Id);
        }

        [Fact]
        public async Task GetRequest_ReturnsNotFoundResult_WhenNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.GetRequestByIdAsync(It.IsAny<int>())).ReturnsAsync((RequestDto?)null);

            // Act
            var result = await _controller.GetRequest(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createRequest = new CreateRequestRequest { FirstName = "New", LastName = "User", Email = "new.user@example.com", Country = "UK", Message = "New Request" };
            var createdRequestDto = new RequestDto { Id = 1, FirstName = "New", LastName = "User", Email = "new.user@example.com", Country = "UK", Message = "New Request", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateRequestAsync(It.IsAny<CreateRequestRequest>())).ReturnsAsync(createdRequestDto);

            // Act
            var result = await _controller.CreateRequest(createRequest);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedRequest = Assert.IsType<RequestDto>(createdAtActionResult.Value);
            Assert.Equal(1, returnedRequest.Id);
            Assert.Equal(nameof(RequestsController.GetRequest), createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task UpdateRequest_ReturnsNoContentResult_WhenFound()
        {
            // Arrange
            var updateRequest = new UpdateRequestRequest { FirstName = "Updated", LastName = "User", Email = "updated.user@example.com", Country = "Germany", Message = "Updated Message" };
            _mockService.Setup(s => s.UpdateRequestAsync(It.IsAny<int>(), It.IsAny<UpdateRequestRequest>())).ReturnsAsync(new RequestDto { Id = 1, FirstName = "Updated", LastName = "User", Email = "updated.user@example.com", Country = "Germany", Message = "Updated Message", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });

            // Act
            var result = await _controller.UpdateRequest(1, updateRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateRequest_ReturnsNotFoundResult_WhenNotFound()
        {
            // Arrange
            var updateRequest = new UpdateRequestRequest { FirstName = "Updated", LastName = "User", Email = "updated.user@example.com", Country = "Germany", Message = "Updated Message" };
            _mockService.Setup(s => s.UpdateRequestAsync(It.IsAny<int>(), It.IsAny<UpdateRequestRequest>())).ReturnsAsync((RequestDto?)null);

            // Act
            var result = await _controller.UpdateRequest(99, updateRequest);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteRequest_ReturnsNoContentResult_WhenFoundAndDeleted()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteRequestAsync(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteRequest(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteRequest_ReturnsNotFoundResult_WhenNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteRequestAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteRequest(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}