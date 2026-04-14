using System.Text.Json;
using SharedContracts.DTOs;
using SharedContracts.Middleware;

namespace AuthService.Middleware;

/// <summary>
/// ASP.NET Core middleware that catches unhandled exceptions and returns a structured JSON error response.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IExceptionHandler> _handlers;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes the middleware with the pipeline delegate, registered exception handlers, and a logger.
    /// </summary>
    public GlobalExceptionMiddleware(RequestDelegate next,IEnumerable<IExceptionHandler> handlers,ILogger<GlobalExceptionMiddleware> logger)
    {
        _next     = next;
        _handlers = handlers;
        _logger   = logger;
    }

    /// <summary>
    /// Executes the next middleware and intercepts any unhandled exception for structured error handling.
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
    /// Maps the exception to an HTTP status code and error message via registered handlers, then writes the JSON response.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Select the first registered handler that knows how to process this exception type
        var handler = _handlers.First(h => h.CanHandle(exception));
        var (statusCode, message) = handler.Handle(exception);

        // Set response headers and status code before writing the body
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, new List<string> { exception.Message });
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Write the serialized error payload to the response stream
        await context.Response.WriteAsync(json);
    }
}
