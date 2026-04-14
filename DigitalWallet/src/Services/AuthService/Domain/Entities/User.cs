namespace AuthService.Domain.Entities;

/// <summary>
/// Core user account entity representing a registered participant in the wallet platform.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user account.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Unique email address used for authentication and notifications.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Unique phone number associated with the account.
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>
    /// BCrypt-hashed password for secure credential storage.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>
    /// Access role assigned to the user: User, Admin, or SupportAgent.
    /// </summary>
    public string Role { get; set; } = "User"; // User, Admin, SupportAgent
    /// <summary>
    /// Indicates whether the account is active and permitted to log in.
    /// </summary>
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Soft-delete flag; true means the account is logically removed.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    /// <summary>
    /// UTC timestamp when the user account was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the most recent modification to the account.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Collection of OTP log entries associated with this user.
    /// </summary>
    public ICollection<OTPLog> OTPLogs { get; set; } = new List<OTPLog>();
    /// <summary>
    /// Collection of KYC documents submitted by this user.
    /// </summary>
    public ICollection<KYCDocument> KYCDocuments { get; set; } = new List<KYCDocument>();
    /// <summary>
    /// Collection of refresh tokens issued to this user.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
