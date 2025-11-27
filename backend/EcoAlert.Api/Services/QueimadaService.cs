using EcoAlerta.Api.Clients;
using EcoAlerta.Api.DTOs;
using EcoAlerta.Api.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace EcoAlerta.Api.Services;

public class QueimadaService : IQueimadaService
{
    private static readonly CompareInfo MunicipioComparer = CultureInfo.GetCultureInfo("pt-BR").CompareInfo;

    private readonly IInpeApiClient _inpeApiClient;
    private readonly ILogger<QueimadaService> _logger;

    public QueimadaService(IInpeApiClient inpeApiClient, ILogger<QueimadaService> logger)
    {
        _inpeApiClient = inpeApiClient;
        _logger = logger;
    }

    public async Task<List<QueimadaDto>> ObterQueimadasAsync(DateTime? dataInicio, DateTime? dataFim, string? municipio)
    {
        _logger.LogInformation(
            "Consultando queimadas. Início: {Inicio}, Fim: {Fim}, Município: {Municipio}",
            dataInicio,
            dataFim,
            municipio);

        var queimadas = await _inpeApiClient.ObterFocosQueimadasAsync(dataInicio, dataFim);
        var filtradas = FiltrarPorMunicipio(queimadas, municipio);

        _logger.LogInformation("Retornando {Quantidade} focos após filtros", filtradas.Count);
        return MapearParaDto(filtradas);
    }

    public async Task<List<EstatisticasMunicipiosDto>> ObterEstatisticasPorMunicipioAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        _logger.LogInformation("Calculando estatísticas por município. Início: {Inicio}, Fim: {Fim}", dataInicio, dataFim);

        var queimadas = await _inpeApiClient.ObterFocosQueimadasAsync(dataInicio, dataFim);

        return queimadas
            .GroupBy(q => q.Municipio)
            .Select(g => new EstatisticasMunicipiosDto
            {
                Municipio = g.Key,
                TotalFocos = g.Count()
            })
            .OrderByDescending(e => e.TotalFocos)
            .ToList();
    }

    public async Task<ResumoEstatisticasDto> ObterResumoEstatisticasAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        _logger.LogInformation("Calculando resumo de estatísticas. Início: {Inicio}, Fim: {Fim}", dataInicio, dataFim);

        var queimadas = await _inpeApiClient.ObterFocosQueimadasAsync(dataInicio, dataFim);
        if (!queimadas.Any())
        {
            return new ResumoEstatisticasDto();
        }

        var totalFocos = queimadas.Count;
        var municipiosAfetados = queimadas.Select(q => q.Municipio).Distinct().Count();
        var focosPorDia = queimadas
            .GroupBy(q => q.DataHora.Date)
            .Select(g => new { Data = g.Key, Total = g.Count() })
            .OrderByDescending(x => x.Total)
            .ToList();

        var diaComMaisFocos = focosPorDia.FirstOrDefault();
        var diasNoPeriodo = CalcularNumeroDeDias(dataInicio, dataFim, queimadas);
        var media = diasNoPeriodo > 0 ? Math.Round(totalFocos / (double)diasNoPeriodo, 2) : 0;

        return new ResumoEstatisticasDto
        {
            TotalFocos = totalFocos,
            TotalMunicipiosAfetados = municipiosAfetados,
            DataComMaisFocos = diaComMaisFocos?.Data,
            FocosNaDataMaxima = diaComMaisFocos?.Total ?? 0,
            MediaFocosPorDia = media
        };
    }

    private static List<Queimada> FiltrarPorMunicipio(List<Queimada> dados, string? municipio)
    {
        if (string.IsNullOrWhiteSpace(municipio))
        {
            return dados;
        }

        return dados
            .Where(q => MunicipioComparer.Compare(
                q.Municipio,
                municipio,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
            .ToList();
    }

    private static List<QueimadaDto> MapearParaDto(IEnumerable<Queimada> queimadas)
        => queimadas
            .Select(q => new QueimadaDto
            {
                Id = q.Id,
                DataHora = q.DataHora,
                Municipio = q.Municipio,
                Estado = q.Estado,
                Latitude = q.Latitude,
                Longitude = q.Longitude,
                Intensidade = q.Intensidade,
                FonteSatelite = q.FonteSatelite
            })
            .ToList();

    private static int CalcularNumeroDeDias(DateTime? dataInicio, DateTime? dataFim, IReadOnlyCollection<Queimada> dados)
    {
        if (dataInicio.HasValue && dataFim.HasValue)
        {
            return Math.Max(1, (dataFim.Value.Date - dataInicio.Value.Date).Days + 1);
        }

        if (dados.Count == 0)
        {
            return 0;
        }

        var primeiraData = dados.Min(q => q.DataHora.Date);
        var ultimaData = dados.Max(q => q.DataHora.Date);
        return Math.Max(1, (ultimaData - primeiraData).Days + 1);
    }
}

