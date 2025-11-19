using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EcoAlerta.Api.Data;
using EcoAlerta.Api.Services;
using EcoAlerta.Api.Clients;
using EcoAlerta.Api.Middleware;
using EcoAlerta.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configuração do JSON serializer para usar camelCase (padrão JavaScript/frontend)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializa propriedades em camelCase para compatibilidade com frontend JavaScript
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true; // JSON formatado para melhor leitura
    });

builder.Services.Configure<InpeApiOptions>(builder.Configuration.GetSection("InpeApi"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EcoAlerta API - Monitoramento de Queimadas em Goiás",
        Version = "v1",
        Description = "API REST para monitoramento de focos de queimadas no estado de Goiás. " +
                      "Sistema acadêmico desenvolvido com Web Services para gestão ambiental."
    });
});

// Configuração do Entity Framework Core
// NOTA: Esta connection string pode ser configurada para apontar para um banco remoto gratuito:
// - MongoDB Atlas: mongodb+srv://user:pass@cluster.mongodb.net/ecoalerta
// - Railway: connection string fornecida pelo Railway
// - Render: connection string fornecida pelo Render
// - Supabase: connection string PostgreSQL fornecida pelo Supabase
// Por enquanto, usando InMemory para desenvolvimento
builder.Services.AddDbContext<EcoAlertaDbContext>(options =>
{
    // Para desenvolvimento: banco em memória
    options.UseInMemoryDatabase("EcoAlertaDb");
    
    // Para produção com banco remoto, descomente e configure:
    // var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    // options.UseSqlServer(connectionString); // Para SQL Server
    // ou
    // options.UseNpgsql(connectionString); // Para PostgreSQL (Supabase)
    // ou use MongoDB.Driver para MongoDB Atlas
});

// Configuração de CORS para permitir requisições do frontend React
// Em produção, especifique apenas as origens permitidas para maior segurança
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:5174") // Vite e Create React App
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("X-Total-Count"); // Expor headers customizados se necessário
    });
});

// Configuração de rate limiting básico (proteção contra abuso)
builder.Services.AddMemoryCache();

// Injeção de dependência - HttpClient para o cliente da API do INPE
builder.Services.AddHttpClient<IInpeApiClient, InpeApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<InpeApiOptions>>().Value;
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }

    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds);
});

// Registro dos serviços de negócio
builder.Services.AddScoped<IQueimadaService, QueimadaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Middleware de tratamento de exceções global (deve ser o primeiro)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoAlerta API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

// Habilita CORS (deve vir antes de UseAuthorization e UseHttpsRedirection)
app.UseCors("AllowReactApp");

// Headers de segurança
app.Use(async (context, next) =>
{
    // Adiciona headers de segurança básicos
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
