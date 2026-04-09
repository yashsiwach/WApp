namespace AuthService.Application.DTOs;

// ── Auth DTOs ──

public record SignupRequest
{
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = null!;
}

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public record UpdateUserStatusRequest
{
    public bool IsActive { get; init; }
}

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ── KYC DTOs ──

public record KYCSubmitRequest
{
    public string DocType { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
}

public record KYCStatusResponse
{
    public Guid DocumentId { get; init; }
    public string DocType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ReviewNotes { get; init; }
    public DateTime SubmittedAt { get; init; }
}

// ── OTP DTOs ──

public record OTPSendRequest
{
    public string Email { get; init; } = string.Empty;
}

public record OTPVerifyRequest
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
