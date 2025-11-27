using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using EcoAlerta.Api.Configuration;
using EcoAlerta.Api.Models;
using EcoAlerta.Api.Clients.Inpe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoAlerta.Api.Clients;

public interface IInpeApiClient
{
    Task<List<Queimada>> ObterFocosQueimadasAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
}

public class InpeApiClient : IInpeApiClient
{
    private const int DefaultDaysBack = 30;

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

    public async Task<List<Queimada>> ObterFocosQueimadasAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var (start, end) = NormalizeDateRange(dataInicio, dataFim);

        try
        {
            var records = await FetchWfsDataAsync(start, end);

            if (!records.Any())
            {
                _logger.LogWarning(
                    "INPE API returned 0 records for period {Start} to {End}. Using mock fallback.",
                    start, end);
                return MockDataGenerator.Generate(start, end, _options);
            }

            return MapToQueimadas(records);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "Error fetching data from INPE API");
            return MockDataGenerator.Generate(start, end, _options);
        }
    }

    private (DateTime start, DateTime end) NormalizeDateRange(DateTime? startDate, DateTime? endDate)
    {
        var today = DateTime.UtcNow.Date;
        var start = (startDate ?? today.AddDays(-DefaultDaysBack)).Date;
        var end = (endDate ?? today).Date;

        if (end > today) end = today;
        if (start > today) start = today;
        if (start > end) (start, end) = (end, start);

        return (start, end);
    }

    private List<Queimada> MapToQueimadas(List<WfsFeatureProperties> records)
    {
        var queimadas = new List<Queimada>(records.Count);
        var idCounter = 1;

        foreach (var record in records)
        {
            var queimada = QueimadaMapper.MapToQueimada(record, idCounter, _options);
            if (queimada != null)
            {
                queimadas.Add(queimada);
                idCounter++;
            }
        }

        return queimadas;
    }

    private async Task<List<WfsFeatureProperties>> FetchWfsDataAsync(DateTime startDate, DateTime endDate)
    {
        var results = new List<WfsFeatureProperties>();
        var years = Enumerable.Range(startDate.Year, endDate.Year - startDate.Year + 1);
        var pageSize = _options.MaxFeatures > 0 ? _options.MaxFeatures : 10000;

        foreach (var year in years)
        {
            var yearStart = year == startDate.Year ? startDate : new DateTime(year, 1, 1);
            var yearEnd = year == endDate.Year ? endDate : new DateTime(year, 12, 31);
            var typeName = ResolveLayerName(year);

            await FetchIntervalAsync(typeName, yearStart, yearEnd, pageSize, results);
        }

        return results;
    }

    private async Task FetchIntervalAsync(
        string typeName,
        DateTime startDate,
        DateTime endDate,
        int pageSize,
        List<WfsFeatureProperties> accumulator)
    {
        if (startDate > endDate) return;

        var requestUri = WfsQueryBuilder.BuildRequestUri(typeName, startDate, endDate, pageSize, _options);

        using var request = CreateHttpRequest(requestUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();

        if (!ValidateResponse(responseText, response.Content.Headers.ContentType?.MediaType, typeName, startDate, endDate))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<WfsFeatureCollection>(responseText, _serializerOptions)
                      ?? new WfsFeatureCollection();

        if (!payload.Features.Any()) return;

        var filtered = FilterFeaturesByState(payload.Features);
        var shouldSplit = payload.Features.Count >= pageSize && (endDate.Date - startDate.Date).TotalDays >= 1;

        if (shouldSplit)
        {
            await SplitAndFetchIntervalAsync(typeName, startDate, endDate, pageSize, accumulator, filtered);
            return;
        }

        accumulator.AddRange(filtered);
    }

    private HttpRequestMessage CreateHttpRequest(string requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        if (!string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
        }
        return request;
    }

    private bool ValidateResponse(string responseText, string? mediaType, string layer, DateTime start, DateTime end)
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogWarning("Empty response from INPE for layer {Layer} ({Start} - {End})", layer, start, end);
            return false;
        }

        if (!WfsResponseValidator.IsValidJsonResponse(mediaType, responseText))
        {
            _logger.LogWarning(
                "Non-JSON response from INPE for layer {Layer}. Preview: {Preview}",
                layer,
                WfsResponseValidator.Truncate(responseText, 200));
            return false;
        }

        return true;
    }

    private List<WfsFeatureProperties> FilterFeaturesByState(List<WfsFeature> features)
    {
        return features
            .Where(f => f?.Properties != null)
            .Select(f => f!.Properties!)
            .Where(p =>
                string.IsNullOrWhiteSpace(_options.EstadoFiltro) ||
                string.Equals(p.Estado, _options.EstadoFiltro, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task SplitAndFetchIntervalAsync(
        string typeName,
        DateTime startDate,
        DateTime endDate,
        int pageSize,
        List<WfsFeatureProperties> accumulator,
        List<WfsFeatureProperties> currentResults)
    {
        var daysInInterval = (endDate.Date - startDate.Date).TotalDays;
        var halfDays = Math.Max(1, (int)Math.Floor(daysInInterval / 2));
        var splitPoint = startDate.Date.AddDays(halfDays);

        if (splitPoint <= startDate || splitPoint >= endDate)
        {
            _logger.LogWarning(
                "WFS limit reached in short interval ({Start} - {End}). Keeping {Count} records.",
                startDate, endDate, currentResults.Count);
            accumulator.AddRange(currentResults);
            return;
        }

        _logger.LogInformation(
            "Splitting interval {Start} - {End} at {Split} due to WFS limit.",
            startDate, endDate, splitPoint);

        await FetchIntervalAsync(typeName, startDate, splitPoint, pageSize, accumulator);
        await FetchIntervalAsync(typeName, splitPoint.AddDays(1), endDate, pageSize, accumulator);
    }

    private string ResolveLayerName(int year)
    {
        var currentYear = DateTime.UtcNow.Year;

        if (year >= currentYear)
        {
            return string.IsNullOrWhiteSpace(_options.CurrentYearLayer)
                ? "dados_abertos:focos_ano_atual_br_todosats"
                : _options.CurrentYearLayer;
        }

        var layerTemplate = string.IsNullOrWhiteSpace(_options.LayerTemplate)
            ? "dados_abertos:focos_{0}_br_todosats"
            : _options.LayerTemplate;

        return string.Format(System.Globalization.CultureInfo.InvariantCulture, layerTemplate, year);
    }
}

