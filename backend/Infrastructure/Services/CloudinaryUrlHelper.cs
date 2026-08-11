namespace Infrastructure.Services;

public static class CloudinaryUrlHelper
{
    public static string? ExtractPublicId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!url.Contains("cloudinary.com"))
            return null;

        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;

            var uploadIndex = path.IndexOf("/upload/");
            if (uploadIndex < 0)
                return null;

            var afterUpload = path[(uploadIndex + "/upload/".Length)..];

            var lastSlash = afterUpload.LastIndexOf('/');
            var fileName = lastSlash >= 0 ? afterUpload[(lastSlash + 1)..] : afterUpload;

            var dotIndex = fileName.LastIndexOf('.');
            if (dotIndex > 0)
                fileName = fileName[..dotIndex];

            var folderPath = lastSlash >= 0 ? afterUpload[..lastSlash] : string.Empty;

            return string.IsNullOrEmpty(folderPath)
                ? fileName
                : $"{folderPath}/{fileName}";
        }
        catch
        {
            return null;
        }
    }
}
