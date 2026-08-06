namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                var frontendUrl = configuration["Frontend:Url"] ?? "http://localhost:3000";
                builder.WithOrigins(frontendUrl)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        return services;
    }
}
