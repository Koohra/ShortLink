using MediatR;
using Microsoft.Extensions.Options;
using ShortLink.Application.Common;
using ShortLink.Application.DTOs;
using ShortLink.Domain.Entities;
using ShortLink.Domain.Interfaces;

namespace ShortLink.Application.Commands.Queries.GetRecentLinks;

public sealed class GetRecentLinksHandler : IRequestHandler<GetRecentLinksQuery, IEnumerable<LinkDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppSettings _settings;
    private readonly ILinkCache _cache;

    public GetRecentLinksHandler(IUnitOfWork unitOfWork, IOptions<AppSettings> settings, ILinkCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<IEnumerable<LinkDto>> Handle(GetRecentLinksQuery query, CancellationToken cancellationToken = default)
    {
        var links =  await _unitOfWork.LinkRepository.GetRecentLinksAsync(query.Count);
        return await Task.WhenAll(links.Select(MapToDto));
    }

    private async Task<LinkDto> MapToDto(Link link)
    {
        var baseUrl = _settings.BaseUrl;
        if (!baseUrl.EndsWith($"/")) baseUrl += "/";

        return new LinkDto
        {
            Id = link.Id,
            OriginalUrl = link.Url,
            ExpiresAt = link.ExpiresAt,
            CreatedAt = link.CreatedAt,
            ShortCode = link.ShortCode.Value,
            ClickCount = await _cache.GetClickCountAsync(link.ShortCode.Value),
            ShortUrl = $"{baseUrl}{link.ShortCode.Value}"
        };
    }
}