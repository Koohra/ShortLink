using MediatR;
using Microsoft.Extensions.Options;
using ShortLink.Application.Common;
using ShortLink.Application.DTOs;
using ShortLink.Domain.Entities;
using ShortLink.Domain.Interfaces;
using ShortLink.Domain.ValueObject;

namespace ShortLink.Application.Commands.CreateLink;

public sealed class CreateLinkHandler : IRequestHandler<CreateLinkCommand, CreateLinkResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShortCodeGenerator _codeGenerator;
    private readonly ILinkCache _cache;
    private readonly AppSettings _settings;

    public CreateLinkHandler(IUnitOfWork unitOfWork, IShortCodeGenerator codeGenerator, ILinkCache cache,
        IOptions<AppSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _codeGenerator = codeGenerator;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<CreateLinkResponse> Handle(CreateLinkCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.OriginalUrl))
        {
            return new CreateLinkResponse
            {
                Success = false,
                ErrorMessage = "Original url is required"
            };
        }

        try
        {
            if (!Uri.TryCreate(command.OriginalUrl, UriKind.Absolute, out _))
                return new CreateLinkResponse
                {
                    Success = false,
                    ErrorMessage = "Original url is invalid"
                };

            ShortCode shortCode;

            if (!string.IsNullOrWhiteSpace(command.CustomCode))
            {
                try
                {
                    shortCode = ShortCode.Create(command.CustomCode);

                    if (await _unitOfWork.LinkRepository.ShortCodeExistsAsync(shortCode))
                        return new CreateLinkResponse
                        {
                            Success = false,
                            ErrorMessage = "A link with the same code already exists"
                        };
                }
                catch (Exception e)
                {
                    return new CreateLinkResponse
                    {
                        Success = false,
                        ErrorMessage = $"Invalid code: {e.Message}"
                    };
                }
            }
            else
            {
                shortCode = await _codeGenerator.GenerateShortCodeAsync();
            }

            var link = Link.Create(command.OriginalUrl, shortCode, command.ExpiresAt);

            await _unitOfWork.LinkRepository.AddLink(link);
            await _unitOfWork.CommitAsync();

            await _cache.CacheOriginalUrlAsync(shortCode.Value, command.OriginalUrl);

            return new CreateLinkResponse
            {
                Success = true,
                Link = await MapToDto(link)
            };
        }
        catch (Exception ex)
        {
            return new CreateLinkResponse
            {
                Success = false,
                ErrorMessage = $"Erro ao criar link: {ex.Message}"
            };
        }
    }

    private async Task<LinkDto> MapToDto(Link link)
    {
        string baseUrl = _settings.BaseUrl;
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";

        return new LinkDto
        {
            Id = link.Id,
            OriginalUrl = link.Url,
            ShortCode = link.ShortCode.Value,
            CreatedAt = link.CreatedAt,
            ExpiresAt = link.ExpiresAt,
            ClickCount = await _cache.GetClickCountAsync(link.ShortCode.Value),
            ShortUrl = $"{baseUrl}{link.ShortCode.Value}"
        };
    }
}