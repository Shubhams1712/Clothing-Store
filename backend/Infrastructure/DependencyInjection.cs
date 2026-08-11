using System.Threading.Channels;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

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
