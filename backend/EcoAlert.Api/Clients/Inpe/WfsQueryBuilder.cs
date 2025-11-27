using System.Globalization;
using System.Text;
using EcoAlerta.Api.Configuration;

namespace EcoAlerta.Api.Clients.Inpe;

internal static class WfsQueryBuilder
{
    public static string BuildRequestUri(
        string typeName,
        DateTime startDate,
        DateTime endDate,
        int pageSize,
        InpeApiOptions options)
    {
        var resource = options.Resource?.Trim('/') ?? "wfs";
        var filters = BuildFilters(startDate, endDate, options);
        var queryParams = BuildQueryParameters(typeName, pageSize, filters, options);

        return $"{resource}?{CreateQueryString(queryParams)}";
    }

    private static List<string> BuildFilters(
        DateTime startDate,
        DateTime endDate,
        InpeApiOptions options)
    {
        var filters = new List<string>
        {
            $"data_pas BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'"
        };

        if (!string.IsNullOrWhiteSpace(options.EstadoFiltro))
        {
            filters.Add($"estado = '{options.EstadoFiltro}'");
        }

        if (!string.IsNullOrWhiteSpace(options.DefaultPais))
        {
            filters.Add($"pais = '{options.DefaultPais}'");
        }

        return filters;
    }

    private static Dictionary<string, string?> BuildQueryParameters(
        string typeName,
        int pageSize,
        List<string> filters,
        InpeApiOptions options)
    {
        return new Dictionary<string, string?>
        {
            ["service"] = "WFS",
            ["version"] = "1.0.0",
            ["request"] = "GetFeature",
            ["typeName"] = typeName,
            ["outputFormat"] = string.IsNullOrWhiteSpace(options.OutputFormat)
                ? "application/json"
                : options.OutputFormat,
            ["srsName"] = "EPSG:4326",
            ["propertyName"] = "latitude,longitude,data_hora_gmt,data_pas,municipio,estado,pais,satelite,frp,foco_id",
            ["maxFeatures"] = pageSize.ToString(CultureInfo.InvariantCulture),
            ["CQL_FILTER"] = string.Join(" AND ", filters)
        };
    }

    private static string CreateQueryString(Dictionary<string, string?> parameters)
    {
        var builder = new StringBuilder();
        var parts = parameters
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}");

        builder.Append(string.Join("&", parts));
        return builder.ToString();
    }
}

