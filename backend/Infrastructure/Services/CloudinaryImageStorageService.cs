using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
    private readonly CloudinarySettings _configSettings;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CloudinaryImageStorageService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private Cloudinary? _cloudinary;
    private bool _initialized;
    private const string Folder = "ecommerce-store";

    public CloudinaryImageStorageService(
        IOptions<CloudinarySettings> configSettings,
        ApplicationDbContext context,
        ILogger<CloudinaryImageStorageService> logger)
    {
        _configSettings = configSettings.Value;
        _context = context;
        _logger = logger;
    }

    private async Task<Cloudinary> GetCloudinaryAsync()
    {
        if (_cloudinary is not null && _initialized)
            return _cloudinary;

        await _initLock.WaitAsync();
        try
        {
            if (_cloudinary is not null && _initialized)
                return _cloudinary;

            var (cloudName, apiKey, apiSecret) = await GetCredentialsAsync();

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary is not configured. Go to Admin → Settings and set your Cloudinary credentials.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _initialized = true;
            return _cloudinary;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task<(string? CloudName, string? ApiKey, string? ApiSecret)> GetCredentialsAsync()
    {
        try
        {
            var dbSettings = await _context.StoreSettings.FirstOrDefaultAsync();
            if (dbSettings is not null &&
                !string.IsNullOrWhiteSpace(dbSettings.CloudinaryCloudName) &&
                !string.IsNullOrWhiteSpace(dbSettings.CloudinaryApiKey) &&
                !string.IsNullOrWhiteSpace(dbSettings.CloudinaryApiSecret))
            {
                _logger.LogInformation("Using Cloudinary credentials from database.");
                return (dbSettings.CloudinaryCloudName, dbSettings.CloudinaryApiKey, dbSettings.CloudinaryApiSecret);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read Cloudinary credentials from database. Falling back to config.");
        }

        if (!string.IsNullOrWhiteSpace(_configSettings.CloudName) &&
            !string.IsNullOrWhiteSpace(_configSettings.ApiKey) &&
            !string.IsNullOrWhiteSpace(_configSettings.ApiSecret))
        {
            _logger.LogInformation("Using Cloudinary credentials from configuration.");
            return (_configSettings.CloudName, _configSettings.ApiKey, _configSettings.ApiSecret);
        }

        return (null, null, null);
    }

    public async Task<UploadResult> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var cloudinary = await GetCloudinaryAsync();
        var publicId = $"{Folder}/{Guid.NewGuid():N}";

        try
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = publicId,
                Overwrite = false
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

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
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image to Cloudinary: {FileName}", fileName);
            throw new InvalidOperationException($"Image upload failed: {ex.Message}");
        }
    }

    public async Task<bool> DeleteAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        try
        {
            var cloudinary = await GetCloudinaryAsync();

            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Auto
            };

            var result = await cloudinary.DestroyAsync(deleteParams);

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
            var (cloudName, apiKey, apiSecret) = await GetCredentialsAsync();

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
                return new List<MediaFileResult>();

            var url = $"https://api.cloudinary.com/v1_1/{cloudName}/resources/search";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"))}");

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
