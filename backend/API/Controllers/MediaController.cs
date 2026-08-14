using Application.Common.Models;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
[EnableRateLimiting("global")]
public class MediaController : ControllerBase
{
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<MediaController> _logger;
    private readonly IWebHostEnvironment _environment;

    public MediaController(IImageStorageService imageStorageService, ILogger<MediaController> logger, IWebHostEnvironment environment)
    {
        _imageStorageService = imageStorageService;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MediaFileResult>>>> GetMediaFiles()
    {
        var files = await _imageStorageService.ListFilesAsync();
        return Ok(ApiResponse<List<MediaFileResult>>.SuccessResponse(files));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<MediaResponse>>> Upload([FromForm] UploadRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest(ApiResponse<MediaResponse>.ErrorResponse("No file provided"));

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(request.File.ContentType))
            return BadRequest(ApiResponse<MediaResponse>.ErrorResponse("Invalid file type. Allowed: JPEG, PNG, WebP, GIF"));

        try
        {
            var ext = Path.GetExtension(request.File.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";

            await using var stream = request.File.OpenReadStream();
            var result = await _imageStorageService.UploadAsync(stream, fileName, request.File.ContentType);

            _logger.LogInformation("File uploaded: {FileName} -> {Url}", fileName, result.Url);

            return Ok(ApiResponse<MediaResponse>.SuccessResponse(new MediaResponse
            {
                Url = result.Url,
                FileName = result.PublicId,
                OriginalName = request.File.FileName,
                ContentType = request.File.ContentType,
                Size = request.File.Length
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", request.File.FileName);
            return StatusCode(500, ApiResponse<MediaResponse>.ErrorResponse($"Upload failed: {ex.Message}"));
        }
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([FromQuery] string url, [FromQuery] string? publicId)
    {
        if (string.IsNullOrEmpty(url))
            return BadRequest(ApiResponse<object>.ErrorResponse("URL is required"));

        string? idToDelete = publicId;

        if (string.IsNullOrEmpty(idToDelete))
        {
            if (url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                idToDelete = Path.GetFileName(url);
            }
            else
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Public ID is required for Cloudinary deletion"));
            }
        }

        var deleted = await _imageStorageService.DeleteAsync(idToDelete);

        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse("File not found or deletion failed"));

        _logger.LogInformation("File deleted: {PublicId}", idToDelete);
        return Ok(ApiResponse<object>.SuccessResponse(new { }));
    }
}

public class UploadRequest
{
    public IFormFile? File { get; set; }
}

public class MediaResponse
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}
