namespace EcoAlerta.Api.DTOs;

/// <summary>
/// DTO (Data Transfer Object) para transferência de dados de queimadas via API.
/// Separa a camada de modelo interno da camada de API, seguindo boas práticas de arquitetura.
/// </summary>
public class QueimadaDto
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public string Municipio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Intensidade { get; set; }
    public string? FonteSatelite { get; set; }
}

