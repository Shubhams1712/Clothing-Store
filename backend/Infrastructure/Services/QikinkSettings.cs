namespace Infrastructure.Services;

public class QikinkSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SandboxSecret { get; set; } = string.Empty;
    public bool SandboxMode { get; set; } = true;
    public string SandboxBaseUrl { get; set; } = "https://sandbox.qikink.com";
    public string ProductionBaseUrl { get; set; } = "https://api.qikink.com";
}
