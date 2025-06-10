using MediatR;
using Microsoft.Extensions.Options;
using ShortLink.Application.Common;
using ShortLink.Application.DTOs;
using ShortLink.Domain.Entities;
using ShortLink.Domain.Interfaces;
using ShortLink.Domain.ValueObject;

namespace ShortLink.Application.Commands.Queries.GetLinkByCode;

public sealed class GetLinkCodeHandler : IRequestHandler<GetLinkByCodeQuery, LinkDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppSettings _settings;
    private readonly ILinkCache _cache;

    public GetLinkCodeHandler(IUnitOfWork unitOfWork, IOptions<AppSettings> settings, ILinkCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _settings = settings.Value;
    }
    
    public async Task<LinkDto> Handle(GetLinkByCodeQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var shortcode = ShortCode.Create(query.ShortCode);
            var link = await _unitOfWork.LinkRepository.GetByShortCodeAsync(shortcode);

            return await (link is null ? null : MapToDto(link));
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private async Task<LinkDto> MapToDto(Link link)
    {
        var baseUrl = _settings.BaseUrl;
        if (!baseUrl.EndsWith($"/"))
            baseUrl += "/";

        return new LinkDto
        {
            Id = link.Id,
            OriginalUrl = link.Url,
            ShortCode = link.ShortCode.Value,
            CreatedAt = link.CreatedAt,
            ExpiresAt = link.ExpiresAt,
            ClickCount =  await _cache.GetClickCountAsync(link.ShortCode.Value),
            ShortUrl = $"{baseUrl}{link.ShortCode.Value}"
        };
    }
}