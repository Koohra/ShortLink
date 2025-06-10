using ShortLink.Domain.ValueObject;

namespace ShortLink.Domain.Entities;

public sealed class Link
{
    public Guid Id { get; private set; }
    public string Url { get; private set; }
    public ShortCode ShortCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public int ClickCount { get; private set; }

    private Link()
    {
    }

    public static Link Create(string originalUrl, ShortCode shortCode, DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
            throw new ArgumentNullException(nameof(originalUrl));

        return new Link
        {
            Id = Guid.NewGuid(),
            Url = originalUrl,
            ShortCode = shortCode,
            CreatedAt = DateTime.Now,
            ExpiresAt = expiresAt,
            ClickCount = 0
        };
    }

    public void RegisterClick()
    {
        ClickCount++;
    }
    
    public bool IsExpired() => ExpiresAt.HasValue && DateTime.Now > ExpiresAt.Value;
}