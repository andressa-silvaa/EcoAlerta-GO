namespace EcoAlerta.Api.DTOs;

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

