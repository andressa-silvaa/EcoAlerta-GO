using Microsoft.AspNetCore.Mvc;
using EcoAlerta.Api.Services;
using EcoAlerta.Api.DTOs;
using System.Text.RegularExpressions;

namespace EcoAlerta.Api.Controllers;

/// <summary>
/// Controller REST para endpoints de queimadas.
/// 
/// Este controller expõe os Web Services da aplicação, seguindo padrões REST:
/// - GET para consultas
/// - Uso de query parameters para filtros
/// - Retorno de DTOs (não modelos internos)
/// - Tratamento de erros HTTP adequado
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QueimadasController : ControllerBase
{
    private const int MaxMunicipioLength = 200;
    private const int MaxAnosRetroativos = 5;
    private static readonly Regex MunicipioRegex = new(@"[^a-zA-ZáàâãéèêíìîóòôõúùûçÁÀÂÃÉÈÊÍÌÎÓÒÔÕÚÙÛÇ\s\-'\.]", RegexOptions.Compiled);

    private readonly IQueimadaService _queimadaService;
    private readonly ILogger<QueimadasController> _logger;

    public QueimadasController(IQueimadaService queimadaService, ILogger<QueimadasController> logger)
    {
        _queimadaService = queimadaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém lista de focos de queimadas com filtros opcionais.
    /// 
    /// Endpoint principal para consulta de dados de queimadas.
    /// Permite filtros por intervalo de datas e município.
    /// </summary>
    /// <param name="dataInicio">Data inicial do período (opcional)</param>
    /// <param name="dataFim">Data final do período (opcional)</param>
    /// <param name="municipio">Nome do município para filtrar (opcional)</param>
    /// <returns>Lista de queimadas filtradas</returns>
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
            var periodoErro = ValidarPeriodo(dataInicio, dataFim, out var inicioNormalizado, out var fimNormalizado, validarJanelaCompleta: true);
            if (periodoErro is not null)
            {
                return periodoErro;
            }

            var municipioErro = TrySanitizarMunicipio(municipio, out var municipioNormalizado);
            if (municipioErro is not null)
            {
                return municipioErro;
            }

            var queimadas = await _queimadaService.ObterQueimadasAsync(inicioNormalizado, fimNormalizado, municipioNormalizado);

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
    /// 
    /// Útil para análise geográfica e identificação de áreas mais afetadas.
    /// </summary>
    /// <param name="dataInicio">Data inicial do período (opcional)</param>
    /// <param name="dataFim">Data final do período (opcional)</param>
    /// <returns>Lista de estatísticas por município</returns>
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
            var periodoErro = ValidarPeriodo(dataInicio, dataFim, out var inicioNormalizado, out var fimNormalizado, validarJanelaCompleta: false);
            if (periodoErro is not null)
            {
                return periodoErro;
            }

            var estatisticas = await _queimadaService.ObterEstatisticasPorMunicipioAsync(inicioNormalizado, fimNormalizado);
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
    /// 
    /// Fornece métricas consolidadas para dashboards e relatórios.
    /// </summary>
    /// <param name="dataInicio">Data inicial do período (opcional)</param>
    /// <param name="dataFim">Data final do período (opcional)</param>
    /// <returns>Resumo das estatísticas</returns>
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
            var periodoErro = ValidarPeriodo(dataInicio, dataFim, out var inicioNormalizado, out var fimNormalizado, validarJanelaCompleta: false);
            if (periodoErro is not null)
            {
                return periodoErro;
            }

            var resumo = await _queimadaService.ObterResumoEstatisticasAsync(inicioNormalizado, fimNormalizado);
            return Ok(resumo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter resumo de estatísticas");
            throw;
        }
    }

    private ActionResult? ValidarPeriodo(
        DateTime? dataInicio,
        DateTime? dataFim,
        out DateTime? inicioNormalizado,
        out DateTime? fimNormalizado,
        bool validarJanelaCompleta)
    {
        inicioNormalizado = dataInicio?.Date;
        fimNormalizado = dataFim?.Date;

        var erroBasico = ValidarOrdemDatas(inicioNormalizado, fimNormalizado);
        if (erroBasico is not null)
        {
            return erroBasico;
        }

        if (!validarJanelaCompleta)
        {
            return null;
        }

        var hoje = DateTime.UtcNow.Date;
        var limite = hoje.AddYears(-MaxAnosRetroativos);

        if (inicioNormalizado.HasValue && inicioNormalizado < limite)
        {
            return BadRequest(new { message = $"Data de início não pode ser anterior a {limite:dd/MM/yyyy}" });
        }

        if (inicioNormalizado.HasValue && inicioNormalizado > hoje)
        {
            return BadRequest(new { message = "Data de início não pode ser futura" });
        }

        if (fimNormalizado.HasValue && fimNormalizado > hoje)
        {
            return BadRequest(new { message = "Data de fim não pode ser futura" });
        }

        return null;
    }

    private ActionResult? ValidarOrdemDatas(DateTime? dataInicio, DateTime? dataFim)
    {
        if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
        {
            return BadRequest(new { message = "Data de início deve ser anterior à data de fim" });
        }

        return null;
    }

    private ActionResult? TrySanitizarMunicipio(string? municipio, out string? municipioSanitizado)
    {
        municipioSanitizado = null;
        if (string.IsNullOrWhiteSpace(municipio))
        {
            return null;
        }

        var trimmed = municipio.Trim();
        if (trimmed.Length > MaxMunicipioLength)
        {
            return BadRequest(new { message = "Nome do município não pode ter mais de 200 caracteres" });
        }

        var normalizado = MunicipioRegex.Replace(trimmed, string.Empty);
        if (string.IsNullOrWhiteSpace(normalizado))
        {
            return BadRequest(new { message = "Nome do município contém caracteres inválidos" });
        }

        municipioSanitizado = normalizado;
        return null;
    }
}

