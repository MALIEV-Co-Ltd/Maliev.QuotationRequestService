namespace Maliev.QuotationRequestService.Api.Services;

public interface IUploadServiceClient
{
    Task<FileUploadResponse> UploadFileToPathAsync(string objectPath, IFormFile file);
    Task<FileDownloadResponse?> DownloadFileByPathAsync(string objectPath);
    Task<bool> DeleteFileByPathAsync(string objectPath);
    Task<bool> FileExistsByPathAsync(string objectPath);
    Task<string> GenerateSignedUrlByPathAsync(string objectPath, TimeSpan expiration);
}

public class FileUploadResponse
{
    public required string ObjectName { get; set; }
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public required string Url { get; set; }
}

public class FileDownloadResponse
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required byte[] Content { get; set; }
    public long FileSize { get; set; }
}