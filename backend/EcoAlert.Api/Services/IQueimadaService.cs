using EcoAlerta.Api.DTOs;

namespace EcoAlerta.Api.Services;

public interface IQueimadaService
{

    Task<List<QueimadaDto>> ObterQueimadasAsync(DateTime? dataInicio, DateTime? dataFim, string? municipio);

    Task<List<EstatisticasMunicipiosDto>> ObterEstatisticasPorMunicipioAsync(DateTime? dataInicio, DateTime? dataFim);

    Task<ResumoEstatisticasDto> ObterResumoEstatisticasAsync(DateTime? dataInicio, DateTime? dataFim);
}

