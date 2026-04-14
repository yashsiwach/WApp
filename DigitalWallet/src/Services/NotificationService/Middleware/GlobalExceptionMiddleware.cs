using System.Text.Json;
using SharedContracts.DTOs;
using SharedContracts.Middleware;

namespace NotificationService.Middleware;

/// <summary>
/// ASP.NET Core middleware that catches unhandled exceptions and returns structured JSON error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IExceptionHandler> _handlers;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes the middleware with the next delegate, registered exception handlers, and a logger.
    /// </summary>
    public GlobalExceptionMiddleware(
        RequestDelegate next,
        IEnumerable<IExceptionHandler> handlers,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next     = next;
        _handlers = handlers;
        _logger   = logger;
    }

    /// <summary>
    /// Processes the HTTP request and intercepts any unhandled exception to produce an error response.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Delegates to the appropriate IExceptionHandler, sets the HTTP status code, and writes a JSON error body.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var handler = _handlers.First(h => h.CanHandle(exception));
        var (statusCode, message) = handler.Handle(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, new List<string> { exception.Message });
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
