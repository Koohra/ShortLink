using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShortLink.Domain.Interfaces;
using StackExchange.Redis;

namespace ShortLink.Infrastructure.Cache;

public sealed class RedisCacheService : ILinkCache
{
    private readonly RedisOptions _options;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;


    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, 
        IOptions<RedisOptions> options, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        _database = connectionMultiplexer.GetDatabase();
        _options = options.Value;
    }

    public async Task<string> GetOriginalUrlAsync(string shortCode)
    {
        var key = $"{_options.KeyPrefix}:url{shortCode}";
        return await _database.StringGetAsync(key);
    }

    public async Task CacheOriginalUrlAsync(string shortCode, string originalUrl, TimeSpan? expiry = null)
    {
        var key = $"{_options.KeyPrefix}:url:{shortCode}";

        var cacheExpiry = expiry ?? TimeSpan.FromDays(1);

        await _database.StringSetAsync(key, originalUrl, cacheExpiry);
    }

    public async Task<long> IncrementClickCountAsync(string shortCode)
    {
        var key = $"{_options.KeyPrefix}:clicks:{shortCode}";

        var newCount = await _database.StringIncrementAsync(key);

        if (newCount == 1)
        {
            await _database.KeyExpireAsync(key, TimeSpan.FromDays(_options.StatsExpiryDays));
        }

        _logger.LogDebug("Click incrementado: {ShortCode} -> {Count}", shortCode, newCount);
        return newCount;
    }

    public async Task<long> GetClickCountAsync(string shortCode)
    {
        var key = $"{_options.KeyPrefix}:clicks:{shortCode}";
        var count = await _database.StringGetAsync(key);
        return (long)(count.HasValue ? count : 0);
    }

    public async Task SetClickCountAsync(string shortCode, long count)
    {
        var key = $"{_options.KeyPrefix}:clicks:{shortCode}";
        await _database.StringSetAsync(key, count, TimeSpan.FromDays(_options.StatsExpiryDays));
    }

    public async Task<Dictionary<string, long>> GetAllClickCountsAsync()
    {
        try
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var pattern = $"{_options.KeyPrefix}:clicks:";
            var keys = server.Keys(database: _database.Database, pattern: pattern);

            var result = new Dictionary<string, long>();
            foreach (var key in keys)
            {
                var shortCode = key.ToString().Split(':').Last();
                var count = await _database.StringGetAsync(key);

                if (count.HasValue)
                {
                    result[shortCode] = (long)count;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contadores do Redis");
            return new Dictionary<string, long>();
        }
    }

    public async Task ResetClickCountAsync(string shortCode)
    {
        var key = $"{_options.KeyPrefix}:clicks:{shortCode}";
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveCacheAsync(string shortCode)
    {
        var urlKey = $"{_options.KeyPrefix}:url:{shortCode}";
        var clicksKey = $"{_options.KeyPrefix}:clicks:{shortCode}";

        await _database.KeyDeleteAsync([urlKey, clicksKey]);
    }
}