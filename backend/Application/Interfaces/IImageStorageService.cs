namespace Application.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteAsync(string publicId);
    Task<List<MediaFileResult>> ListFilesAsync();
}

public class ImageUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}

public class MediaFileResult
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public long LastModified { get; set; }
}
