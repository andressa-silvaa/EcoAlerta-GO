using EcoAlerta.Api.Services;
using EcoAlerta.Api.Clients;
using EcoAlerta.Api.Middleware;
using EcoAlerta.Api.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "AllowReactApp";

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.Configure<InpeApiOptions>(builder.Configuration.GetSection(nameof(InpeApiOptions)));

var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

var allowAnyOrigin = configuredOrigins is { Length: 1 } && configuredOrigins[0] == "*";

string[] allowedOrigins;
if (allowAnyOrigin)
{
    allowedOrigins = Array.Empty<string>();
}
else if (configuredOrigins is { Length: > 0 })
{
    allowedOrigins = configuredOrigins;
}
else
{
    allowedOrigins = new[]
    {
        "http://localhost:5173",
        "http://localhost:3000",
        "http://localhost:5174"
    };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowAnyOrigin)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowCredentials();
        }

        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Total-Count");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EcoAlerta API - Monitoramento de Queimadas em Goiás",
        Version = "v1",
        Description = "API REST para monitoramento de focos de queimadas no estado de Goiás."
    });
});

builder.Services.AddHttpClient<IInpeApiClient, InpeApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<InpeApiOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }

    var timeout = options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 30;
    client.Timeout = TimeSpan.FromSeconds(timeout);
});

builder.Services.AddScoped<IQueimadaService, QueimadaService>();

var app = builder.Build();

// CORS deve vir antes de outros middlewares
app.UseCors(CorsPolicyName);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoAlerta API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Middleware de log para depuração
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Requisição recebida: {Method} {Path}", context.Request.Method, context.Request.Path);
    
    await next();
    
    logger.LogInformation("Resposta enviada: {StatusCode} para {Method} {Path}", 
        context.Response.StatusCode, context.Request.Method, context.Request.Path);
});

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["X-XSS-Protection"] = "1; mode=block";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    await next();
});

// Comentado para permitir acesso HTTP em desenvolvimento
// Em produção, use HTTPS e descomente esta linha
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
