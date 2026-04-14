using System.Text.Json;
using SharedContracts.DTOs;
using SharedContracts.Middleware;

namespace SupportTicketService.Middleware;

/// <summary>
/// ASP.NET Core middleware that catches unhandled exceptions, delegates to registered handlers, and writes a structured JSON error response.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IExceptionHandler> _handlers;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes the middleware with the next pipeline delegate, exception handlers, and a logger.
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
    /// Executes the next middleware step and intercepts any unhandled exception for structured error handling.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Resolves the appropriate handler for the exception, sets the HTTP status code, and writes the JSON error payload.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var handler = _handlers.First(h => h.CanHandle(exception));
        var (statusCode, message) = handler.Handle(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, new List<string> { exception.Message });
        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
