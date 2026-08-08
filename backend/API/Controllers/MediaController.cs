using Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IWebHostEnvironment environment, ILogger<MediaController> logger)
    {
        _environment = environment;
        _logger = logger;
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

        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(request.File.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await request.File.CopyToAsync(stream);

        var url = $"/uploads/{fileName}";
        _logger.LogInformation("File uploaded: {FileName}", fileName);

        return Ok(ApiResponse<MediaResponse>.SuccessResponse(new MediaResponse
        {
            Url = url,
            FileName = fileName,
            OriginalName = request.File.FileName,
            ContentType = request.File.ContentType,
            Size = request.File.Length
        }));
    }

    [HttpDelete]
    public ActionResult Delete([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
            return BadRequest(ApiResponse<object>.ErrorResponse("URL is required"));

        var fileName = Path.GetFileName(url);
        var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            _logger.LogInformation("File deleted: {FileName}", fileName);
        }

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
