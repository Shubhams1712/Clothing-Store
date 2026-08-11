using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class LocalImageStorageService : IImageStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalImageStorageService> _logger;
    private const string UploadsFolder = "uploads";

    public LocalImageStorageService(IWebHostEnvironment environment, ILogger<LocalImageStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public Task<ImageUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var uploadsDir = Path.Combine(_environment.WebRootPath, UploadsFolder);
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(fileName);
        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, storedName);

        using var stream = new FileStream(filePath, FileMode.Create);
        fileStream.CopyTo(stream);

        var url = $"/{UploadsFolder}/{storedName}";
        _logger.LogInformation("Image saved locally: {FileName}", storedName);

        return Task.FromResult(new ImageUploadResult
        {
            Url = url,
            PublicId = storedName
        });
    }

    public Task<bool> DeleteAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return Task.FromResult(false);

        var uploadsDir = Path.GetFullPath(Path.Combine(_environment.WebRootPath, UploadsFolder));
        var filePath = Path.GetFullPath(Path.Combine(uploadsDir, publicId));

        if (!filePath.StartsWith(uploadsDir, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt blocked for delete: {PublicId}", publicId);
            return Task.FromResult(false);
        }

        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogWarning("File not found for delete: {PublicId}", publicId);
            return Task.FromResult(false);
        }

        System.IO.File.Delete(filePath);
        _logger.LogInformation("Image deleted locally: {PublicId}", publicId);
        return Task.FromResult(true);
    }
}
