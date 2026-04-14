namespace SharedContracts.DTOs;

/// <summary>
/// Standard API response wrapper for all endpoints.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// Human-readable message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// The response payload returned on success.
    /// </summary>
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful response with the given data and optional message.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    /// <summary>
    /// Creates a failure response with the given message and optional error list.
    /// </summary>
    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };
}
