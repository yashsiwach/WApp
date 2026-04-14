using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Defines the contract for authentication and user-management operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and returns tokens on success.
    /// </summary>
    Task<AuthResponse> SignupAsync(SignupRequest request);
    /// <summary>
    /// Validates credentials and returns a fresh token pair.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);
    /// <summary>
    /// Exchanges a valid refresh token for a new access/refresh token pair.
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    /// <summary>
    /// Revokes the given refresh token to invalidate the user's session.
    /// </summary>
    Task LogoutAsync(Guid userId, string refreshToken);
    /// <summary>
    /// Returns a list of all registered users for administrative views.
    /// </summary>
    Task<List<UserDto>> GetUsersAsync();
    /// <summary>
    /// Activates or blocks a user account and returns the updated profile.
    /// </summary>
    Task<UserDto> SetUserStatusAsync(Guid userId, bool isActive);
    /// <summary>
    /// Resolves a user's unique identifier from their email address.
    /// </summary>
    Task<Guid?> GetUserIdByEmailAsync(string email);
}
