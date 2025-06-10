using Microsoft.EntityFrameworkCore;
using ShortLink.Domain.Entities;
using ShortLink.Domain.Interfaces;
using ShortLink.Domain.ValueObject;
using ShortLink.Infrastructure.Context;

namespace ShortLink.Infrastructure.Repositories;

public sealed class LinkRepository : ILinkRepository
{
    private readonly AppDbContext _context;

    public LinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Link> GetByShortCodeAsync(ShortCode shortCode)
    {
        var codeValue = shortCode.Value;
        
        return await _context.Links
            .FromSqlRaw("SELECT * FROM Links WHERE ShortCode = {0}", codeValue)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ShortCodeExistsAsync(ShortCode shortCode)
    {
        var codeValue = shortCode.Value;
        
        var result = await _context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM Links WHERE ShortCode = {0}", codeValue)
            .FirstOrDefaultAsync();
        return result > 0;
    }

    public async Task<Link> AddLink(Link link)
    {
        await _context.Links.AddAsync(link);
        return link;
    }

    public async Task<IEnumerable<Link>> GetRecentLinksAsync(int count)
    {
        return await _context.Links
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
};