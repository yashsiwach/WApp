namespace AuthService.Domain.Entities;

/// <summary>
/// Audit record for a generated one-time password, tracking its lifecycle.
/// </summary>
public class OTPLog
{
    /// <summary>
    /// Unique identifier for this OTP log entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Optional identifier of the associated user account, if one exists.
    /// </summary>
    public Guid? UserId { get; set; }
    /// <summary>
    /// The email address the OTP was sent to.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// The numeric or alphanumeric OTP code that was issued.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp after which the OTP is no longer valid.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>
    /// UTC timestamp when the OTP was consumed; null if not yet used.
    /// </summary>
    public DateTime? UsedAt { get; set; }
    /// <summary>
    /// UTC timestamp when this OTP log entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>
    /// Optional navigation property to the associated user entity.
    /// </summary>
    public User? User { get; set; }
}
