using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var envPath = FindEnvFile();
        if (envPath is not null)
        {
            DotNetEnv.Env.Load(envPath);
        }

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string not found. Set the ConnectionStrings__DefaultConnection " +
                "environment variable or place a .env file in the API project directory.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string? FindEnvFile()
    {
        var dir = AppContext.BaseDirectory;

        for (var i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(dir);
            if (parent is null)
            {
                break;
            }

            dir = parent.FullName;
        }

        return null;
    }
}
