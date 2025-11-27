namespace EcoAlerta.Api.Configuration;

/// <summary>
/// Opções de configuração para consumo da API real do INPE.
/// </summary>
public class InpeApiOptions
{
    /// <summary>
    /// URL base da API do INPE (ex.: https://terrabrasilis.dpi.inpe.br/queimadas/geoserver/).
    /// </summary>
    public string BaseUrl { get; set; } = "https://terrabrasilis.dpi.inpe.br/queimadas/geoserver/";

    /// <summary>
    /// Recurso/endpoint principal (ex.: wfs, api/v1/focos etc.).
    /// </summary>
    public string Resource { get; set; } = "wfs";

    /// <summary>
    /// Template do layer WFS utilizado para consulta (utiliza o ano como placeholder).
    /// </summary>
    public string LayerTemplate { get; set; } = "dados_abertos:focos_{0}_br_todosats";

    /// <summary>
    /// Layer utilizado para o ano corrente (dados em atualização).
    /// </summary>
    public string CurrentYearLayer { get; set; } = "dados_abertos:focos_ano_atual_br_todosats";

    /// <summary>
    /// País padrão enviado ao INPE (mantido configurável para reuso futuro).
    /// </summary>
    public string DefaultPais { get; set; } = "Brasil";

    /// <summary>
    /// UF padrão filtrada na API. Neste projeto fixa em Goiás (GO).
    /// </summary>
    public string DefaultEstado { get; set; } = "GO";

    /// <summary>
    /// Nome completo do estado utilizado no filtro WFS (ex.: GOIÁS).
    /// </summary>
    public string EstadoFiltro { get; set; } = "GOIÁS";

    /// <summary>
    /// Timeout padrão das chamadas à API externa, em segundos.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 90;

    /// <summary>
    /// Token opcional de autenticação (caso o INPE exija em algum momento).
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>
    /// Quantidade máxima de registros retornados por requisição no WFS.
    /// </summary>
    public int MaxFeatures { get; set; } = 10000;

    /// <summary>
    /// Formato esperado para a resposta do WFS.
    /// </summary>
    public string OutputFormat { get; set; } = "application/json";
}

