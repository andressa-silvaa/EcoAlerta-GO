using Microsoft.AspNetCore.Mvc;

namespace EcoAlerta.Api.Validation;

public static class DateRangeValidator
{
    private const int MaxYearsBack = 5;

    public static ActionResult? ValidateDateRange(
        DateTime? startDate,
        DateTime? endDate,
        out DateTime? normalizedStart,
        out DateTime? normalizedEnd,
        bool validateCompleteRange = true)
    {
        normalizedStart = startDate?.Date;
        normalizedEnd = endDate?.Date;

        var basicError = ValidateDateOrder(normalizedStart, normalizedEnd);
        if (basicError != null)
        {
            return basicError;
        }

        if (!validateCompleteRange)
        {
            return null;
        }

        var today = DateTime.UtcNow.Date;
        var earliestAllowed = today.AddYears(-MaxYearsBack);

        if (normalizedStart.HasValue && normalizedStart < earliestAllowed)
        {
            return CreateBadRequest($"Data de início não pode ser anterior a {earliestAllowed:dd/MM/yyyy}");
        }

        if (normalizedStart.HasValue && normalizedStart > today)
        {
            return CreateBadRequest("Data de início não pode ser futura");
        }

        if (normalizedEnd.HasValue && normalizedEnd > today)
        {
            return CreateBadRequest("Data de fim não pode ser futura");
        }

        return null;
    }

    private static ActionResult? ValidateDateOrder(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            return CreateBadRequest("Data de início deve ser anterior à data de fim");
        }

        return null;
    }

    private static BadRequestObjectResult CreateBadRequest(string message)
    {
        return new BadRequestObjectResult(new { message });
    }
}

