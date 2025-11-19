using Microsoft.AspNetCore.Mvc;
using EcoAlerta.Api.Services;
using EcoAlerta.Api.DTOs;

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
            var hoje = DateTime.UtcNow.Date;
            DateTime? inicioNormalizado = dataInicio?.Date;
            DateTime? fimNormalizado = dataFim?.Date;

            if (inicioNormalizado.HasValue && fimNormalizado.HasValue && inicioNormalizado > fimNormalizado)
            {
                return BadRequest(new { message = "Data de início deve ser anterior à data de fim" });
            }

            var limiteAnos = 5;
            var dataLimite = hoje.AddYears(-limiteAnos);
            if (inicioNormalizado.HasValue && inicioNormalizado < dataLimite)
            {
                return BadRequest(new { message = $"Data de início não pode ser anterior a {dataLimite:dd/MM/yyyy}" });
            }

            if (inicioNormalizado.HasValue && inicioNormalizado > hoje)
            {
                return BadRequest(new { message = "Data de início não pode ser futura" });
            }
            if (fimNormalizado.HasValue && fimNormalizado > hoje)
            {
                return BadRequest(new { message = "Data de fim não pode ser futura" });
            }

            // Validação: sanitização do nome do município (remover caracteres especiais perigosos)
            if (!string.IsNullOrWhiteSpace(municipio))
            {
                municipio = municipio.Trim();
                if (municipio.Length > 200)
                {
                    return BadRequest(new { message = "Nome do município não pode ter mais de 200 caracteres" });
                }
                
                // Validação de segurança: remover caracteres potencialmente perigosos
                // Permitir apenas letras, números, espaços e alguns caracteres especiais comuns em nomes
                var municipioSanitizado = System.Text.RegularExpressions.Regex.Replace(
                    municipio, 
                    @"[^a-zA-ZáàâãéèêíìîóòôõúùûçÁÀÂÃÉÈÊÍÌÎÓÒÔÕÚÙÛÇ\s\-'\.]", 
                    string.Empty);
                
                if (string.IsNullOrWhiteSpace(municipioSanitizado))
                {
                    return BadRequest(new { message = "Nome do município contém caracteres inválidos" });
                }
                
                municipio = municipioSanitizado;
            }

            var queimadas = await _queimadaService.ObterQueimadasAsync(inicioNormalizado, fimNormalizado, municipio);

            return Ok(queimadas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter queimadas");
            // O middleware de exceções vai capturar e tratar
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
            // Validação de parâmetros de data
            if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
            {
                return BadRequest(new { message = "Data de início deve ser anterior à data de fim" });
            }

            var estatisticas = await _queimadaService.ObterEstatisticasPorMunicipioAsync(dataInicio, dataFim);
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
            // Validação de parâmetros de data
            if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
            {
                return BadRequest(new { message = "Data de início deve ser anterior à data de fim" });
            }

            var resumo = await _queimadaService.ObterResumoEstatisticasAsync(dataInicio, dataFim);
            return Ok(resumo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter resumo de estatísticas");
            throw;
        }
    }
}

