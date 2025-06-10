using ShortLink.Domain.Entities;
using ShortLink.Domain.ValueObject;

namespace ShortLink.Domain.Interfaces;

public interface ILinkRepository
{
    Task<Link> GetByShortCodeAsync(ShortCode shortCode);
    Task<bool> ShortCodeExistsAsync(ShortCode shortCode);
    Task<Link> AddLink(Link link);
    Task<IEnumerable<Link>> GetRecentLinksAsync(int count);
}