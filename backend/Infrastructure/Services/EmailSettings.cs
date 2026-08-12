namespace Infrastructure.Services;

public class EmailSettings
{
    public string ResendApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Clothing Store";
    public string FrontendUrl { get; set; } = "http://localhost:3000";
}
