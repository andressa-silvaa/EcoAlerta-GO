namespace EcoAlerta.Api.Configuration;


public class InpeApiOptions
{

    public string BaseUrl { get; set; } = "https://terrabrasilis.dpi.inpe.br/queimadas/geoserver/";

    public string Resource { get; set; } = "wfs";

    public string LayerTemplate { get; set; } = "dados_abertos:focos_{0}_br_todosats";

    public string CurrentYearLayer { get; set; } = "dados_abertos:focos_ano_atual_br_todosats";

    public string DefaultPais { get; set; } = "Brasil";

    public string DefaultEstado { get; set; } = "GO";

    public string EstadoFiltro { get; set; } = "GOIÁS";

    public int TimeoutSeconds { get; set; } = 90;

    public string? ApiToken { get; set; }

    public int MaxFeatures { get; set; } = 10000;

    public string OutputFormat { get; set; } = "application/json";
}

