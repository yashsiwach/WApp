using System.Net;

namespace SharedContracts.Middleware;

/// <summary>
/// Strategy for translating a specific exception type to an HTTP status code and message.
/// Register additional implementations in DI to extend error handling without modifying middleware.
/// </summary>
public interface IExceptionHandler
{
    /// <summary>
    /// Determines whether this handler is capable of processing the given exception.
    /// </summary>
    bool CanHandle(Exception exception);
    /// <summary>
    /// Translates the exception into an HTTP status code and error message pair.
    /// </summary>
    (HttpStatusCode StatusCode, string Message) Handle(Exception exception);
}
