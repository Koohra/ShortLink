using MediatR;
using Microsoft.Extensions.Logging;
using ShortLink.Domain.Interfaces;
using ShortLink.Domain.ValueObject;

namespace ShortLink.Application.Commands.Queries.RedirectLink;

public sealed class RedirectLinkHandler : IRequestHandler<RedirectLinkCommand, RedirectLinkResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILinkCache _cache;
    private readonly ILogger<RedirectLinkHandler> _logger;

    public RedirectLinkHandler(IUnitOfWork unitOfWork, ILinkCache cache, ILogger<RedirectLinkHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RedirectLinkResponse> Handle(RedirectLinkCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.ShortCode))
            return RedirectLinkResponse.Failed("Código inválido");

        try
        {
            _logger.LogInformation("Processando redirecionamento: {ShortCode}", command.ShortCode);
            
            var cachedUrl = await _cache.GetOriginalUrlAsync(command.ShortCode);

            if (!string.IsNullOrEmpty(cachedUrl))
            {
                _logger.LogInformation("URL encontrada no cache: {ShortCode}", command.ShortCode);
                
                var clickCount = await _cache.IncrementClickCountAsync(command.ShortCode);

                _logger.LogInformation("Click incrementado via cache: {ShortCode} → Click #{Count}",
                    command.ShortCode, clickCount);

                return RedirectLinkResponse.Successful(cachedUrl);
            }
            
            _logger.LogInformation("Buscando no banco: {ShortCode}", command.ShortCode);

            var shortCode = ShortCode.Create(command.ShortCode);
            var link = await _unitOfWork.LinkRepository.GetByShortCodeAsync(shortCode);

            if (link is null)
            {
                _logger.LogWarning("Link não encontrado: {ShortCode}", command.ShortCode);
                return RedirectLinkResponse.Failed("Link não encontrado");
            }

            if (link.IsExpired())
            {
                _logger.LogWarning("Link expirado: {ShortCode}", command.ShortCode);
                return RedirectLinkResponse.Failed("Link expirado");
            }
            
            await _cache.CacheOriginalUrlAsync(command.ShortCode, link.Url);
            
            var redisClickCount = await _cache.IncrementClickCountAsync(command.ShortCode);

            _logger.LogInformation(
                "Redirecionamento processado via banco: {ShortCode} → {OriginalUrl} (Click #{Count})",
                command.ShortCode, link.Url, redisClickCount);

            return RedirectLinkResponse.Successful(link.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar redirecionamento: {ShortCode}", command.ShortCode);
            return RedirectLinkResponse.Failed($"Erro interno: {ex.Message}");
        }
    }
}