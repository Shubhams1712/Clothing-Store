namespace Application.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteAsync(string publicId);
}

public class ImageUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}
