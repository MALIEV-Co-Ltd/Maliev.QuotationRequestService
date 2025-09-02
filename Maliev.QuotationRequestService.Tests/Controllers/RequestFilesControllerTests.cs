using Maliev.QuotationRequestService.Api.Controllers;
using Maliev.QuotationRequestService.Api.DTOs;
using Maliev.QuotationRequestService.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Maliev.QuotationRequestService.Tests.New.Controllers
{
    public class RequestFilesControllerTests
    {
        private readonly Mock<IQuotationRequestServiceService> _mockService;
        private readonly RequestFilesController _controller;

        public RequestFilesControllerTests()
        {
            _mockService = new Mock<IQuotationRequestServiceService>();
            _controller = new RequestFilesController(_mockService.Object);
        }

        [Fact]
        public async Task GetRequestFiles_ReturnsOkResult_WithListOfRequestFiles()
        {
            // Arrange
            var requestFiles = new List<RequestFileDto>
            {
                new RequestFileDto { Id = 1, RequestId = 1, Bucket = "test", ObjectName = "file1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
                new RequestFileDto { Id = 2, RequestId = 1, Bucket = "test", ObjectName = "file2", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow }
            };
            _mockService.Setup(s => s.GetRequestFilesAsync(1)).ReturnsAsync(requestFiles);

            // Act
            var result = await _controller.GetRequestFiles(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRequestFiles = Assert.IsType<List<RequestFileDto>>(okResult.Value);
            Assert.Equal(2, returnedRequestFiles.Count);
        }

        [Fact]
        public async Task GetRequestFile_ReturnsOkResult_WithRequestFile_WhenFound()
        {
            // Arrange
            var requestFile = new RequestFileDto { Id = 1, RequestId = 1, Bucket = "test", ObjectName = "file1", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _mockService.Setup(s => s.GetRequestFileByIdAsync(1)).ReturnsAsync(requestFile);

            // Act
            var result = await _controller.GetRequestFile(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRequestFile = Assert.IsType<RequestFileDto>(okResult.Value);
            Assert.Equal(1, returnedRequestFile.Id);
        }

        [Fact]
        public async Task GetRequestFile_ReturnsNotFoundResult_WhenNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.GetRequestFileByIdAsync(It.IsAny<int>())).ReturnsAsync((RequestFileDto?)null);

            // Act
            var result = await _controller.GetRequestFile(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateRequestFile_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var createRequestFile = new CreateRequestFileRequest { RequestId = 1, Bucket = "newbucket", ObjectName = "newfile" };
            var createdRequestFileDto = new RequestFileDto { Id = 1, RequestId = 1, Bucket = "test", ObjectName = "newfile", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            _mockService.Setup(s => s.CreateRequestFileAsync(It.IsAny<CreateRequestFileRequest>())).ReturnsAsync(createdRequestFileDto);

            // Act
            var result = await _controller.CreateRequestFile(createRequestFile);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedRequestFile = Assert.IsType<RequestFileDto>(createdAtActionResult.Value);
            Assert.Equal(1, returnedRequestFile.Id);
            Assert.Equal(nameof(RequestFilesController.GetRequestFile), createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task UpdateRequestFile_ReturnsNoContentResult_WhenFound()
        {
            // Arrange
            var updateRequestFile = new UpdateRequestFileRequest { RequestId = 1, Bucket = "updatedbucket", ObjectName = "updatedfile" };
            _mockService.Setup(s => s.UpdateRequestFileAsync(It.IsAny<int>(), It.IsAny<UpdateRequestFileRequest>())).ReturnsAsync(new RequestFileDto { Id = 1, Bucket = "test", ObjectName = "test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });

            // Act
            var result = await _controller.UpdateRequestFile(1, updateRequestFile);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateRequestFile_ReturnsNotFoundResult_WhenNotFound()
        {
            // Arrange
            var updateRequestFile = new UpdateRequestFileRequest { RequestId = 1, Bucket = "updatedbucket", ObjectName = "updatedfile" };
            _mockService.Setup(s => s.UpdateRequestFileAsync(It.IsAny<int>(), It.IsAny<UpdateRequestFileRequest>())).ReturnsAsync((RequestFileDto?)null);

            // Act
            var result = await _controller.UpdateRequestFile(99, updateRequestFile);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteRequestFile_ReturnsNoContentResult_WhenFoundAndDeleted()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteRequestFileAsync(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteRequestFile(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteRequestFile_ReturnsNotFoundResult_WhenNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteRequestFileAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteRequestFile(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}