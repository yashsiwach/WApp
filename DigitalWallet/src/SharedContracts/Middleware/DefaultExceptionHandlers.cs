using System.Net;

namespace SharedContracts.Middleware;

/// <summary>
/// Handles UnauthorizedAccessException and maps it to HTTP 401 Unauthorized.
/// </summary>
public sealed class UnauthorizedExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Returns true when the exception is an UnauthorizedAccessException.
    /// </summary>
    public bool CanHandle(Exception exception) => exception is UnauthorizedAccessException;
    /// <summary>
    /// Returns HTTP 401 Unauthorized with the exception message.
    /// </summary>
    public (HttpStatusCode, string) Handle(Exception exception) =>
        (HttpStatusCode.Unauthorized, exception.Message);
}

/// <summary>
/// Handles InvalidOperationException and maps it to HTTP 400 Bad Request.
/// </summary>
public sealed class InvalidOperationExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Returns true when the exception is an InvalidOperationException.
    /// </summary>
    public bool CanHandle(Exception exception) => exception is InvalidOperationException;
    /// <summary>
    /// Returns HTTP 400 Bad Request with the exception message.
    /// </summary>
    public (HttpStatusCode, string) Handle(Exception exception) =>
        (HttpStatusCode.BadRequest, exception.Message);
}

/// <summary>
/// Handles KeyNotFoundException and maps it to HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Returns true when the exception is a KeyNotFoundException.
    /// </summary>
    public bool CanHandle(Exception exception) => exception is KeyNotFoundException;
    /// <summary>
    /// Returns HTTP 404 Not Found with the exception message.
    /// </summary>
    public (HttpStatusCode, string) Handle(Exception exception) =>
        (HttpStatusCode.NotFound, exception.Message);
}

/// <summary>Catch-all handler — always registered last.</summary>
public sealed class FallbackExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => true;
    public (HttpStatusCode, string) Handle(Exception exception) =>
        (HttpStatusCode.InternalServerError, "An unexpected error occurred.");
}
