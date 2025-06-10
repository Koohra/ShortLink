using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShortLink.Application.Commands.Queries.GetLinkByCode;
using ShortLink.Domain.Interfaces;

namespace ShortLink.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StatsController : ControllerBase
{
    private readonly ILogger<StatsController> _logger;
    private readonly ILinkCache _cache;
    private readonly IMediator _mediator;

    public StatsController(ILogger<StatsController> logger, ILinkCache cache, IMediator mediator)
    {
        _logger = logger;
        _cache = cache;
        _mediator = mediator;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetLinkStats(string code)
    {
        try
        {
            var realTimeClicks = await _cache.GetClickCountAsync(code);

            var query = new GetLinkByCodeQuery { ShortCode = code };
            var link = await _mediator.Send(query);

            if (link is null)
            {
                return NotFound(new { Message = "Link not found" });
            }

            var stats = new
            {
                shortCode = link.ShortCode,
                Url = link.OriginalUrl,
                clickCount = Math.Max(link.ClickCount, realTimeClicks),
                databaseClickCount = link.ClickCount,
                realtimeClickCount = realTimeClicks,
                createdAt = link.CreatedAt,
                expiresAt = link.ExpiresAt,
                isExpired = link.ExpiresAt.HasValue && link.ExpiresAt.Value < DateTime.UtcNow,
                daysActive = (DateTime.UtcNow - link.CreatedAt).Days
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas: {ShortCode}", code);
            return StatusCode(500, new { message = "Erro interno" });
        }
    }
}