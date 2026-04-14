using System.Text.Json;
using SharedContracts.DTOs;
using SharedContracts.Middleware;

namespace WalletService.Middleware;

/// <summary>
/// ASP.NET Core middleware that intercepts unhandled exceptions and returns a structured JSON error response.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IExceptionHandler> _handlers;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes the middleware with the next delegate, a collection of typed exception handlers, and a logger.
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
    /// Passes the request down the pipeline and catches any unhandled exceptions for structured error handling.
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
    /// Selects the appropriate exception handler, sets the HTTP status code, and writes the JSON error body.
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
