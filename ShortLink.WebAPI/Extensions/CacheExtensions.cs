using ShortLink.Domain.Interfaces;
using ShortLink.Infrastructure.Cache;
using StackExchange.Redis;

namespace ShortLink.WebAPI.Extensions;

public static class CacheExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisConnection);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectRetry = 3;
            configurationOptions.ConnectTimeout = 5000;

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        // ✅ Registrar o cache service
        services.AddSingleton<ILinkCache, RedisCacheService>();

        return services;
    }
}