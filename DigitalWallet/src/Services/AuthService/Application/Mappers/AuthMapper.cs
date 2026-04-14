using AuthService.Application.DTOs;
using AuthService.Domain.Entities;

namespace AuthService.Application.Mappers;

/// <summary>Single responsibility: maps AuthService domain objects to DTOs.</summary>
public static class AuthMapper
{
    /// <summary>
    /// Maps a User domain entity to a UserDto for client consumption.
    /// </summary>
    public static UserDto ToDto(User user) => new()
    {
        Id        = user.Id,
        Email     = user.Email,
        Phone     = user.Phone,
        Role      = user.Role,
        IsActive  = user.IsActive,
        CreatedAt = user.CreatedAt
    };

    /// <summary>
    /// Maps a KYCDocument domain entity to a KYCStatusResponse DTO.
    /// </summary>
    public static KYCStatusResponse ToDto(KYCDocument doc) => new()
    {
        DocumentId  = doc.Id,
        DocType     = doc.DocType,
        Status      = doc.Status,
        ReviewNotes = doc.ReviewNotes,
        SubmittedAt = doc.SubmittedAt
    };

    /// <summary>
    /// Assembles a full AuthResponse from token data and a User entity.
    /// </summary>
    public static AuthResponse ToAuthResponse(string accessToken, string refreshToken, DateTime expiresAt, User user) => new()
    {
        AccessToken  = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt    = expiresAt,
        User         = ToDto(user)
    };
}
