using System.Text.Json;

namespace API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;

        _logger.LogInformation("HTTP {Method} {Path}", method, path);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;
        _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
            method, path, statusCode, stopwatch.ElapsedMilliseconds);
    }
}
