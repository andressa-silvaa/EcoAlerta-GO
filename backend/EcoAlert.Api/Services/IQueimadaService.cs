using EcoAlerta.Api.DTOs;

namespace EcoAlerta.Api.Services;

/// <summary>
/// Interface do serviço de queimadas.
/// Define os contratos de negócio para processamento de dados de queimadas.
/// </summary>
public interface IQueimadaService
{
    /// <summary>
    /// Obtém lista de queimadas com filtros opcionais.
    /// Aplica regras de negócio: filtro por estado (Goiás), datas e município.
    /// </summary>
    Task<List<QueimadaDto>> ObterQueimadasAsync(DateTime? dataInicio, DateTime? dataFim, string? municipio);

    /// <summary>
    /// Obtém estatísticas de focos agrupados por município.
    /// </summary>
    Task<List<EstatisticasMunicipiosDto>> ObterEstatisticasPorMunicipioAsync(DateTime? dataInicio, DateTime? dataFim);

    /// <summary>
    /// Obtém resumo geral das estatísticas de queimadas.
    /// </summary>
    Task<ResumoEstatisticasDto> ObterResumoEstatisticasAsync(DateTime? dataInicio, DateTime? dataFim);
}

