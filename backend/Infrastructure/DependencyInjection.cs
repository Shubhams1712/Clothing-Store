using System.Threading.Channels;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Resend;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<Data.ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(Domain.Common.IRepository<>), typeof(Repositories.BaseRepository<>));
        services.AddScoped<IUnitOfWork, Repositories.UnitOfWork>();

        services.AddHttpClient<IQikinkClient, QikinkClient>();

        services.AddSingleton(Channel.CreateUnbounded<Guid>());
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, FulfillmentBackgroundService>();

        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.Configure<QikinkSettings>(configuration.GetSection("Qikink"));

        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        var envName = configuration["Environment"]
                   ?? configuration["ASPNETCORE_ENVIRONMENT"]
                   ?? "Production";
        var isDevelopment = string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(envName, "Local", StringComparison.OrdinalIgnoreCase);

        if (isDevelopment)
        {
            services.AddScoped<IImageStorageService, LocalImageStorageService>();
        }
        else
        {
            services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
        }

        var resendApiKey = configuration["Email:ResendApiKey"]
                       ?? Environment.GetEnvironmentVariable("RESEND_API_KEY");

        if (!string.IsNullOrEmpty(resendApiKey))
        {
            services.Configure<EmailSettings>(settings =>
            {
                settings.ResendApiKey = resendApiKey;

                var fromEmail = configuration["Email:FromEmail"]
                    ?? Environment.GetEnvironmentVariable("EMAIL_FROM");
                if (!string.IsNullOrEmpty(fromEmail))
                    settings.FromEmail = fromEmail;

                var frontendUrl = configuration["Email:FrontendUrl"]
                    ?? Environment.GetEnvironmentVariable("FRONTEND_URL");
                if (!string.IsNullOrEmpty(frontendUrl))
                    settings.FrontendUrl = frontendUrl;
            });

            services.AddResend(options =>
            {
                options.ApiToken = resendApiKey;
            });
            services.AddScoped<IEmailService, ResendEmailService>();
        }
        else if (!isDevelopment)
        {
            Console.WriteLine("[WARNING] Email configuration is missing. Set Email__ResendApiKey or RESEND_API_KEY, " +
                "Email__FromEmail or EMAIL_FROM, and Email__FrontendUrl or FRONTEND_URL environment variables. " +
                "Verification emails will not be sent.");
            services.AddScoped<IEmailService, DevEmailService>();
        }
        else
        {
            Console.WriteLine("[INFO] Email service not configured. Running in Development mode. " +
                "Email verification will be auto-approved.");
            services.AddScoped<IEmailService, DevEmailService>();
        }

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<HealthService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStorefrontService, StorefrontService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFulfillmentService, FulfillmentService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }
}
