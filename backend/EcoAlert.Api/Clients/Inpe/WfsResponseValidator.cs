namespace EcoAlerta.Api.Clients.Inpe;

internal static class WfsResponseValidator
{
    public static bool IsValidJsonResponse(string? mediaType, string payload)
    {
        var isJsonMediaType = !string.IsNullOrWhiteSpace(mediaType) &&
                              mediaType.Contains("json", StringComparison.OrdinalIgnoreCase);

        var startsWithJsonChar = payload.TrimStart().StartsWith("{", StringComparison.Ordinal) ||
                                 payload.TrimStart().StartsWith("[", StringComparison.Ordinal);

        return isJsonMediaType && startsWithJsonChar;
    }

    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }
}

