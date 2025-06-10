using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShortLink.Application.Commands.CreateLink;
using ShortLink.Application.Commands.Queries.GetLinkByCode;
using ShortLink.Application.Commands.Queries.GetRecentLinks;
using ShortLink.Application.Commands.DeleteLink;
using ShortLink.Application.DTOs;

namespace ShortLink.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LinksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LinksController> _logger;

    public LinksController(IMediator mediator, ILogger<LinksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo link encurtado
    /// </summary>
    /// <param name="command">Dados do link a ser encurtado</param>
    /// <returns>Link encurtado criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateLinkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateLinkResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLink([FromBody] CreateLinkCommand command)
    {
        try
        {
            _logger.LogInformation("Criando link encurtado para URL: {OriginalUrl}", command.OriginalUrl);

            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao criar link: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result);
            }

            _logger.LogInformation("Link criado com sucesso: {ShortCode}", result.Link.ShortCode);

            return CreatedAtAction(
                nameof(GetLinkByCode),
                new { code = result.Link.ShortCode },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao criar link");
            return StatusCode(500, new CreateLinkResponse
            {
                Success = false,
                ErrorMessage = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Busca um link pelo código curto
    /// </summary>
    /// <param name="code">Código curto do link</param>
    /// <returns>Detalhes do link</returns>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(LinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLinkByCode(string code)
    {
        try
        {
            var query = new GetLinkByCodeQuery { ShortCode = code };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogInformation("Link não encontrado: {ShortCode}", code);
                return NotFound(new { message = "Link não encontrado" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar link {ShortCode}", code);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista os links mais recentes
    /// </summary>
    /// <param name="count">Quantidade de links a retornar (padrão: 10, máximo: 50)</param>
    /// <returns>Lista de links recentes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LinkDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentLinks([FromQuery] int count = 10)
    {
        try
        {
            // Validação do parâmetro
            if (count <= 0) count = 10;
            if (count > 50) count = 50;

            var query = new GetRecentLinksQuery { Count = count };
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retornando {Count} links recentes", result.Count());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar links recentes");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui um link pelo código curto
    /// </summary>
    /// <param name="code">Código curto do link</param>
    /// <returns>Resultado da exclusão</returns>
    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLink(string code)
    {
        try
        {
            // Primeiro verificar se o link existe
            var query = new GetLinkByCodeQuery { ShortCode = code };
            var existingLink = await _mediator.Send(query);

            if (existingLink == null)
            {
                return NotFound(new { message = "Link não encontrado" });
            }

            // Implementação do DeleteLinkCommand
            var deleteCommand = new DeleteLinkCommand { ShortCode = code };
            var result = await _mediator.Send(deleteCommand);

            if (!result.Success)
            {
                _logger.LogWarning("Falha ao deletar link: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result);
            }

            _logger.LogInformation("Link deletado com sucesso: {ShortCode}", code);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir link {ShortCode}", code);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}