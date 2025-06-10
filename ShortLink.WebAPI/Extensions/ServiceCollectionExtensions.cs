using ShortLink.Infrastructure.Context;

namespace ShortLink.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShortLinkServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddOpenApi();

        services.AddDatabase(configuration);
        services.AddRedisCache(configuration);
        services.AddInfrastructure();
        services.AddApplication(configuration);
        services.AddHealthChecks(configuration);
        services.AddCorsPolicy();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(tags: ["database"])
            .AddRedis(
                configuration.GetConnectionString("Redis") ?? "localhost:6379",
                tags: ["cache"]);

        return services;
    }

    private static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}