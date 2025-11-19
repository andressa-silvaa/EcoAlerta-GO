namespace EcoAlerta.Api.DTOs;

/// <summary>
/// DTO para retornar resumo geral das estatísticas de queimadas.
/// Fornece visão consolidada dos dados para o dashboard do frontend.
/// </summary>
public class ResumoEstatisticasDto
{
    /// <summary>
    /// Total de focos de queimadas no período consultado.
    /// </summary>
    public int TotalFocos { get; set; }

    /// <summary>
    /// Total de municípios afetados no período.
    /// </summary>
    public int TotalMunicipiosAfetados { get; set; }

    /// <summary>
    /// Data com maior número de focos detectados.
    /// </summary>
    public DateTime? DataComMaisFocos { get; set; }

    /// <summary>
    /// Quantidade de focos na data com mais ocorrências.
    /// </summary>
    public int FocosNaDataMaxima { get; set; }

    /// <summary>
    /// Média de focos por dia no período.
    /// </summary>
    public double MediaFocosPorDia { get; set; }
}

