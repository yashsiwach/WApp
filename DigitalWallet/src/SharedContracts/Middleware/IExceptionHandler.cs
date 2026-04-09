using System.Net;

namespace SharedContracts.Middleware;

/// <summary>
/// Strategy for translating a specific exception type to an HTTP status code and message.
/// Register additional implementations in DI to extend error handling without modifying middleware.
/// </summary>
public interface IExceptionHandler
{
    bool CanHandle(Exception exception);
    (HttpStatusCode StatusCode, string Message) Handle(Exception exception);
}
