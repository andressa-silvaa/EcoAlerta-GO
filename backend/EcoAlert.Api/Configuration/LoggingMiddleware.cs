namespace EcoAlerta.Api.Configuration;

public static class LoggingMiddleware
{
    public static void UseRequestLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            logger.LogInformation(
                "Request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await next();

            logger.LogInformation(
                "Response: {StatusCode} for {Method} {Path}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path);
        });
    }
}

