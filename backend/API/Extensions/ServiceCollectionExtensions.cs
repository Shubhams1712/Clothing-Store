using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    private const int MinJwtSecretLength = 32;

    private static readonly HashSet<string> PlaceholderSecrets = new(StringComparer.OrdinalIgnoreCase)
    {
        "your-secret-key-change-in-production",
        "your-secret-key-change-in-production-at-least-32-chars!!",
        "change-this-to-a-real-secret",
        "secret",
        "changeme",
        "your-256-bit-secret",
        "replace-this",
    };

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var envName = configuration["Environment"]
                   ?? configuration["ASPNETCORE_ENVIRONMENT"]
                   ?? "Production";
        var isProduction = !string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(envName, "Local", StringComparison.OrdinalIgnoreCase);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                var frontendUrl = configuration["Frontend:Url"];
                if (string.IsNullOrEmpty(frontendUrl))
                {
                    if (isProduction)
                        throw new InvalidOperationException(
                            "Frontend URL is not configured. Set the Frontend:Url configuration value or the Frontend__Url environment variable. " +
                            "Application cannot start without a configured frontend URL in production.");
                    frontendUrl = "http://localhost:3000";
                }
                builder.WithOrigins(frontendUrl)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        var jwtSecret = configuration["Jwt:Secret"];

        if (string.IsNullOrEmpty(jwtSecret))
        {
            if (isProduction)
                throw new InvalidOperationException(
                    "JWT secret is not configured. Set the Jwt:Secret configuration value or the Jwt__Secret environment variable. " +
                    "Application cannot start without a valid JWT secret in production.");

            Console.WriteLine("[WARNING] Jwt:Secret is not configured. Authentication is disabled. This is acceptable only in Development.");
        }
        else
        {
            if (PlaceholderSecrets.Contains(jwtSecret))
            {
                if (isProduction)
                    throw new InvalidOperationException(
                        "JWT secret appears to be a placeholder value. Set a real, unique secret in Jwt:Secret or Jwt__Secret. " +
                        "Application cannot start with a placeholder secret in production.");

                Console.WriteLine("[WARNING] Jwt:Secret appears to be a placeholder. Authentication may not work correctly.");
            }
            else if (jwtSecret.Length < MinJwtSecretLength)
            {
                if (isProduction)
                    throw new InvalidOperationException(
                        $"JWT secret is too short ({jwtSecret.Length} characters). " +
                        $"It must be at least {MinJwtSecretLength} characters. " +
                        "Application cannot start with a weak JWT secret in production.");

                Console.WriteLine($"[WARNING] Jwt:Secret is only {jwtSecret.Length} characters. Recommended minimum is {MinJwtSecretLength}.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();
        }

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                context.HttpContext.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
                return ValueTask.CompletedTask;
            };

            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("global", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 10;
            });
        });

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (isProduction)
        {
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary configuration is missing. Set Cloudinary__CloudName, Cloudinary__ApiKey, and Cloudinary__ApiSecret " +
                    "environment variables. Application cannot start without Cloudinary in production.");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                Console.WriteLine("[WARNING] Cloudinary is not configured. Using local file storage. " +
                    "This is acceptable only in Development.");
            }
        }

        var qikinkClientId = configuration["Qikink:ClientId"];
        var qikinkSandboxMode = configuration["Qikink:SandboxMode"];
        var isQikinkSandbox = string.IsNullOrEmpty(qikinkSandboxMode)
            || string.Equals(qikinkSandboxMode, "true", StringComparison.OrdinalIgnoreCase);

        if (isProduction)
        {
            if (string.IsNullOrEmpty(qikinkClientId))
            {
                throw new InvalidOperationException(
                    "Qikink configuration is missing. Set Qikink__ClientId environment variable. " +
                    "Application cannot start without Qikink configuration in production.");
            }

            var qikinkSecret = isQikinkSandbox
                ? configuration["Qikink:SandboxSecret"]
                : configuration["Qikink:ClientSecret"];

            if (string.IsNullOrEmpty(qikinkSecret))
            {
                var secretName = isQikinkSandbox ? "Qikink__SandboxSecret" : "Qikink__ClientSecret";
                throw new InvalidOperationException(
                    $"Qikink secret is missing. Set {secretName} environment variable for " +
                    $"{(isQikinkSandbox ? "sandbox" : "production")} mode. " +
                    "Application cannot start without Qikink credentials in production.");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(qikinkClientId))
            {
                Console.WriteLine("[WARNING] Qikink is not configured. Fulfillment will not work. " +
                    "This is acceptable only in Development.");
            }
        }

        return services;
    }
}
