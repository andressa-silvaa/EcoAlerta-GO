namespace EcoAlerta.Api.Configuration;

public static class CorsConfiguration
{
    private const string PolicyName = "AllowReactApp";

    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var (allowAnyOrigin, allowedOrigins) = GetAllowedOrigins(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
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
    }

    public static void UseCorsPolicy(this IApplicationBuilder app)
    {
        app.UseCors(PolicyName);
    }

    private static (bool allowAnyOrigin, string[] allowedOrigins) GetAllowedOrigins(IConfiguration configuration)
    {
        var configuredOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        var allowAnyOrigin = configuredOrigins is { Length: 1 } && configuredOrigins[0] == "*";

        if (allowAnyOrigin)
        {
            return (true, Array.Empty<string>());
        }

        if (configuredOrigins is { Length: > 0 })
        {
            return (false, configuredOrigins);
        }

        var defaultOrigins = new[]
        {
            "http://localhost:5173",
            "http://localhost:3000",
            "http://localhost:5174"
        };

        return (false, defaultOrigins);
    }
}

