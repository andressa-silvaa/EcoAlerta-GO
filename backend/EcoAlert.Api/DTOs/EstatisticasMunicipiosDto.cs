namespace EcoAlerta.Api.DTOs;

/// <summary>
/// DTO para retornar estatísticas de focos de queimadas agrupados por município.
/// Utilizado no endpoint de estatísticas para análise geográfica dos dados.
/// </summary>
public class EstatisticasMunicipiosDto
{
    public string Municipio { get; set; } = string.Empty;
    public int TotalFocos { get; set; }
}

