using MediatR;
using ShortLink.Application.DTOs;

namespace ShortLink.Application.Commands.Queries.GetLinkByCode;

public sealed class GetLinkByCodeQuery : IRequest<LinkDto>
{
    public string ShortCode { get; set; }
}