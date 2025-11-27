using EcoAlerta.Api.Services;
using EcoAlerta.Api.Clients;
using EcoAlerta.Api.Middleware;
using EcoAlerta.Api.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app, app.Environment);

app.Run();

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers().AddJsonOptions();
    services.Configure<InpeApiOptions>(configuration.GetSection(nameof(InpeApiOptions)));

    services.AddCorsPolicy(configuration);
    services.AddSwaggerDocumentation();

    services.AddHttpClient<IInpeApiClient, InpeApiClient>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<InpeApiOptions>>().Value;
        ConfigureHttpClient(client, options);
    });

    services.AddScoped<IQueimadaService, QueimadaService>();
}

static void ConfigureHttpClient(HttpClient client, InpeApiOptions options)
{
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }

    var timeout = options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 30;
    client.Timeout = TimeSpan.FromSeconds(timeout);
}

static void ConfigureMiddleware(IApplicationBuilder app, IWebHostEnvironment environment)
{
    app.UseCorsPolicy();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSwaggerDocumentation(environment);
    app.UseRequestLogging();
    app.UseSecurityHeaders();

    app.UseAuthorization();
    app.UseRouting();
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}

public partial class Program { }
