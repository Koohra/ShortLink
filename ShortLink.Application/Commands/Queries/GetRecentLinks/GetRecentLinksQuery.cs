using MediatR;
using ShortLink.Application.DTOs;

namespace ShortLink.Application.Commands.Queries.GetRecentLinks;

public sealed class GetRecentLinksQuery : IRequest<IEnumerable<LinkDto>>
{
    public int Count { get; set; } = 10;
}