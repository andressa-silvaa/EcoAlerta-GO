using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcoAlerta.Api.Configuration;
using EcoAlerta.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace EcoAlerta.Api.Clients;

/// <summary>
/// Cliente HTTP para integração com a API do INPE (Instituto Nacional de Pesquisas Espaciais).
/// Consome os dados reais públicos do Programa Queimadas e converte para o modelo interno.
/// Em caso de indisponibilidade da API do INPE, aplica um fallback controlado para dados mockados
/// a fim de manter o ambiente acadêmico funcional.
/// </summary>
public interface IInpeApiClient
{
    /// <summary>
    /// Obtém dados de focos de queimadas do INPE (ou mock).
    /// </summary>
    Task<List<Queimada>> ObterFocosQueimadasAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
}

public class InpeApiClient : IInpeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InpeApiClient> _logger;
    private readonly InpeApiOptions _options;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public InpeApiClient(
        HttpClient httpClient,
        ILogger<InpeApiClient> logger,
        IOptions<InpeApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Obtém focos de queimadas utilizando o endpoint público do INPE.
    /// </summary>
    public async Task<List<Queimada>> ObterFocosQueimadasAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var dataAtual = DateTime.UtcNow.Date;
        var periodoInicio = (dataInicio ?? dataAtual.AddDays(-30)).Date;
        var periodoFim = (dataFim ?? dataAtual).Date;

        if (periodoFim > dataAtual)
        {
            periodoFim = dataAtual;
        }

        if (periodoInicio > dataAtual)
        {
            periodoInicio = dataAtual;
        }

        if (periodoInicio > periodoFim)
        {
            (periodoInicio, periodoFim) = (periodoFim, periodoInicio);
        }

        try
        {
            var registros = await ConsultarWfsAsync(periodoInicio, periodoFim);

            if (!registros.Any())
            {
                _logger.LogWarning("API do INPE retornou 0 registros para Goiás entre {Inicio} e {Fim}. Ativando fallback para mock controlado.",
                    periodoInicio, periodoFim);
                return GerarDadosMock(periodoInicio, periodoFim);
            }

            var queimadas = new List<Queimada>(registros.Count);
            var idCounter = 1;

            foreach (var registro in registros)
            {
                var queimada = ConverterParaModeloInterno(registro, idCounter);
                if (queimada != null)
                {
                    queimadas.Add(queimada);
                    idCounter++;
                }
            }

            _logger.LogInformation(
                "API do INPE respondeu {Quantidade} focos para Goiás entre {Inicio} e {Fim}",
                queimadas.Count,
                periodoInicio,
                periodoFim);

            return queimadas;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro ao consultar API do INPE");
            return GerarDadosMock(periodoInicio, periodoFim);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout ao consultar API do INPE");
            return GerarDadosMock(periodoInicio, periodoFim);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao desserializar resposta do INPE");
            return GerarDadosMock(periodoInicio, periodoFim);
        }
    }

    private async Task<List<WfsFeatureProperties>> ConsultarWfsAsync(DateTime periodoInicio, DateTime periodoFim)
    {
        var resultados = new List<WfsFeatureProperties>();
        var anos = Enumerable.Range(
            periodoInicio.Year,
            periodoFim.Year - periodoInicio.Year + 1);
        var pageSize = _options.MaxFeatures > 0 ? _options.MaxFeatures : 10000;

        foreach (var ano in anos)
        {
            var anoInicio = ano == periodoInicio.Year ? periodoInicio : new DateTime(ano, 1, 1);
            var anoFim = ano == periodoFim.Year ? periodoFim : new DateTime(ano, 12, 31);
            var typeName = ResolveTypeNameForYear(ano);

            await BuscarIntervaloAsync(typeName, anoInicio, anoFim, pageSize, resultados);
        }

        return resultados;
    }

    private async Task BuscarIntervaloAsync(
        string typeName,
        DateTime dataInicio,
        DateTime dataFim,
        int pageSize,
        List<WfsFeatureProperties> acumulador)
    {
        if (dataInicio > dataFim)
        {
            return;
        }

        var requestUri = BuildWfsRequestUri(typeName, dataInicio, dataFim, pageSize);
        _logger.LogDebug("Consultando WFS (layer {Layer}) intervalo {Inicio:yyyy-MM-dd} a {Fim:yyyy-MM-dd}", typeName, dataInicio, dataFim);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        if (!string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogWarning("INPE retornou resposta vazia para layer {Layer} no intervalo {Inicio} - {Fim}.", typeName, dataInicio, dataFim);
            return;
        }

        if (!IsJsonResponse(response.Content.Headers.ContentType?.MediaType, responseText))
        {
            _logger.LogWarning("INPE retornou conteúdo não JSON para layer {Layer}. Intervalo {Inicio} - {Fim}. Conteúdo: {Preview}",
                typeName,
                dataInicio,
                dataFim,
                Truncate(responseText, 200));
            return;
        }

        var payload = JsonSerializer.Deserialize<WfsFeatureCollection>(responseText, _serializerOptions)
                      ?? new WfsFeatureCollection();

        if (!payload.Features.Any())
        {
            return;
        }

        var filtrados = payload.Features
            .Where(f => f?.Properties != null)
            .Select(f => f!.Properties!)
            .Where(p =>
                string.IsNullOrWhiteSpace(_options.EstadoFiltro) ||
                string.Equals(p.Estado, _options.EstadoFiltro, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var intervaloDias = (dataFim.Date - dataInicio.Date).TotalDays;
        var precisaDividir = payload.Features.Count >= pageSize && intervaloDias >= 1;

        if (precisaDividir)
        {
            var metadeDias = Math.Max(1, (int)Math.Floor(intervaloDias / 2));
            var pontoDeCorte = dataInicio.Date.AddDays(metadeDias);

            if (pontoDeCorte <= dataInicio || pontoDeCorte >= dataFim)
            {
                _logger.LogWarning("Limite do WFS atingido em um intervalo muito curto ({Inicio} - {Fim}). Mantendo os {Quantidade} registros retornados.", dataInicio, dataFim, filtrados.Count);
                acumulador.AddRange(filtrados);
                return;
            }

            _logger.LogInformation("Dividindo intervalo {Inicio} - {Fim} devido ao limite do WFS. Novo corte em {Corte:yyyy-MM-dd}.",
                dataInicio,
                dataFim,
                pontoDeCorte);

            await BuscarIntervaloAsync(typeName, dataInicio, pontoDeCorte, pageSize, acumulador);
            await BuscarIntervaloAsync(typeName, pontoDeCorte.AddDays(1), dataFim, pageSize, acumulador);
            return;
        }

        acumulador.AddRange(filtrados);
    }

    private string BuildWfsRequestUri(string typeName, DateTime dataInicio, DateTime dataFim, int pageSize)
    {
        var resource = _options.Resource?.Trim('/') ?? "wfs";
        var filtros = new List<string>
        {
            $"data_pas BETWEEN '{dataInicio:yyyy-MM-dd}' AND '{dataFim:yyyy-MM-dd}'"
        };

        if (!string.IsNullOrWhiteSpace(_options.EstadoFiltro))
        {
            filtros.Add($"estado = '{_options.EstadoFiltro}'");
        }

        if (!string.IsNullOrWhiteSpace(_options.DefaultPais))
        {
            filtros.Add($"pais = '{_options.DefaultPais}'");
        }

        var query = new Dictionary<string, string?>
        {
            ["service"] = "WFS",
            ["version"] = "1.0.0",
            ["request"] = "GetFeature",
            ["typeName"] = typeName,
            ["outputFormat"] = string.IsNullOrWhiteSpace(_options.OutputFormat)
                ? "application/json"
                : _options.OutputFormat,
            ["srsName"] = "EPSG:4326",
            ["propertyName"] = "latitude,longitude,data_hora_gmt,data_pas,municipio,estado,pais,satelite,frp,foco_id",
            ["maxFeatures"] = pageSize.ToString(CultureInfo.InvariantCulture),
            ["CQL_FILTER"] = string.Join(" AND ", filtros)
        };

        var builder = new StringBuilder();
        builder.Append(resource);
        builder.Append('?');
        builder.Append(string.Join("&", query
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}")));

        return builder.ToString();
    }

    private static bool IsJsonResponse(string? mediaType, string payload)
    {
        var isJsonMediaType = !string.IsNullOrWhiteSpace(mediaType) &&
                              mediaType.Contains("json", StringComparison.OrdinalIgnoreCase);

        var startsWithJsonChar = payload.TrimStart().StartsWith("{", StringComparison.Ordinal) ||
                                 payload.TrimStart().StartsWith("[", StringComparison.Ordinal);

        return isJsonMediaType && startsWithJsonChar;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }

    private string ResolveTypeNameForYear(int ano)
    {
        var currentYear = DateTime.UtcNow.Year;

        if (ano >= currentYear)
        {
            return string.IsNullOrWhiteSpace(_options.CurrentYearLayer)
                ? "dados_abertos:focos_ano_atual_br_todosats"
                : _options.CurrentYearLayer;
        }

        var layerTemplate = string.IsNullOrWhiteSpace(_options.LayerTemplate)
            ? "dados_abertos:focos_{0}_br_todosats"
            : _options.LayerTemplate;

        return string.Format(CultureInfo.InvariantCulture, layerTemplate, ano);
    }

    private Queimada? ConverterParaModeloInterno(WfsFeatureProperties registro, int id)
    {
        if (registro.Latitude == null || registro.Longitude == null)
        {
            return null;
        }

        var dataHora = registro.DataHoraGmt ?? registro.DataPas ?? DateTime.UtcNow;

        return new Queimada
        {
            Id = id,
            DataHora = DateTime.SpecifyKind(dataHora, DateTimeKind.Utc),
            Municipio = string.IsNullOrWhiteSpace(registro.Municipio) ? "Não informado" : registro.Municipio,
            Estado = _options.DefaultEstado,
            Latitude = (decimal)registro.Latitude.Value,
            Longitude = (decimal)registro.Longitude.Value,
            Intensidade = registro.Frp.HasValue ? (decimal)registro.Frp.Value : null,
            FonteSatelite = registro.Satelite,
            DataCriacao = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gera dados mockados simulando a resposta da API do INPE.
    /// Em produção, este método não existiria - os dados viriam da API real.
    /// </summary>
    private List<Queimada> GerarDadosMock(DateTime dataInicio, DateTime dataFim)
    {
        _logger.LogWarning("Utilizando fallback de dados mockados para garantir disponibilidade do sistema acadêmico.");

        var random = new Random();
        var queimadas = new List<Queimada>();
        var municipiosGoias = new[]
        {
            "Goiânia", "Aparecida de Goiânia", "Anápolis", "Rio Verde", "Luziânia",
            "Águas Lindas de Goiás", "Valparaíso de Goiás", "Trindade", "Formosa", "Novo Gama",
            "Senador Canedo", "Catalão", "Jataí", "Itumbiara", "Santo Antônio do Descoberto"
        };

        var dataAtual = dataInicio;
        int idCounter = 1;

        while (dataAtual <= dataFim)
        {
            // Gera entre 0 e 15 focos por dia (simulando variação sazonal)
            int focosNoDia = random.Next(0, 16);

            for (int i = 0; i < focosNoDia; i++)
            {
                var municipio = municipiosGoias[random.Next(municipiosGoias.Length)];
                
                // Coordenadas aproximadas de Goiás (centro-oeste do Brasil)
                // Latitude: -16 a -18, Longitude: -48 a -51
                var latitude = (decimal)(-16.0 - random.NextDouble() * 2.0);
                var longitude = (decimal)(-48.0 - random.NextDouble() * 3.0);

                var queimada = new Queimada
                {
                    Id = idCounter++,
                    DataHora = dataAtual.AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60)),
                    Municipio = municipio,
                    Estado = _options.DefaultEstado,
                    Latitude = latitude,
                    Longitude = longitude,
                    Intensidade = (decimal)(random.NextDouble() * 100), // 0 a 100
                    FonteSatelite = new[] { "AQUA", "TERRA", "NOAA", "SUOMI" }[random.Next(4)],
                    DataCriacao = DateTime.UtcNow
                };

                queimadas.Add(queimada);
            }

            dataAtual = dataAtual.AddDays(1);
        }

        return queimadas;
    }

    private sealed class WfsFeatureCollection
    {
        [JsonPropertyName("features")]
        public List<WfsFeature> Features { get; set; } = new();
    }

    private sealed class WfsFeature
    {
        [JsonPropertyName("properties")]
        public WfsFeatureProperties? Properties { get; set; }
    }

    private sealed class WfsFeatureProperties
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
}

