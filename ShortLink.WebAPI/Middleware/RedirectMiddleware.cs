using ShortLink.Domain.Interfaces;
using ShortLink.Domain.ValueObject;

namespace ShortLink.WebAPI.Middleware;

public sealed class RedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RedirectMiddleware> _logger;

    public RedirectMiddleware(RequestDelegate next, ILogger<RedirectMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILinkCache cache, IUnitOfWork unitOfWork)
    {
        var path = context.Request.Path.Value;

        if (ShouldProcessRedirect(path))
        {
            var shortCode = ExtractShortCode(path);

            if (!string.IsNullOrWhiteSpace(shortCode))
            {
                try
                {
                    _logger.LogInformation("Redirecting to {ShortCode}.", shortCode);

                    var originalUrl = await cache.GetOriginalUrlAsync(shortCode);

                    if (!string.IsNullOrWhiteSpace(originalUrl))
                    {
                        var clickCount = await cache.GetClickCountAsync(shortCode);

                        _logger.LogInformation(
                            "Redirecionamento via cache: {ShortCode} → {OriginalUrl} (Click #{Count})",
                            shortCode, originalUrl, clickCount);

                        context.Response.Redirect(originalUrl, permanent: false);
                        return;
                    }

                    await HandleDatabaseFallback(shortCode, cache, context, unitOfWork);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar redirecionamento para {ShortCode}", shortCode);
                }
            }
        }

        // Se não foi redirecionamento ou falhou, continuar pipeline
        await _next(context);
    }

    private async Task HandleDatabaseFallback(string shortCode, ILinkCache cache, HttpContext context,
        IUnitOfWork unitOfWork)
    {
        try
        {
            _logger.LogInformation("Buscando no banco: {ShortCode}", shortCode);

            var code = ShortCode.Create(shortCode);
            var link = await unitOfWork.LinkRepository.GetByShortCodeAsync(code);

            if (link is null)
            {
                _logger.LogWarning("Link não encontrado: {ShortCode}", shortCode);
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Link não encontrado");
                return;
            }

            if (link.IsExpired())
            {
                _logger.LogWarning("Link expirado: {ShortCode}", shortCode);
                context.Response.StatusCode = 410;
                await context.Response.WriteAsync("Link expirado");
                return;
            }

            // Adicionar ao cache para próximos acessos
            await cache.CacheOriginalUrlAsync(shortCode, link.Url);

            // Incrementar contador no Redis
            var clickCount = await cache.IncrementClickCountAsync(shortCode);

            _logger.LogInformation("Redirecionamento via banco: {ShortCode} → {OriginalUrl} (Click #{Count})",
                shortCode, link.Url, clickCount);

            context.Response.Redirect(link.Url, permanent: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no fallback do banco: {ShortCode}", shortCode);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Erro interno");
        }
    }

    private static bool ShouldProcessRedirect(string? path)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= 1)
            return false;

        var ignoredPaths = new[]
        {
            "/api/",
            "/swagger",
            "/health",
            "/openapi",
            "/favicon.ico",
            "/.well-known/",
            "/robots.txt"
        };

        return !ignoredPaths.Any(ignored => path.StartsWith(ignored, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractShortCode(string path) => path.TrimStart('/').Split('/')[0];
}