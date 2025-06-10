using ShortLink.Domain.ValueObject;

namespace ShortLink.Domain.Interfaces;

public interface ILinkCache
{
    Task<string> GetOriginalUrlAsync(string shortCode);
    Task CacheOriginalUrlAsync(string shortCode, string originalUrl, TimeSpan? expiry = null);
    Task<long> IncrementClickCountAsync(string shortCode);
    Task<long> GetClickCountAsync(string shortCode);
    Task SetClickCountAsync(string shortCode, long count);
    Task<Dictionary<string, long>> GetAllClickCountsAsync();
    Task ResetClickCountAsync(string shortCode);
    Task RemoveCacheAsync(string shortCode);
}