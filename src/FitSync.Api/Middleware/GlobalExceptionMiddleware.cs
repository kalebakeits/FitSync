namespace FitSync.Api.Middleware;

using FitSync.Api.Exceptions;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionMiddleware> logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger
    )
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (ApiException ex)
        {
            this.logger.LogWarning(
                "API exception occurred: {Message} (Status: {StatusCode})",
                ex.Message,
                ex.StatusCode
            );
            await HandleApiExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleUnhandledExceptionAsync(context, ex);
        }
    }

    private static Task HandleApiExceptionAsync(HttpContext context, ApiException exception)
    {
        context.Response.StatusCode = exception.StatusCode;
        context.Response.ContentType = "application/json";
        return Task.CompletedTask;
    }

    private static Task HandleUnhandledExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        return Task.CompletedTask;
    }
}
