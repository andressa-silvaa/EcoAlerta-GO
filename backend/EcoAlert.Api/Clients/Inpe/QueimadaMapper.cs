using EcoAlerta.Api.Models;
using EcoAlerta.Api.Configuration;

namespace EcoAlerta.Api.Clients.Inpe;

internal static class QueimadaMapper
{
    public static Queimada? MapToQueimada(WfsFeatureProperties properties, int id, InpeApiOptions options)
    {
        if (properties.Latitude == null || properties.Longitude == null)
        {
            return null;
        }

        var dateTime = properties.DataHoraGmt ?? properties.DataPas ?? DateTime.UtcNow;

        return new Queimada
        {
            Id = id,
            DataHora = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            Municipio = string.IsNullOrWhiteSpace(properties.Municipio) ? "Não informado" : properties.Municipio,
            Estado = options.DefaultEstado,
            Latitude = (decimal)properties.Latitude.Value,
            Longitude = (decimal)properties.Longitude.Value,
            Intensidade = properties.Frp.HasValue ? (decimal)properties.Frp.Value : null,
            FonteSatelite = properties.Satelite,
            DataCriacao = DateTime.UtcNow
        };
    }
}

