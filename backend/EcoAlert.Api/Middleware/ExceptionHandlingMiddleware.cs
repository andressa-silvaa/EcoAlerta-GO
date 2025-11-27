using System.Net;
using System.Text.Json;

namespace EcoAlerta.Api.Middleware;

/// <summary>
/// Middleware global para tratamento centralizado de exceções.
/// 
/// Este middleware captura todas as exceções não tratadas e retorna
/// respostas HTTP padronizadas, melhorando a segurança ao não expor
/// detalhes internos do sistema em produção.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado capturado pelo middleware");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse
        {
            Message = "Ocorreu um erro ao processar sua requisição"
        };

        switch (exception)
        {
            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = argEx.Message;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Acesso não autorizado";
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "Recurso não encontrado";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                if (_environment.IsDevelopment())
                {
                    errorResponse.Message = exception.Message;
                    errorResponse.StackTrace = exception.StackTrace;
                }
                break;
        }

        _logger.LogError(exception, "Erro processado: {Message}", errorResponse.Message);

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
}

