using Microsoft.OpenApi.Models;

namespace EcoAlerta.Api.Configuration;

public static class SwaggerConfiguration
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EcoAlerta API - Monitoramento de Queimadas em Goiás",
                Version = "v1",
                Description = "API REST para monitoramento de focos de queimadas no estado de Goiás."
            });
        });
    }

    public static void UseSwaggerDocumentation(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) return;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoAlerta API v1");
            options.RoutePrefix = string.Empty;
        });
    }
}

