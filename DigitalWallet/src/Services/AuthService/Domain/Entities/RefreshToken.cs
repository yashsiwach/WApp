namespace AuthService.Domain.Entities;

/// <summary>
/// Represents a long-lived refresh token issued to a user for obtaining new access tokens.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Unique identifier for this refresh token record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the user this token was issued to.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// The opaque token string presented by the client during refresh.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp after which this token can no longer be used.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>
    /// Indicates whether this token has been explicitly revoked (e.g., on logout).
    /// </summary>
    public bool Revoked { get; set; } = false;
    /// <summary>
    /// UTC timestamp when this token was issued.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>
    /// Navigation property to the user this token belongs to.
    /// </summary>
    public User User { get; set; } = null!;
}
