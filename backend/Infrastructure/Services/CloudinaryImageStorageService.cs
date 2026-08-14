using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using UploadResult = Application.Interfaces.ImageUploadResult;

namespace Infrastructure.Services;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryImageStorageService> _logger;
    private const string Folder = "ecommerce-store";

    public CloudinaryImageStorageService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryImageStorageService> logger)
    {
        _logger = logger;
        _settings = settings.Value;
        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<UploadResult> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName);
        var publicId = $"{Folder}/{Guid.NewGuid():N}";

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            PublicId = publicId,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogWarning("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error?.Message}");
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {PublicId}", publicId);

        return new UploadResult
        {
            Url = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
            PublicId = publicId
        };
    }

    public async Task<bool> DeleteAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        try
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Auto
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Image deleted from Cloudinary: {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning("Cloudinary delete returned status {StatusCode} for {PublicId}", result.StatusCode, publicId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image from Cloudinary: {PublicId}", publicId);
            return false;
        }
    }

    public async Task<List<MediaFileResult>> ListFilesAsync()
    {
        try
        {
            var url = $"https://api.cloudinary.com/v1_1/{_settings.CloudName}/resources/search";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_settings.ApiKey}:{_settings.ApiSecret}"))}");

            var response = await httpClient.PostAsJsonAsync(url, new
            {
                expression = $"folder:{Folder}",
                max_results = 500
            });

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<CloudinarySearchResult>(json);

            return result?.Resources?.Select(r => new MediaFileResult
            {
                Url = r.SecureUrl ?? r.Url ?? string.Empty,
                Name = Path.GetFileName(r.PublicId),
                Size = r.Bytes,
                LastModified = r.CreatedAt != default ? new DateTimeOffset(r.CreatedAt).ToUnixTimeSeconds() : 0
            }).ToList() ?? new List<MediaFileResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files from Cloudinary");
            return new List<MediaFileResult>();
        }
    }
}

internal class CloudinarySearchResult
{
    public List<CloudinaryResource> Resources { get; set; } = new();
}

internal class CloudinaryResource
{
    public string PublicId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SecureUrl { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
