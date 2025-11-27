using Microsoft.AspNetCore.Mvc;
using EcoAlerta.Api.Services;
using EcoAlerta.Api.DTOs;
using EcoAlerta.Api.Validation;

namespace EcoAlerta.Api.Controllers;

/// <summary>
/// Controller REST para endpoints de queimadas.
/// Expõe os Web Services da aplicação seguindo padrões REST.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QueimadasController : ControllerBase
{
    private readonly IQueimadaService _queimadaService;
    private readonly ILogger<QueimadasController> _logger;

    public QueimadasController(IQueimadaService queimadaService, ILogger<QueimadasController> logger)
    {
        _queimadaService = queimadaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém lista de focos de queimadas com filtros opcionais.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QueimadaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<QueimadaDto>>> GetQueimadas(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] string? municipio = null)
    {
        try
        {
            var dateError = DateRangeValidator.ValidateDateRange(
                dataInicio, dataFim,
                out var normalizedStart, out var normalizedEnd,
                validateCompleteRange: true);

            if (dateError != null) return dateError;

            var municipioError = MunicipioValidator.ValidateAndSanitize(municipio, out var sanitizedMunicipio);
            if (municipioError != null) return municipioError;

            var queimadas = await _queimadaService.ObterQueimadasAsync(
                normalizedStart, normalizedEnd, sanitizedMunicipio);

            return Ok(queimadas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter queimadas");
            throw;
        }
    }

    /// <summary>
    /// Obtém estatísticas de focos agrupados por município.
    /// </summary>
    [HttpGet("estatisticas/municipios")]
    [ProducesResponseType(typeof(List<EstatisticasMunicipiosDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<EstatisticasMunicipiosDto>>> GetEstatisticasPorMunicipio(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var dateError = DateRangeValidator.ValidateDateRange(
                dataInicio, dataFim,
                out var normalizedStart, out var normalizedEnd,
                validateCompleteRange: false);

            if (dateError != null) return dateError;

            var estatisticas = await _queimadaService.ObterEstatisticasPorMunicipioAsync(
                normalizedStart, normalizedEnd);

            return Ok(estatisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas por município");
            throw;
        }
    }

    /// <summary>
    /// Obtém resumo geral das estatísticas de queimadas.
    /// </summary>
    [HttpGet("estatisticas/resumo")]
    [ProducesResponseType(typeof(ResumoEstatisticasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResumoEstatisticasDto>> GetResumoEstatisticas(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var dateError = DateRangeValidator.ValidateDateRange(
                dataInicio, dataFim,
                out var normalizedStart, out var normalizedEnd,
                validateCompleteRange: false);

            if (dateError != null) return dateError;

            var resumo = await _queimadaService.ObterResumoEstatisticasAsync(
                normalizedStart, normalizedEnd);

            return Ok(resumo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter resumo de estatísticas");
            throw;
        }
    }
}

