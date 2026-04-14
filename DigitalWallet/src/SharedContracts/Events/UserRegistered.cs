namespace SharedContracts.Events;

/// <summary>
/// Event raised when a new user account is successfully registered.
/// </summary>
public record UserRegistered
{
    /// <summary>
    /// The unique identifier assigned to the newly registered user.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// The email address of the registered user.
    /// </summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>
    /// The phone number of the registered user.
    /// </summary>
    public string Phone { get; init; } = string.Empty;
    /// <summary>
    /// The role assigned to the user upon registration.
    /// </summary>
    public string Role { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the registration occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
