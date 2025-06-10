namespace ShortLink.Application.DTOs;

public sealed class LinkDto
{
    public Guid Id { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long ClickCount { get; set; }
    public string ShortUrl { get; set; }
    
}