using ShortLink.Domain.Interfaces;
using ShortLink.Infrastructure.Context;

namespace ShortLink.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private ILinkRepository? _linkRepo;
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public ILinkRepository LinkRepository
    {
        get { return _linkRepo ??= new LinkRepository(_context); }
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}