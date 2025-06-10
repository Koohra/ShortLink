using MediatR;

namespace ShortLink.Application.Commands.Queries.RedirectLink;

public sealed class RedirectLinkCommand : IRequest<RedirectLinkResponse>
{
    public string ShortCode { get; set; }
}