namespace ShortLink.Domain.Interfaces;

public interface IUnitOfWork
{
    ILinkRepository LinkRepository { get; }
    Task CommitAsync();
}