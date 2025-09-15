using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Maliev.QuotationRequestService.Api.Models;

namespace Maliev.QuotationRequestService.Api.Services;

public class UploadServiceClient : IUploadServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly UploadServiceOptions _options;
    private readonly ILogger<UploadServiceClient> _logger;

    public UploadServiceClient(HttpClient httpClient, IOptions<UploadServiceOptions> options, ILogger<UploadServiceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<FileUploadResponse> UploadFileToPathAsync(string objectPath, IFormFile file)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            form.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync($"path?objectPath={Uri.EscapeDataString(objectPath)}", form);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var uploadResponse = JsonSerializer.Deserialize<FileUploadResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (uploadResponse == null)
            {
                throw new InvalidOperationException("Invalid response from upload service");
            }

            _logger.LogInformation("Successfully uploaded file {FileName} to {ObjectPath}", file.FileName, objectPath);
            return uploadResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} to {ObjectPath}", file.FileName, objectPath);
            throw;
        }
    }

    public async Task<FileDownloadResponse?> DownloadFileByPathAsync(string objectPath)
    {
        try
        {
            var response = await _httpClient.GetAsync($"path?objectPath={Uri.EscapeDataString(objectPath)}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "download";
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            _logger.LogInformation("Successfully downloaded file from {ObjectPath}", objectPath);

            return new FileDownloadResponse
            {
                FileName = fileName,
                ContentType = contentType,
                Content = content,
                FileSize = content.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from {ObjectPath}", objectPath);
            throw;
        }
    }

    public async Task<bool> DeleteFileByPathAsync(string objectPath)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"path?objectPath={Uri.EscapeDataString(objectPath)}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Successfully deleted file at {ObjectPath}", objectPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file at {ObjectPath}", objectPath);
            throw;
        }
    }

    public async Task<bool> FileExistsByPathAsync(string objectPath)
    {
        try
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"path?objectPath={Uri.EscapeDataString(objectPath)}"));
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check file existence at {ObjectPath}", objectPath);
            return false;
        }
    }

    public async Task<string> GenerateSignedUrlByPathAsync(string objectPath, TimeSpan expiration)
    {
        try
        {
            var expirationHours = Math.Ceiling(expiration.TotalHours);
            var response = await _httpClient.PostAsync($"path/signed-url?objectPath={Uri.EscapeDataString(objectPath)}&expirationHours={expirationHours}", null);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully generated signed URL for {ObjectPath}", objectPath);
            return result.Trim('"'); // Remove potential quotes from JSON string response
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate signed URL for {ObjectPath}", objectPath);
            throw;
        }
    }
}