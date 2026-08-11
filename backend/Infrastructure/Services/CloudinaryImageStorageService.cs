using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly ILogger<CloudinaryImageStorageService> _logger;
    private const string Folder = "ecommerce-store";

    public CloudinaryImageStorageService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryImageStorageService> logger)
    {
        _logger = logger;
        var account = new Account(settings.Value.CloudName, settings.Value.ApiKey, settings.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<UploadResult> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName);
        var publicId = $"{Folder}/{Guid.NewGuid():N}";

        var uploadParams = new RawUploadParams
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
}
