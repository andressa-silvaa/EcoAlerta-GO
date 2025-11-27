using System.Text.Json.Serialization;

namespace EcoAlerta.Api.Clients.Inpe;

internal sealed class WfsFeatureCollection
{
    [JsonPropertyName("features")]
    public List<WfsFeature> Features { get; set; } = new();
}

internal sealed class WfsFeature
{
    [JsonPropertyName("properties")]
    public WfsFeatureProperties? Properties { get; set; }
}

internal sealed class WfsFeatureProperties
{
    [JsonPropertyName("foco_id")]
    public string? FocoId { get; set; }

    [JsonPropertyName("datahora_gmt")]
    public DateTime? DataHoraGmt { get; set; }

    [JsonPropertyName("data_pas")]
    public DateTime? DataPas { get; set; }

    [JsonPropertyName("municipio")]
    public string? Municipio { get; set; }

    [JsonPropertyName("estado")]
    public string? Estado { get; set; }

    [JsonPropertyName("pais")]
    public string? Pais { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("frp")]
    public double? Frp { get; set; }

    [JsonPropertyName("satelite")]
    public string? Satelite { get; set; }
}

