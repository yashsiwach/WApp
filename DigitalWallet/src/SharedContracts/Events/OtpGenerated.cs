namespace SharedContracts.Events;

/// <summary>
/// Event raised when a one-time password is generated for a user.
/// </summary>
public record OtpGenerated
{
    /// <summary>
    /// The email address to which the OTP will be delivered.
    /// </summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>
    /// The generated OTP code to be sent to the user.
    /// </summary>
    public string Code { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp after which the OTP is no longer valid.
    /// </summary>
    public DateTime ExpiresAt { get; init; }
    /// <summary>
    /// UTC timestamp when the OTP was generated.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
