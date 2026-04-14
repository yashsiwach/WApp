namespace AuthService.Application.DTOs;

// ── Auth DTOs ──

/// <summary>
/// Request payload for registering a new user account.
/// </summary>
public record SignupRequest
{
    /// <summary>
    /// The user's email address used as a unique login identifier.
    /// </summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>
    /// The user's phone number for account identification and contact.
    /// </summary>
    public string Phone { get; init; } = string.Empty;
    /// <summary>
    /// The plain-text password that will be hashed before storage.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Request payload for authenticating an existing user.
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// The email address of the user attempting to log in.
    /// </summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>
    /// The plain-text password to validate against the stored hash.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Response returned after a successful authentication or token refresh.
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// Short-lived JWT access token for authorizing API requests.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;
    /// <summary>
    /// Long-lived opaque token used to obtain a new access token.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp indicating when the access token expires.
    /// </summary>
    public DateTime ExpiresAt { get; init; }
    /// <summary>
    /// Basic profile information about the authenticated user.
    /// </summary>
    public UserDto User { get; init; } = null!;
}

/// <summary>
/// Request payload carrying the refresh token for issuing a new access token.
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// The opaque refresh token string previously issued at login.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// Request payload for toggling a user's active/blocked status.
/// </summary>
public record UpdateUserStatusRequest
{
    /// <summary>
    /// True to activate the user; false to block them.
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Lightweight user profile data transferred to clients.
/// </summary>
public record UserDto
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// The user's registered email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>
    /// The user's registered phone number.
    /// </summary>
    public string Phone { get; init; } = string.Empty;
    /// <summary>
    /// The role assigned to the user (e.g., User, Admin, SupportAgent).
    /// </summary>
    public string Role { get; init; } = string.Empty;
    /// <summary>
    /// Indicates whether the user account is currently active.
    /// </summary>
    public bool IsActive { get; init; }
    /// <summary>
    /// UTC timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

// ── KYC DTOs ──

/// <summary>
/// Request payload for submitting a KYC document for review.
/// </summary>
public record KYCSubmitRequest
{
    /// <summary>
    /// The type of identity document being submitted (e.g., Aadhaar, PAN, Passport).
    /// </summary>
    public string DocType { get; init; } = string.Empty;
    /// <summary>
    /// The URL of the uploaded document file.
    /// </summary>
    public string FileUrl { get; init; } = string.Empty;
}

/// <summary>
/// Response containing the current review status of a submitted KYC document.
/// </summary>
public record KYCStatusResponse
{
    /// <summary>
    /// Unique identifier of the KYC document record.
    /// </summary>
    public Guid DocumentId { get; init; }
    /// <summary>
    /// The type of identity document (e.g., Aadhaar, PAN, Passport).
    /// </summary>
    public string DocType { get; init; } = string.Empty;
    /// <summary>
    /// Current review status: Pending, Approved, or Rejected.
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// Optional notes left by the reviewer during approval or rejection.
    /// </summary>
    public string? ReviewNotes { get; init; }
    /// <summary>
    /// UTC timestamp when the document was submitted.
    /// </summary>
    public DateTime SubmittedAt { get; init; }
}

// ── OTP DTOs ──

/// <summary>
/// Request payload for triggering an OTP to be sent to an email address.
/// </summary>
public record OTPSendRequest
{
    /// <summary>
    /// The email address to which the OTP will be delivered.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Request payload for verifying an OTP code submitted by the user.
/// </summary>
public record OTPVerifyRequest
{
    /// <summary>
    /// The email address the OTP was sent to.
    /// </summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>
    /// The one-time password code to validate.
    /// </summary>
    public string Code { get; init; } = string.Empty;
}
