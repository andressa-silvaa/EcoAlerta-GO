using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EcoAlerta.Api.Validation;

public static partial class MunicipioValidator
{
    private const int MaxLength = 200;

    [GeneratedRegex(@"[^a-zA-ZáàâãéèêíìîóòôõúùûçÁÀÂÃÉÈÊÍÌÎÓÒÔÕÚÙÛÇ\s\-'\.]", RegexOptions.Compiled)]
    private static partial Regex InvalidCharactersRegex();

    public static ActionResult? ValidateAndSanitize(
        string? municipio,
        out string? sanitized)
    {
        sanitized = null;

        if (string.IsNullOrWhiteSpace(municipio))
        {
            return null;
        }

        var trimmed = municipio.Trim();

        if (trimmed.Length > MaxLength)
        {
            return CreateBadRequest($"Nome do município não pode ter mais de {MaxLength} caracteres");
        }

        var normalized = InvalidCharactersRegex().Replace(trimmed, string.Empty);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return CreateBadRequest("Nome do município contém caracteres inválidos");
        }

        sanitized = normalized;
        return null;
    }

    private static BadRequestObjectResult CreateBadRequest(string message)
    {
        return new BadRequestObjectResult(new { message });
    }
}

