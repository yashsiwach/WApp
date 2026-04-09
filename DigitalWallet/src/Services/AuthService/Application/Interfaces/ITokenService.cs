using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

/// <summary>Responsible for generating access and refresh tokens — details hidden behind this interface.</summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT access token for the given user.</summary>
    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(User user);

    /// <summary>Generates a cryptographically random refresh token string.</summary>
    string GenerateRefreshToken();
}
