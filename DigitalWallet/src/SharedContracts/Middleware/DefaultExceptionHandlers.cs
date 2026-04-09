using System.Net;

namespace SharedContracts.Middleware;

public sealed class UnauthorizedExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is UnauthorizedAccessException;
    public (HttpStatusCode, string) Handle(Exception exception) =>
        (HttpStatusCode.Unauthorized, exception.Message);
}

public sealed class InvalidOperationExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is InvalidOperationException;
    public (HttpStatusCode, string) Handle(Exception exception) =>
        (HttpStatusCode.BadRequest, exception.Message);
}

public sealed class NotFoundExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is KeyNotFoundException;
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
