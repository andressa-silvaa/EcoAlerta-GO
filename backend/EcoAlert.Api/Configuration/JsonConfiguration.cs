using System.Text.Json;

namespace EcoAlerta.Api.Configuration;

public static class JsonConfiguration
{
    public static void AddJsonOptions(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
        });
    }
}

