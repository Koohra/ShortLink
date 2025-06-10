namespace ShortLink.Infrastructure.Cache;

public sealed class RedisOptions
{
    public string KeyPrefix { get; set; } = "shortlink";
    public int DefaultExpiryHours { get; set; } = 24;
    public int StatsExpiryDays { get; set; } = 30;
}