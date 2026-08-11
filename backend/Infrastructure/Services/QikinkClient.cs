using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class QikinkClient : IQikinkClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QikinkClient> _logger;
    private readonly QikinkSettings _settings;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly object _tokenLock = new();
    private const int TokenSafetyMarginSeconds = 300;

    public QikinkClient(HttpClient httpClient, ILogger<QikinkClient> logger, IOptions<QikinkSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;

        var baseUrl = _settings.SandboxMode ? _settings.SandboxBaseUrl : _settings.ProductionBaseUrl;
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<object?> SubmitOrderAsync(object orderPayload)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            _logger.LogInformation("Submitting order to Qikink API ({BaseUrl})", _httpClient.BaseAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/order/create")
            {
                Content = JsonContent.Create(orderPayload, options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };
            request.Headers.Add("ClientId", _settings.ClientId);
            request.Headers.Add("Accesstoken", _accessToken!);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Qikink API returned {StatusCode}: {Content}", response.StatusCode, content);
                throw new HttpRequestException(
                    $"Qikink API error: {response.StatusCode} - {content}");
            }

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            _logger.LogInformation("Qikink order submitted successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit order to Qikink");
            throw;
        }
    }

    public async Task<object?> GetOrderStatusAsync(string externalOrderId)
    {
        await EnsureAuthenticatedAsync();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/order?id={externalOrderId}");
            request.Headers.Add("ClientId", _settings.ClientId);
            request.Headers.Add("Accesstoken", _accessToken!);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Qikink order status for {ExternalOrderId}", externalOrderId);
            return null;
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            return;

        lock (_tokenLock)
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                return;
        }

        await RefreshTokenAsync();
    }

    private async Task RefreshTokenAsync()
    {
        var secret = _settings.SandboxMode ? _settings.SandboxSecret : _settings.ClientSecret;

        if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException(
                "Qikink credentials are not configured. Set Qikink__ClientId and " +
                (_settings.SandboxMode ? "Qikink__SandboxSecret" : "Qikink__ClientSecret") +
                " environment variables.");
        }

        _logger.LogInformation("Requesting new Qikink access token (SandboxMode={SandboxMode})", _settings.SandboxMode);

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/api/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "ClientId", _settings.ClientId },
                { "client_secret", secret }
            })
        };

        var response = await _httpClient.SendAsync(tokenRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Qikink token request failed: {StatusCode}", response.StatusCode);
            throw new InvalidOperationException(
                $"Qikink authentication failed ({response.StatusCode}). Check Qikink credentials.");
        }

        var doc = JsonDocument.Parse(content);
        var accessToken = doc.RootElement.GetProperty("Accesstoken").GetString();
        var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 3600;

        if (string.IsNullOrEmpty(accessToken))
            throw new InvalidOperationException("Qikink returned an empty access token.");

        lock (_tokenLock)
        {
            _accessToken = accessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - TokenSafetyMarginSeconds);
        }

        _logger.LogInformation("Qikink access token obtained, expires in {Seconds}s", expiresIn);
    }
}
