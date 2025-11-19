using EcoAlerta.Api.Clients;
using EcoAlerta.Api.DTOs;
using EcoAlerta.Api.Models;
using Microsoft.Extensions.Logging;

namespace EcoAlerta.Api.Services;

/// <summary>
/// Serviço de negócio para processamento de dados de queimadas.
/// 
/// Responsabilidades:
/// - Aplicar regras de negócio (filtros, validações)
/// - Orquestrar chamadas ao cliente da API do INPE
/// - Processar e agregar dados para estatísticas
/// - Converter modelos internos em DTOs para a API
/// </summary>
public class QueimadaService : IQueimadaService
{
    private readonly IInpeApiClient _inpeApiClient;
    private readonly ILogger<QueimadaService> _logger;

    public QueimadaService(IInpeApiClient inpeApiClient, ILogger<QueimadaService> logger)
    {
        _inpeApiClient = inpeApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Obtém lista de queimadas aplicando filtros de negócio.
    /// 
    /// Regras de negócio aplicadas:
    /// 1. Filtro obrigatório: apenas estado de Goiás (já aplicado no cliente INPE)
    /// 2. Filtro opcional por intervalo de datas
    /// 3. Filtro opcional por município
    /// </summary>
    public async Task<List<QueimadaDto>> ObterQueimadasAsync(DateTime? dataInicio, DateTime? dataFim, string? municipio)
    {
        _logger.LogInformation($"Consultando queimadas - DataInicio: {dataInicio}, DataFim: {dataFim}, Municipio: {municipio}");

        // Obtém dados da API do INPE (ou mock)
        var queimadas = await _inpeApiClient.ObterFocosQueimadasAsync(dataInicio, dataFim);

        // Aplica filtro por município se fornecido
        if (!string.IsNullOrWhiteSpace(municipio))
        {
            queimadas = queimadas
                .Where(q => q.Municipio.Contains(municipio, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Converte modelos internos para DTOs (separação de camadas)
        var dtos = queimadas.Select(q => new QueimadaDto
        {
            Id = q.Id,
            DataHora = q.DataHora,
            Municipio = q.Municipio,
            Estado = q.Estado,
            Latitude = q.Latitude,
            Longitude = q.Longitude,
            Intensidade = q.Intensidade,
            FonteSatelite = q.FonteSatelite
        }).ToList();

        _logger.LogInformation($"Retornando {dtos.Count} queimadas após aplicação de filtros");

        return dtos;
    }

    /// <summary>
    /// Calcula estatísticas de focos agrupados por município.
    /// 
    /// Esta agregação permite análise geográfica dos dados,
    /// identificando quais municípios têm maior incidência de queimadas.
    /// </summary>
    public async Task<List<EstatisticasMunicipiosDto>> ObterEstatisticasPorMunicipioAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        _logger.LogInformation("Calculando estatísticas por município");

        var queimadas = await _inpeApiClient.ObterFocosQueimadasAsync(dataInicio, dataFim);

        var estatisticas = queimadas
            .GroupBy(q => q.Municipio)
            .Select(g => new EstatisticasMunicipiosDto
            {
                Municipio = g.Key,
                TotalFocos = g.Count()
            })
            .OrderByDescending(e => e.TotalFocos)
            .ToList();

        _logger.LogInformation($"Estatísticas calculadas para {estatisticas.Count} municípios");

        return estatisticas;
    }

    /// <summary>
    /// Calcula resumo geral das estatísticas de queimadas.
    /// 
    /// Fornece métricas consolidadas para o dashboard:
    /// - Total de focos
    /// - Total de municípios afetados
    /// - Data com mais focos
    /// - Média de focos por dia
    /// </summary>
    public async Task<ResumoEstatisticasDto> ObterResumoEstatisticasAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        _logger.LogInformation("Calculando resumo de estatísticas");

        var queimadas = await _inpeApiClient.ObterFocosQueimadasAsync(dataInicio, dataFim);

        if (!queimadas.Any())
        {
            return new ResumoEstatisticasDto
            {
                TotalFocos = 0,
                TotalMunicipiosAfetados = 0,
                MediaFocosPorDia = 0
            };
        }

        var totalFocos = queimadas.Count;
        var municipiosAfetados = queimadas.Select(q => q.Municipio).Distinct().Count();

        // Calcula dia com mais focos
        var focosPorDia = queimadas
            .GroupBy(q => q.DataHora.Date)
            .Select(g => new { Data = g.Key, Total = g.Count() })
            .OrderByDescending(x => x.Total)
            .FirstOrDefault();

        // Calcula média de focos por dia
        var diasNoPeriodo = dataInicio.HasValue && dataFim.HasValue
            ? (dataFim.Value - dataInicio.Value).Days + 1
            : 30; // Default para 30 dias se não especificado
        var mediaFocosPorDia = diasNoPeriodo > 0 ? (double)totalFocos / diasNoPeriodo : 0;

        var resumo = new ResumoEstatisticasDto
        {
            TotalFocos = totalFocos,
            TotalMunicipiosAfetados = municipiosAfetados,
            DataComMaisFocos = focosPorDia?.Data,
            FocosNaDataMaxima = focosPorDia?.Total ?? 0,
            MediaFocosPorDia = Math.Round(mediaFocosPorDia, 2)
        };

        _logger.LogInformation($"Resumo calculado: {totalFocos} focos em {municipiosAfetados} municípios");

        return resumo;
    }
}

