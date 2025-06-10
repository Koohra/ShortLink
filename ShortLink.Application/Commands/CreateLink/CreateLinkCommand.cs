using MediatR;

namespace ShortLink.Application.Commands.CreateLink;

public record CreateLinkCommand : IRequest<CreateLinkResponse>
{
    public string OriginalUrl { get; set; }
    public string? CustomCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
}